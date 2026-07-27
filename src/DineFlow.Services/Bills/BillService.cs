using System.Collections.Concurrent;
using System.Threading;
using DineFlow.BusinessObjects.Bills;
using DineFlow.BusinessObjects.Menu;
using DineFlow.BusinessObjects.Orders;
using DineFlow.Repositories.Bills;
using DineFlow.Repositories.Common;
using DineFlow.Repositories.Menu;
using DineFlow.Repositories.Orders;
using DineFlow.Services.Common;
using DineFlow.Services.Realtime;
using DineFlow.Services.Tables;

namespace DineFlow.Services.Bills;

public class BillService : IBillService
{
    private const string DefaultSalesChannelCode = "DINE_IN";
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> DefaultBillLocks = new();
    private readonly IBillRepository _billRepository;
    private readonly IMenuReadRepository _menuReadRepository;
    private readonly IMenuManagementRepository _menuManagementRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IRealtimeNotificationService _realtimeNotificationService;
    private readonly ITableSessionRepository _tableSessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BillService(
        IBillRepository billRepository,
        IMenuReadRepository menuReadRepository,
        IMenuManagementRepository menuManagementRepository,
        IOrderRepository orderRepository,
        IRealtimeNotificationService realtimeNotificationService,
        ITableSessionRepository tableSessionRepository,
        IUnitOfWork unitOfWork)
    {
        _billRepository = billRepository;
        _menuReadRepository = menuReadRepository;
        _menuManagementRepository = menuManagementRepository;
        _orderRepository = orderRepository;
        _realtimeNotificationService = realtimeNotificationService;
        _tableSessionRepository = tableSessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BillDto> GetOrCreateDefaultBillAsync(
        int tableSessionId,
        int? createdBy,
        CancellationToken cancellationToken = default)
    {
        SemaphoreSlim sessionLock = DefaultBillLocks.GetOrAdd(tableSessionId, _ => new SemaphoreSlim(1, 1));
        await sessionLock.WaitAsync(cancellationToken);
        try
        {
            Bill? existingBill = await _billRepository.GetDefaultUnpaidBillBySessionAsync(tableSessionId, cancellationToken);

            if (existingBill is not null)
            {
                return MapBill(existingBill);
            }

            BillDto dto = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                SalesChannel salesChannel = await ResolveSalesChannelByCodeAsync(DefaultSalesChannelCode, ct);
                Bill bill = await CreateDefaultBillAsync(tableSessionId, createdBy, salesChannel, ct);
                await _billRepository.AddBillAsync(bill, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                return MapBill(bill);
            }, cancellationToken);

            await NotifyBillChangedAsync(dto.TableSessionId, dto.BillId, cancellationToken);
            return dto;
        }
        finally
        {
            sessionLock.Release();
        }
    }

    public async Task<BillDto?> GetBillByIdAsync(int billId, CancellationToken cancellationToken = default)
    {
        Bill? bill = await _billRepository.GetBillByIdAsync(billId, cancellationToken);
        return bill is null ? null : MapBill(bill);
    }

