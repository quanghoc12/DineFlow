using DineFlow.BusinessObjects.Orders;
using DineFlow.BusinessObjects.Tables;
using DineFlow.Repositories.Bills;
using DineFlow.Repositories.Common;
using DineFlow.Repositories.Orders;
using DineFlow.Services.Common;
using DineFlow.Services.Realtime;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.Services.Orders;

public class TableSessionService : ITableSessionService
{
    private static readonly TimeSpan BrowsingLifetime = TimeSpan.FromMinutes(15);
    private readonly IBillRepository _billRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IRealtimeNotificationService _realtimeNotificationService;
    private readonly ITableSessionRepository _tableSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TableSessionService(
        IBillRepository billRepository,
        IOrderRepository orderRepository,
        IRealtimeNotificationService realtimeNotificationService,
        ITableSessionRepository tableSessionRepository,
        IUnitOfWork unitOfWork)
    {
        _billRepository = billRepository;
        _orderRepository = orderRepository;
        _realtimeNotificationService = realtimeNotificationService;
        _tableSessionRepository = tableSessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<DiningTableDto>> GetTablesAsync(
        DiningTableFilter filter,
        CancellationToken cancellationToken = default)
    {
        List<DiningTableDto> tables = (await _tableSessionRepository.GetDiningTablesAsync(
                filter.ActiveOnly,
                filter.Status,
                filter.Area,
                cancellationToken))
            .Select(table => new DiningTableDto
            {
                TableId = table.TableId,
                TableName = table.TableName,
                AreaId = table.AreaId,
                Area = table.AreaEntity?.AreaName ?? table.Area,
                AreaDisplayOrder = table.AreaEntity?.DisplayOrder ?? int.MaxValue,
                TableDisplayOrder = table.DisplayOrder,
                Status = table.Status,
                IsActive = table.IsActive,
                CurrentTableSessionId = table.TableSessions
                    .Where(session => session.Status == "Open" || session.Status == "WaitingPayment")
                    .OrderByDescending(session => session.StartedAt)
                    .Select(session => (int?)session.TableSessionId)
                    .FirstOrDefault(),
                CurrentSessionStatus = table.TableSessions
                    .Where(session => session.Status == "Open" || session.Status == "WaitingPayment")
                    .OrderByDescending(session => session.StartedAt)
                    .Select(session => session.Status)
                    .FirstOrDefault()
            })
            .ToList();

        return tables;
    }

    public async Task<TableSessionDto?> GetByIdAsync(int tableSessionId, CancellationToken cancellationToken = default)
    {
        TableSession? session = await _tableSessionRepository.GetByIdAsync(tableSessionId, cancellationToken);
        return session is null ? null : MapSession(session);
    }

    public async Task<TableSessionDto?> GetCurrentSessionByTableIdAsync(
        int tableId,
        CancellationToken cancellationToken = default)
    {
        TableSession? session = await _tableSessionRepository.GetCurrentByTableIdAsync(tableId, cancellationToken);
        return session is null ? null : MapSession(session);
    }

    public async Task<TableSessionDto> GetOrCreateActiveSessionByTableIdAsync(
        int tableId,
        int? openedBy,
        CancellationToken cancellationToken = default)
    {
        TableSession? existing = await _tableSessionRepository.GetCurrentByTableIdAsync(tableId, cancellationToken);

        if (existing is not null)
        {
            return MapSession(existing);
        }

        TableSession? browsing = await _tableSessionRepository.GetCurrentCustomerSessionByTableIdAsync(
            tableId,
            cancellationToken);
        if (browsing?.Status == "Browsing")
        {
            return await ActivateBrowsingSessionAsync(
                browsing.TableSessionId,
                openedBy ?? 0,
                cancellationToken);
        }

        TableSessionDto dto;
        try
        {
            dto = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                DiningTable table = await _tableSessionRepository.GetActiveTableByIdAsync(tableId, ct)
                    ?? throw new BusinessException("TABLE_NOT_FOUND", "Dining table does not exist or is inactive.");

                TableSession session = CreateSession(table.TableId, openedBy);
                table.Status = "Occupied";
                table.UpdatedAt = DateTime.UtcNow;

                await _tableSessionRepository.AddTableSessionAsync(session, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                return MapSession(session);
            }, cancellationToken);
        }
        catch (DbUpdateException) when (!cancellationToken.IsCancellationRequested)
        {
            TableSession? current = await _tableSessionRepository.GetCurrentCustomerSessionByTableIdAsync(tableId, cancellationToken);
            if (current is null)
            {
                throw;
            }

            dto = MapSession(current);
        }

        await NotifyTableSessionChangedAsync(dto.TableSessionId, dto.TableId, cancellationToken);
        return dto;
    }

