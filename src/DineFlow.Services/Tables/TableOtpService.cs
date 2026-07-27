using DineFlow.BusinessObjects.Auth;
using DineFlow.BusinessObjects.Tables;
using DineFlow.Repositories.Tables;
using DineFlow.Services.Common;
using DineFlow.Services.Realtime;

namespace DineFlow.Services.Tables;

public sealed class TableOtpService : ITableOtpService
{
    private readonly ITableManagementRepository _repository;
    private readonly IRealtimeNotificationService _realtimeNotificationService;

    public TableOtpService(
        ITableManagementRepository repository,
        IRealtimeNotificationService realtimeNotificationService)
    {
        _repository = repository;
        _realtimeNotificationService = realtimeNotificationService;
    }

    public async Task<IReadOnlyList<StaffTableOtpDto>> GetAsync(
        TableOtpFilter filter,
        string currentUserRole,
        CancellationToken cancellationToken = default)
    {
        EnsureCanView(currentUserRole);

        IEnumerable<DiningTable> query = await _repository.GetAllAsync(cancellationToken);

        if (filter.AreaId.HasValue)
        {
            query = query.Where(table => table.AreaId == filter.AreaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            string status = filter.Status.Trim();
            query = query.Where(table => table.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            string search = filter.Search.Trim();
            query = query.Where(table =>
                table.TableName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (table.AreaEntity?.AreaName ?? table.Area).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                table.CurrentOtp.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return query.Select(Map).ToList();
    }

    public async Task<StaffTableOtpDto> ResetAsync(
        int tableId,
        string currentUserRole,
        CancellationToken cancellationToken = default)
    {
        EnsureCanReset(currentUserRole);

        DiningTable table = await _repository.GetByIdAsync(tableId, cancellationToken)
            ?? throw new BusinessException("TABLE_NOT_FOUND", "Dining table does not exist.");

        Rotate(table);
        await _repository.SaveChangesAsync(cancellationToken);
        StaffTableOtpDto dto = Map(table);
        await NotifyTableOtpChangedAsync(dto, cancellationToken);
        return dto;
    }

    public async Task<IReadOnlyList<StaffTableOtpDto>> ResetBatchAsync(
        ResetTableOtpBatchRequest request,
        string currentUserRole,
        CancellationToken cancellationToken = default)
    {
        EnsureCanReset(currentUserRole);

        bool hasTableIds = request.TableIds.Count > 0;
        if (!hasTableIds && !request.AreaId.HasValue)
        {
            throw new BusinessException("OTP_RESET_TARGET_REQUIRED", "Choose tables or an area before resetting OTP.");
        }

        List<DiningTable> tables = await _repository.GetAllForUpdateAsync(cancellationToken);
        IEnumerable<DiningTable> target = tables;

        if (hasTableIds)
        {
            HashSet<int> ids = request.TableIds.ToHashSet();
            target = target.Where(table => ids.Contains(table.TableId));
        }
        else if (request.AreaId.HasValue)
        {
            target = target.Where(table => table.AreaId == request.AreaId.Value);
        }

        List<DiningTable> selected = target.ToList();
        if (selected.Count == 0)
        {
            throw new BusinessException("OTP_RESET_TARGET_EMPTY", "No dining tables matched the reset target.");
        }

        foreach (DiningTable table in selected)
        {
            Rotate(table);
        }

        await _repository.SaveChangesAsync(cancellationToken);
        List<StaffTableOtpDto> dtos = selected.Select(Map).ToList();
        foreach (StaffTableOtpDto dto in dtos)
        {
            await NotifyTableOtpChangedAsync(dto, cancellationToken);
        }

        return dtos;
    }

    public async Task RotateForClosedSessionAsync(int tableId, CancellationToken cancellationToken = default)
    {
        DiningTable? table = await _repository.GetByIdAsync(tableId, cancellationToken);
        if (table is null)
        {
            return;
        }

        Rotate(table);
        await _repository.SaveChangesAsync(cancellationToken);
        await NotifyTableOtpChangedAsync(Map(table), cancellationToken);
    }

    private async Task NotifyTableOtpChangedAsync(
        StaffTableOtpDto table,
        CancellationToken cancellationToken)
    {
        RealtimeEventDto payload = new()
        {
            TableSessionId = table.CurrentSessionId ?? 0,
            TableId = table.TableId,
            CurrentOtp = table.CurrentOtp,
            OtpUpdatedAt = table.OtpUpdatedAt,
            TableStatus = table.Status,
            SessionStatus = table.CurrentSessionStatus
        };

        await _realtimeNotificationService.NotifyStaffAsync(
            RealtimeEvents.TableOtpChanged,
            payload,
            cancellationToken);

        if (payload.TableSessionId > 0)
        {
            await _realtimeNotificationService.NotifySessionAsync(
                payload.TableSessionId,
                RealtimeEvents.TableOtpChanged,
                payload,
                cancellationToken);
        }
    }

    private static void Rotate(DiningTable table)
    {
        TableOtpRotation.Rotate(table);
    }

    private static StaffTableOtpDto Map(DiningTable table)
    {
        var currentSession = table.TableSessions
            .Where(session => session.Status == "Open" || session.Status == "WaitingPayment")
            .OrderByDescending(session => session.StartedAt)
            .FirstOrDefault();

        return new StaffTableOtpDto
        {
            TableId = table.TableId,
            TableName = table.TableName,
            AreaId = table.AreaId,
            Area = table.AreaEntity?.AreaName ?? table.Area,
            AreaDisplayOrder = table.AreaEntity?.DisplayOrder ?? int.MaxValue,
            TableDisplayOrder = table.DisplayOrder,
            Status = table.Status,
            CurrentOtp = table.CurrentOtp,
            OtpUpdatedAt = table.OtpUpdatedAt,
            CurrentSessionId = currentSession?.TableSessionId,
            CurrentSessionStatus = currentSession?.Status
        };
    }

    private static void EnsureCanView(string currentUserRole)
    {
        if (!AuthRoles.IsStaff(currentUserRole) &&
            !AuthRoles.IsAdmin(currentUserRole) &&
            !AuthRoles.IsOwner(currentUserRole))
        {
            throw new UnauthorizedAccessException("Không có quyền xem OTP bàn.");
        }
    }

    private static void EnsureCanReset(string currentUserRole)
    {
        if (!AuthRoles.IsAdmin(currentUserRole))
        {
            throw new UnauthorizedAccessException("Chỉ Admin được reset OTP bàn.");
        }
    }
}