    public async Task<BillDto> RenameBillAsync(
        int billId,
        string billName,
        CancellationToken cancellationToken = default)
    {
        string normalizedName = billName.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName) || normalizedName.Length > 100)
        {
            throw new BusinessException("BILL_NAME_INVALID", "Bill name must contain between 1 and 100 characters.");
        }

        Bill bill = await _billRepository.GetBillByIdAsync(billId, cancellationToken)
            ?? throw new BusinessException("BILL_NOT_FOUND", "Bill does not exist.");

        if (bill.Status != "Unpaid")
        {
            throw new BusinessException("BILL_NOT_UNPAID", "Only unpaid bill can be renamed.");
        }

        bill.BillName = normalizedName;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        BillDto dto = MapBill(bill);
        await NotifyBillChangedAsync(dto.TableSessionId, dto.BillId, cancellationToken);
        return dto;
    }

    public async Task<IReadOnlyList<BillSummaryDto>> GetBillsBySessionAsync(
        int tableSessionId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<BillSummaryDto> bills = NormalizeBillsForListing(
            await _billRepository.GetBillsBySessionAsync(tableSessionId, cancellationToken))
            .Select(MapBillSummary)
            .ToList();

        return bills;
    }

    private static IReadOnlyList<Bill> NormalizeBillsForListing(IReadOnlyList<Bill> bills)
    {
        return bills
            .GroupBy(x => x.BillId)
            .Select(group => group.First())
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.BillNo)
            .ThenBy(x => x.BillId)
            .ToList();
    }

    public async Task<BillDto> AddOrderItemsToDefaultBillAsync(
        AddOrderItemsToBillRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.TargetBillId.HasValue)
        {
            SemaphoreSlim sessionLock = DefaultBillLocks.GetOrAdd(request.TableSessionId, _ => new SemaphoreSlim(1, 1));
            await sessionLock.WaitAsync(cancellationToken);
            try
            {
                return await AddOrderItemsToBillCoreAsync(request, cancellationToken);
            }
            finally
            {
                sessionLock.Release();
            }
        }

        return await AddOrderItemsToBillCoreAsync(request, cancellationToken);
    }

    private async Task<BillDto> AddOrderItemsToBillCoreAsync(
        AddOrderItemsToBillRequest request,
        CancellationToken cancellationToken)
    {
        BillDto dto = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            Order order = await _orderRepository.GetOrderByIdAsync(request.OrderId, ct)
                ?? throw new BusinessException("ORDER_NOT_FOUND", "Order does not exist.");

            if (order.TableSessionId != request.TableSessionId)
            {
                throw new BusinessException("ORDER_SESSION_MISMATCH", "Order does not belong to the requested table session.");
            }

            if (order.Status != "Accepted")
            {
                throw new BusinessException("ORDER_NOT_ACCEPTED", "Only accepted order can be added to bill.");
            }

            Bill bill;

            if (request.TargetBillId.HasValue)
            {
                bill = await _billRepository.GetBillByIdAsync(request.TargetBillId.Value, ct)
                    ?? throw new BusinessException("TARGET_BILL_NOT_FOUND", "Target bill does not exist.");

                if (bill.TableSessionId != request.TableSessionId)
                {
                    throw new BusinessException("TARGET_BILL_SESSION_MISMATCH", "Target bill does not belong to the order table session.");
                }

                if (bill.Status != "Unpaid")
                {
                    throw new BusinessException("TARGET_BILL_NOT_UNPAID", "Only unpaid bill can receive order items.");
                }
            }
            else
            {
                bill = await _billRepository.GetDefaultUnpaidBillBySessionAsync(request.TableSessionId, ct)
                    ?? await CreateDefaultBillAsync(
                        request.TableSessionId,
                        request.CreatedBy,
                        order.SalesChannel ?? await ResolveSalesChannelByIdAsync(order.SalesChannelId, ct),
                        ct);
            }

            if (bill.BillId == 0)
            {
                await _billRepository.AddBillAsync(bill, ct);
                await _unitOfWork.SaveChangesAsync(ct);
            }

            Bill trackedBill = await _billRepository.GetBillByIdAsync(bill.BillId, ct) ?? bill;
            EnsureBillChannelMatchesOrder(trackedBill, order);

            foreach (OrderItem orderItem in order.OrderItems)
            {
                BillDetail line = CreateBillDetailFromOrderItem(trackedBill.BillId, order, orderItem);
                BillDetail? mergeTarget = FindMergeTarget(trackedBill.BillDetails, line);

                if (mergeTarget is null)
                {
                    trackedBill.BillDetails.Add(line);
                }
                else
                {
                    mergeTarget.Quantity += line.Quantity;
                    mergeTarget.NotifiedQuantity += line.NotifiedQuantity;
                    mergeTarget.TotalPrice = mergeTarget.Quantity * mergeTarget.UnitPrice;
                }
            }

            Recalculate(trackedBill);
            await _unitOfWork.SaveChangesAsync(ct);
            return MapBill(trackedBill);
        }, cancellationToken);

        return dto;
    }

    public async Task<BillDto> AdjustBillDetailQuantityAsync(
        AdjustBillDetailQuantityRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        BillDto dto = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            if (request.NewQuantity < 0)
            {
                throw new BusinessException("BILL_DETAIL_QUANTITY_INVALID", "Bill detail quantity cannot be negative.");
            }

            BillDetail detail = await _billRepository.GetBillDetailByIdAsync(request.BillDetailId, ct)
                ?? throw new BusinessException("BILL_DETAIL_NOT_FOUND", "Bill detail does not exist.");

            Bill bill = await _billRepository.GetBillByIdAsync(detail.BillId, ct)
                ?? throw new BusinessException("BILL_NOT_FOUND", "Bill does not exist.");

            if (bill.Status != "Unpaid")
            {
                throw new BusinessException("BILL_NOT_UNPAID", "Only unpaid bill can be adjusted.");
            }

            int originalQuantity = detail.Quantity;
            int reducedQuantity = Math.Max(0, originalQuantity - request.NewQuantity);
            int notifiedReducedQuantity = Math.Max(0, detail.NotifiedQuantity - request.NewQuantity);
            await RestoreStockIfNeededAsync(detail.MenuItemId, reducedQuantity, request.RestoreStock, ct);

            if (notifiedReducedQuantity > 0 && string.IsNullOrWhiteSpace(request.ChangeReason))
            {
                throw new BusinessException("CHANGE_REASON_REQUIRED", "Cancel reason is required when reducing notified quantity.");
            }

            if (notifiedReducedQuantity > 0)
            {
                await _billRepository.AddBillDetailAdjustmentAsync(new BillDetailAdjustment
                {
                    BillId = bill.BillId,
                    BillDetailId = detail.BillDetailId,
                    MenuItemId = detail.MenuItemId,
                    ItemName = detail.ItemName,
                    QuantityBefore = detail.NotifiedQuantity,
                    QuantityAfter = request.NewQuantity,
                    ChangedQuantity = notifiedReducedQuantity,
                    ChangeType = request.NewQuantity == 0 ? "CancelItem" : "ReduceQuantity",
                    Reason = Normalize(request.ChangeReason) ?? "Khac",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId
                }, ct);
            }

            if (request.NewQuantity < detail.NotifiedQuantity)
            {
                detail.NotifiedQuantity = request.NewQuantity;
            }

            if (request.NewQuantity == 0)
            {
                _billRepository.RemoveBillDetail(detail);
                bill.BillDetails.Remove(detail);
            }
            else
            {
                detail.Quantity = request.NewQuantity;
                detail.TotalPrice = detail.Quantity * detail.UnitPrice;
            }

            Recalculate(bill);
            await _unitOfWork.SaveChangesAsync(ct);
            return MapBill(bill);
        }, cancellationToken);

        await NotifyBillChangedAsync(dto.TableSessionId, dto.BillId, cancellationToken);
        return dto;
    }

    public async Task<BillDto> NotifyBillAsync(
        int billId,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        BillDto dto = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            Bill bill = await _billRepository.GetBillByIdAsync(billId, ct)
                ?? throw new BusinessException("BILL_NOT_FOUND", "Bill does not exist.");

            if (bill.Status != "Unpaid")
            {
                throw new BusinessException("BILL_NOT_UNPAID", "Only unpaid bill can be notified.");
            }

            foreach (BillDetail detail in bill.BillDetails)
            {
                detail.NotifiedQuantity = detail.Quantity;
            }

            await _unitOfWork.SaveChangesAsync(ct);
            return MapBill(bill);
        }, cancellationToken);

        await NotifyBillChangedAsync(dto.TableSessionId, dto.BillId, cancellationToken);
        return dto;
    }

    public async Task<BillDto> RecalculateBillTotalAsync(int billId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            Bill bill = await _billRepository.GetBillByIdAsync(billId, ct)
                ?? throw new BusinessException("BILL_NOT_FOUND", "Bill does not exist.");

            Recalculate(bill);
            await _unitOfWork.SaveChangesAsync(ct);
            return MapBill(bill);
        }, cancellationToken);
    }

    public async Task CancelUnpaidBillAsync(
        int billId,
        string reason,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        (int TableSessionId, int? TableId) changed = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            Bill bill = await _billRepository.GetBillByIdAsync(billId, ct)
                ?? throw new BusinessException("BILL_NOT_FOUND", "Bill does not exist.");

            if (bill.Status != "Unpaid")
            {
                throw new BusinessException("BILL_NOT_UNPAID", "Only unpaid bill can be cancelled.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new BusinessException("CANCEL_REASON_REQUIRED", "Cancel reason is required.");
            }

            bill.Status = "Cancelled";
            bill.CancelledAt = DateTime.UtcNow;
            bill.CancelledBy = currentUserId;
            bill.CancelReason = reason.Trim();

            IReadOnlyList<Bill> sessionBills = await _billRepository.GetBillsBySessionAsync(bill.TableSessionId, ct);
            List<Bill> remainingUnpaidBills = sessionBills
                .Where(x => x.BillId != bill.BillId && x.Status == "Unpaid")
                .OrderBy(x => x.BillNo)
                .ToList();

            if (bill.IsDefault)
            {
                bill.IsDefault = false;

                if (remainingUnpaidBills.Count > 0)
                {
                    remainingUnpaidBills[0].IsDefault = true;
                }
            }

            int? tableId = null;

            if (remainingUnpaidBills.Count == 0)
            {
                TableSession? session = await _tableSessionRepository.GetByIdAsync(bill.TableSessionId, ct);
                if (session is not null && (session.Status == "Open" || session.Status == "WaitingPayment"))
                {
                    session.Status = "Closed";
                    session.EndedAt = DateTime.UtcNow;
                    session.ClosedBy = currentUserId;

                    var table = await _tableSessionRepository.GetTableByIdAsync(session.TableId, ct);
                    if (table is not null)
                    {
                        tableId = table.TableId;
                        table.Status = "Available";
                        RotateTableOtp(table, DateTime.UtcNow);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);
            return (bill.TableSessionId, tableId);
        }, cancellationToken);

        await NotifyBillChangedAsync(changed.TableSessionId, billId, cancellationToken);
        if (changed.TableId.HasValue)
        {
            await NotifyTableSessionChangedAsync(changed.TableSessionId, changed.TableId, cancellationToken);
        }
    }

    private async Task<Bill> CreateDefaultBillAsync(
        int tableSessionId,
        int? createdBy,
        SalesChannel salesChannel,
        CancellationToken cancellationToken)
    {
        await _billRepository.ClearDefaultBillFlagsAsync(tableSessionId, cancellationToken);
        int nextBillNo = await _billRepository.GetNextBillNoAsync(tableSessionId, cancellationToken);

        return new Bill
        {
            TableSessionId = tableSessionId,
            SalesChannelId = salesChannel.SalesChannelId,
            SalesChannelCodeSnapshot = salesChannel.ChannelCode,
            SalesChannelNameSnapshot = salesChannel.ChannelName,
            BillCode = $"B{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            BillNo = nextBillNo,
            BillName = $"Bill {nextBillNo}",
            IsDefault = true,
            Status = "Unpaid",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    private static BillDetail CreateBillDetailFromOrderItem(int billId, Order order, OrderItem orderItem)
    {
        decimal selectedChoiceExtraTotal = orderItem.SelectedChoices.Sum(x => x.FinalExtraPriceSnapshot);
        decimal unitPrice = orderItem.FinalUnitPriceSnapshot + selectedChoiceExtraTotal;

        return new BillDetail
        {
            BillId = billId,
            MenuItemId = orderItem.MenuItemId,
            SalesChannelId = order.SalesChannelId,
            ItemName = orderItem.MenuItemNameSnapshot,
            ChoiceSummary = BuildChoiceSummary(orderItem.SelectedChoices),
            Note = Normalize(orderItem.Note),
            Quantity = orderItem.Quantity,
            NotifiedQuantity = 0,
            BasePriceSnapshot = orderItem.BasePriceSnapshot,
            MenuItemChannelExtraPriceSnapshot = orderItem.ChannelExtraPriceSnapshot,
            ChoiceExtraPriceSnapshot = selectedChoiceExtraTotal,
            UnitPrice = unitPrice,
            TotalPrice = unitPrice * orderItem.Quantity,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string? BuildChoiceSummary(IEnumerable<OrderItemSelectedChoice> selectedChoices)
    {
        List<string> groups = selectedChoices
            .OrderBy(x => x.ChoiceGroupId)
            .ThenBy(x => x.ChoiceItemId)
            .GroupBy(x => x.GroupNameSnapshot)
            .Select(group => $"{group.Key}: {string.Join(", ", group.Select(x => x.ChoiceNameSnapshot))}")
            .ToList();

        return groups.Count == 0 ? null : string.Join("; ", groups);
    }

    private static BillDetail? FindMergeTarget(IEnumerable<BillDetail> details, BillDetail incoming)
    {
        return details.FirstOrDefault(x =>
            x.MenuItemId == incoming.MenuItemId &&
            x.SalesChannelId == incoming.SalesChannelId &&
            Normalize(x.ChoiceSummary) == Normalize(incoming.ChoiceSummary) &&
            x.UnitPrice == incoming.UnitPrice &&
            Normalize(x.Note) == Normalize(incoming.Note));
    }

    private static void RotateTableOtp(DineFlow.BusinessObjects.Tables.DiningTable table, DateTime now)
    {
        table.CurrentOtp = TableOtpGenerator.Generate();
        table.OtpUpdatedAt = now;
        table.UpdatedAt = now;
    }

    private async Task RestoreStockIfNeededAsync(
        int menuItemId,
        int quantity,
        bool restoreStock,
        CancellationToken cancellationToken)
    {
        if (!restoreStock || quantity <= 0)
        {
            return;
        }

        var menuItem = await _menuReadRepository.GetMenuItemByIdAsync(menuItemId, cancellationToken);

        if (menuItem?.Stock is null)
        {
            return;
        }

        menuItem.Stock += quantity;

        if (menuItem.Stock > 0)
        {
            menuItem.IsAvailable = true;
        }
    }

    private static void Recalculate(Bill bill)
    {
        foreach (BillDetail detail in bill.BillDetails)
        {
            detail.TotalPrice = detail.Quantity * detail.UnitPrice;
        }

        bill.SubTotal = bill.BillDetails.Sum(x => x.TotalPrice);
        bill.FinalAmount = bill.SubTotal - bill.DiscountAmount;
    }

    private static BillDto MapBill(Bill bill)
    {
        BillDto dto = new()
        {
            BillId = bill.BillId,
            BillCode = bill.BillCode,
            TableSessionId = bill.TableSessionId,
            SalesChannelId = bill.SalesChannelId,
            SalesChannelCode = bill.SalesChannelCodeSnapshot,
            SalesChannelName = bill.SalesChannelNameSnapshot,
            BillNo = bill.BillNo,
            BillName = bill.BillName,
            IsDefault = bill.IsDefault,
            Status = bill.Status,
            SubTotal = bill.SubTotal,
            DiscountAmount = bill.DiscountAmount,
            FinalAmount = bill.FinalAmount,
            CreatedAt = bill.CreatedAt,
            Details = bill.BillDetails
                .OrderBy(x => x.BillDetailId)
                .Select(MapBillDetail)
                .ToList(),
            Payments = bill.Payments
                .OrderBy(x => x.PaidAt)
                .Select(MapPayment)
                .ToList()
        };

        return dto;
    }

    private static BillSummaryDto MapBillSummary(Bill bill)
    {
        return new BillSummaryDto
        {
            BillId = bill.BillId,
            BillCode = bill.BillCode,
            TableSessionId = bill.TableSessionId,
            SalesChannelId = bill.SalesChannelId,
            SalesChannelCode = bill.SalesChannelCodeSnapshot,
            SalesChannelName = bill.SalesChannelNameSnapshot,
            BillNo = bill.BillNo,
            BillName = bill.BillName,
            IsDefault = bill.IsDefault,
            Status = bill.Status,
            SubTotal = bill.SubTotal,
            DiscountAmount = bill.DiscountAmount,
            FinalAmount = bill.FinalAmount,
            CreatedAt = bill.CreatedAt
        };
    }

    private static BillDetailDto MapBillDetail(BillDetail detail)
    {
        return new BillDetailDto
        {
            BillDetailId = detail.BillDetailId,
            BillId = detail.BillId,
            MenuItemId = detail.MenuItemId,
            SalesChannelId = detail.SalesChannelId,
            ItemName = detail.ItemName,
            ChoiceSummary = detail.ChoiceSummary,
            Note = detail.Note,
            Quantity = detail.Quantity,
            NotifiedQuantity = detail.NotifiedQuantity,
            BasePriceSnapshot = detail.BasePriceSnapshot,
            MenuItemChannelExtraPriceSnapshot = detail.MenuItemChannelExtraPriceSnapshot,
            ChoiceExtraPriceSnapshot = detail.ChoiceExtraPriceSnapshot,
            UnitPrice = detail.UnitPrice,
            TotalPrice = detail.TotalPrice
        };
    }

    private static PaymentDto MapPayment(Payment payment)
    {
        return new PaymentDto
        {
            PaymentId = payment.PaymentId,
            BillId = payment.BillId,
            PaymentMethod = payment.PaymentMethod,
            Amount = payment.Amount,
            PaidAt = payment.PaidAt,
            ConfirmedBy = payment.ConfirmedBy,
            UpdatedAt = payment.UpdatedAt,
            UpdatedBy = payment.UpdatedBy,
            ChangeReason = payment.ChangeReason
        };
    }

    private static string? Normalize(string? value)
    {
        string? normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private async Task<SalesChannel> ResolveSalesChannelByCodeAsync(
        string salesChannelCode,
        CancellationToken cancellationToken)
    {
        SalesChannel? salesChannel = await _menuReadRepository.GetSalesChannelByCodeAsync(salesChannelCode, cancellationToken);
        return salesChannel ?? throw new BusinessException("CHANNEL_NOT_FOUND", "Sales channel does not exist.");
    }

    private async Task<SalesChannel> ResolveSalesChannelByIdAsync(
        int salesChannelId,
        CancellationToken cancellationToken)
    {
        SalesChannel? salesChannel = await _menuManagementRepository.GetSalesChannelAsync(salesChannelId, cancellationToken);
        return salesChannel is not null && salesChannel.IsActive && !salesChannel.IsDeleted
            ? salesChannel
            : throw new BusinessException("CHANNEL_NOT_FOUND", "Sales channel does not exist.");
    }

    private static void EnsureBillChannelMatchesOrder(Bill bill, Order order)
    {
        if (bill.SalesChannelId == order.SalesChannelId)
        {
            return;
        }

        bool emptyBill = bill.BillDetails.Count == 0;
        if (emptyBill && order.SalesChannel is not null)
        {
            bill.SalesChannelId = order.SalesChannelId;
            bill.SalesChannelCodeSnapshot = order.SalesChannel.ChannelCode;
            bill.SalesChannelNameSnapshot = order.SalesChannel.ChannelName;
            return;
        }

        throw new BusinessException(
            "BILL_CHANNEL_MISMATCH",
            "Bill sales channel is different from order sales channel. Create or select another bill for this channel.");
    }

    private async Task NotifyBillChangedAsync(
        int tableSessionId,
        int? billId,
        CancellationToken cancellationToken)
    {
        RealtimeEventDto payload = new()
        {
            TableSessionId = tableSessionId,
            BillId = billId
        };

        await _realtimeNotificationService.NotifyStaffAsync(
            RealtimeEvents.BillChanged,
            payload,
            cancellationToken);
        await _realtimeNotificationService.NotifySessionAsync(
            tableSessionId,
            RealtimeEvents.BillChanged,
            payload,
            cancellationToken);
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

    public async Task<BillDto> ApplySalesChannelPricingAsync(int billId, int salesChannelId, CancellationToken cancellationToken = default)
    {
        BillDto dto = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            Bill bill = await _billRepository.GetBillByIdAsync(billId, ct)
                ?? throw new BusinessException("BILL_NOT_FOUND", "Bill does not exist.");

            if (bill.Status != "Unpaid")
            {
                throw new BusinessException("BILL_NOT_UNPAID", "Only unpaid bills can have their pricing updated.");
            }

            SalesChannel salesChannel = await ResolveSalesChannelByIdAsync(salesChannelId, ct);

            bill.SalesChannelId = salesChannel.SalesChannelId;
            bill.SalesChannelCodeSnapshot = salesChannel.ChannelCode;
            bill.SalesChannelNameSnapshot = salesChannel.ChannelName;

            foreach (BillDetail detail in bill.BillDetails)
            {
                var menuItem = await _menuReadRepository.GetCatalogItemByIdAsync(detail.MenuItemId, ct);
                if (menuItem is null) continue;

                decimal menuItemExtraPrice = menuItem.ChannelPrices
                    .FirstOrDefault(x => x.SalesChannelId == salesChannelId)?
                    .ChannelExtraPrice ?? 0m;

                decimal choiceExtraPriceSum = 0m;

                if (!string.IsNullOrWhiteSpace(detail.ChoiceSummary))
                {
                    var selectedChoiceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var parts = detail.ChoiceSummary.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        int colonIdx = part.IndexOf(':');
                        if (colonIdx >= 0)
                        {
                            var choicesPart = part.Substring(colonIdx + 1);
                            var names = choicesPart.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            foreach (var name in names)
                            {
                                selectedChoiceNames.Add(name.Trim());
                            }
                        }
                    }

                    foreach (var assignment in menuItem.MenuItemChoiceGroups)
                    {
                        var choiceGroup = assignment.ChoiceGroup;
                        if (choiceGroup is null) continue;

                        foreach (var choiceItem in choiceGroup.ChoiceItems)
                        {
                            if (selectedChoiceNames.Contains(choiceItem.ChoiceName))
                            {
                                decimal choiceExtra = choiceItem.ExtraPrice;
                                decimal channelChoiceExtra = choiceItem.ChannelPrices
                                    .FirstOrDefault(x => x.SalesChannelId == salesChannelId)?
                                    .ChannelExtraPrice ?? 0m;

                                choiceExtraPriceSum += (choiceExtra + channelChoiceExtra);
                            }
                        }
                    }
                }

                decimal newUnitPrice = menuItem.BasePrice + menuItemExtraPrice + choiceExtraPriceSum;
                detail.SalesChannelId = salesChannel.SalesChannelId;
                detail.BasePriceSnapshot = menuItem.BasePrice;
                detail.MenuItemChannelExtraPriceSnapshot = menuItemExtraPrice;
                detail.ChoiceExtraPriceSnapshot = choiceExtraPriceSum;
                detail.UnitPrice = newUnitPrice;
                detail.TotalPrice = newUnitPrice * detail.Quantity;
            }

            Recalculate(bill);
            await _unitOfWork.SaveChangesAsync(ct);
            return MapBill(bill);
        }, cancellationToken);

        await NotifyBillChangedAsync(dto.TableSessionId, dto.BillId, cancellationToken);
        return dto;
    }
}