    public async Task<TableSessionDto> GetOrCreateActiveSessionByQrTokenAsync(
        string qrToken,
        int? openedBy,
        CancellationToken cancellationToken = default)
    {
        await ExpireInactiveBrowsingSessionsAsync(cancellationToken);

        DiningTable table = await _tableSessionRepository.GetActiveTableByQrTokenAsync(qrToken, cancellationToken)
            ?? throw new BusinessException("TABLE_NOT_FOUND", "Dining table does not exist or is inactive.");

        TableSession? existing = await _tableSessionRepository.GetCurrentCustomerSessionByTableIdAsync(
            table.TableId,
            cancellationToken);
        if (existing is not null)
        {
            return MapSession(existing);
        }

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                TableSession session = new()
                {
                    TableId = table.TableId,
                    StartedAt = DateTime.UtcNow,
                    Status = "Browsing",
                    OpenedBy = openedBy
                };

                await _tableSessionRepository.AddTableSessionAsync(session, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                return MapSession(session);
            }, cancellationToken);
        }
        catch (DbUpdateException) when (!cancellationToken.IsCancellationRequested)
        {
            TableSession? current = await _tableSessionRepository.GetCurrentCustomerSessionByTableIdAsync(table.TableId, cancellationToken);
            if (current is null)
            {
                throw;
            }

            return MapSession(current);
        }
    }

    public async Task<TableSessionDto> ActivateBrowsingSessionAsync(
        int tableSessionId,
        int openedBy,
        CancellationToken cancellationToken = default)
    {
        TableSessionDto dto = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            TableSession session = await _tableSessionRepository.GetByIdAsync(tableSessionId, ct)
                ?? throw new BusinessException("SESSION_NOT_FOUND", "Table session does not exist.");

            if (session.Status is "Open" or "WaitingPayment")
            {
                return MapSession(session);
            }

            if (session.Status != "Browsing")
            {
                throw new BusinessException("SESSION_NOT_BROWSING", "Only a browsing session can be activated.");
            }

            session.Status = "Open";
            session.OpenedBy = openedBy > 0 ? openedBy : null;

            DiningTable table = await _tableSessionRepository.GetTableByIdAsync(session.TableId, ct)
                ?? throw new BusinessException("TABLE_NOT_FOUND", "Dining table does not exist.");
            table.Status = "Occupied";
            table.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);
            return MapSession(session);
        }, cancellationToken);

        await NotifyTableSessionChangedAsync(dto.TableSessionId, dto.TableId, cancellationToken);
        return dto;
    }

    public async Task<int> ExpireInactiveBrowsingSessionsAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TableSession> expired = await _tableSessionRepository.GetExpiredBrowsingSessionsAsync(
            DateTime.UtcNow.Subtract(BrowsingLifetime),
            cancellationToken);
        if (expired.Count == 0)
        {
            return 0;
        }

        DateTime expiredAt = DateTime.UtcNow;
        foreach (TableSession session in expired)
        {
            session.Status = "Expired";
            session.EndedAt = expiredAt;

            foreach (var request in session.ServiceRequests.Where(x => x.Status == "Pending"))
            {
                request.Status = "Expired";
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return expired.Count;
    }

    public async Task<TableSessionDto> MarkWaitingPaymentAsync(
        int tableSessionId,
        CancellationToken cancellationToken = default)
    {
        TableSessionDto dto = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            TableSession session = await _tableSessionRepository.GetByIdAsync(tableSessionId, ct)
                ?? throw new BusinessException("SESSION_NOT_FOUND", "Table session does not exist.");

            if (session.Status != "Open" && session.Status != "WaitingPayment")
            {
                throw new BusinessException("SESSION_NOT_ACTIVE", "Only active session can be marked waiting payment.");
            }

            session.Status = "WaitingPayment";
            DiningTable? table = await _tableSessionRepository.GetTableByIdAsync(session.TableId, ct);

            if (table is not null)
            {
                table.Status = "WaitingPayment";
                table.UpdatedAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync(ct);
            return MapSession(session);
        }, cancellationToken);

        await NotifyTableSessionChangedAsync(dto.TableSessionId, dto.TableId, cancellationToken);
        return dto;
    }

    public async Task<bool> CloseSessionIfCompletedAsync(
        int tableSessionId,
        int closedBy,
        CancellationToken cancellationToken = default)
    {
        bool closed = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            TableSession session = await _tableSessionRepository.GetByIdAsync(tableSessionId, ct)
                ?? throw new BusinessException("SESSION_NOT_FOUND", "Table session does not exist.");

            bool hasUnpaidBills = await _billRepository.HasUnpaidBillsAsync(tableSessionId, ct);

            if (hasUnpaidBills)
            {
                return false;
            }

            session.Status = "Closed";
            session.EndedAt = DateTime.UtcNow;
            session.ClosedBy = closedBy;

            DiningTable? table = await _tableSessionRepository.GetTableByIdAsync(session.TableId, ct);

            if (table is not null)
            {
                table.Status = "Available";
                table.UpdatedAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync(ct);
            return true;
        }, cancellationToken);

        if (closed)
        {
            await NotifyTableSessionChangedAsync(tableSessionId, null, cancellationToken);
        }

        return closed;
    }

    public async Task<IReadOnlyList<TableSessionDto>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TableSessionDto> sessions = (await _tableSessionRepository.GetActiveSessionsAsync(cancellationToken))
            .Select(MapSession)
            .ToList();

        return sessions;
    }

    public async Task<TableSessionDto> MoveTableAsync(
        int tableSessionId,
        MoveTableSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        TableSessionDto moved = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            TableSession session = await _tableSessionRepository.GetByIdAsync(tableSessionId, ct)
                ?? throw new BusinessException("SESSION_NOT_FOUND", "Table session does not exist.");

            if (session.Status != "Open" && session.Status != "WaitingPayment")
            {
                throw new BusinessException("SESSION_NOT_ACTIVE", "Only active session can be moved.");
            }

            if (session.TableId == request.TargetTableId)
            {
                throw new BusinessException("TABLE_MOVE_SAME_TABLE", "Target table must be different.");
            }

            DiningTable sourceTable = await _tableSessionRepository.GetTableByIdAsync(session.TableId, ct)
                ?? throw new BusinessException("SOURCE_TABLE_NOT_FOUND", "Source table does not exist.");

            DiningTable targetTable = await _tableSessionRepository.GetActiveTableByIdAsync(request.TargetTableId, ct)
                ?? throw new BusinessException("TARGET_TABLE_NOT_FOUND", "Target table does not exist or is inactive.");

            TableSession? targetSession = await _tableSessionRepository.GetCurrentByTableIdAsync(targetTable.TableId, ct);

            if (targetSession is not null)
            {
                throw new BusinessException("TARGET_TABLE_OCCUPIED", "Target table already has an active session.");
            }

            session.TableId = targetTable.TableId;
            sourceTable.Status = "Available";
            sourceTable.UpdatedAt = DateTime.UtcNow;
            targetTable.Status = session.Status == "WaitingPayment" ? "WaitingPayment" : "Occupied";
            targetTable.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);
            return MapSession(session);
        }, cancellationToken);

        await NotifyTableSessionChangedAsync(moved.TableSessionId, moved.TableId, cancellationToken);
        return moved;
    }

    public async Task<TableSessionDetailDto?> GetSessionDetailAsync(
        int tableSessionId,
        CancellationToken cancellationToken = default)
    {
        TableSession? session = await _tableSessionRepository.GetByIdAsync(tableSessionId, cancellationToken);

        if (session is null)
        {
            return null;
        }

        List<OrderSummaryDto> orders = (await _orderRepository.GetOrdersBySessionAsync(tableSessionId, cancellationToken))
            .Select(x => new OrderSummaryDto
            {
                OrderId = x.OrderId,
                OrderCode = x.OrderCode,
                TableSessionId = x.TableSessionId,
                OrderSource = x.OrderSource,
                Status = x.Status,
                PrintStatus = x.PrintStatus,
                CreatedAt = x.CreatedAt,
                ItemCount = x.OrderItems.Sum(item => item.Quantity)
            })
            .ToList();

        TableSessionDto dto = MapSession(session);
        return new TableSessionDetailDto
        {
            TableSessionId = dto.TableSessionId,
            TableId = dto.TableId,
            StartedAt = dto.StartedAt,
            EndedAt = dto.EndedAt,
            Status = dto.Status,
            Orders = orders
        };
    }

    private static TableSession CreateSession(int tableId, int? openedBy)
    {
        return new TableSession
        {
            TableId = tableId,
            StartedAt = DateTime.UtcNow,
            Status = "Open",
            OpenedBy = openedBy
        };
    }

    private static TableSessionDto MapSession(TableSession session)
    {
        return new TableSessionDto
        {
            TableSessionId = session.TableSessionId,
            TableId = session.TableId,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            Status = session.Status
        };
    }

    private async Task NotifyTableSessionChangedAsync(
        int tableSessionId,
        int? tableId,
        CancellationToken cancellationToken)
    {
        RealtimeEventDto payload = new()
        {
            TableSessionId = tableSessionId,
            TableId = tableId
        };

        await _realtimeNotificationService.NotifyStaffAsync(
            RealtimeEvents.TableSessionChanged,
            payload,
            cancellationToken);
        await _realtimeNotificationService.NotifySessionAsync(
            tableSessionId,
            RealtimeEvents.TableSessionChanged,
            payload,
            cancellationToken);
    }
}
