using DineFlow.BusinessObjects.Bills;
using DineFlow.BusinessObjects.Menu;
using DineFlow.Repositories.Bills;
using DineFlow.Repositories.Common;
using DineFlow.Repositories.Menu;
using DineFlow.Services.Common;
using DineFlow.Services.Realtime;

namespace DineFlow.Services.Bills;

public class SplitBillService : ISplitBillService
{
    private const string DefaultSalesChannelCode = "DINE_IN";
    private readonly IBillRepository _billRepository;
    private readonly IMenuManagementRepository _menuManagementRepository;
    private readonly IRealtimeNotificationService _realtimeNotificationService;
    private readonly IUnitOfWork _unitOfWork;

    public SplitBillService(
        IBillRepository billRepository,
        IMenuManagementRepository menuManagementRepository,
        IRealtimeNotificationService realtimeNotificationService,
        IUnitOfWork unitOfWork)
    {
        _billRepository = billRepository;
        _menuManagementRepository = menuManagementRepository;
        _realtimeNotificationService = realtimeNotificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<BillDto> CreateSplitBillAsync(
        SplitBillRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        BillMoveResult result = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            Bill sourceBill = await GetUnpaidBillAsync(request.SourceBillId, ct);
            Bill targetBill = request.TargetBillId.HasValue
                ? await GetUnpaidBillAsync(request.TargetBillId.Value, ct)
                : await CreateTargetBillAsync(sourceBill.TableSessionId, request.NewBillName, currentUserId, sourceBill, ct);

            ValidateSourceBillWillRemainNonEmpty(sourceBill, new Dictionary<int, int>
            {
                [request.BillDetailId] = request.QuantityToMove
            });

            await MoveQuantityAsync(sourceBill, targetBill, request.BillDetailId, request.QuantityToMove, ct);

            await _unitOfWork.SaveChangesAsync(ct);
            return new BillMoveResult(MapBill(targetBill), sourceBill.BillId);
        }, cancellationToken);

        await NotifyBillChangedAsync(result.TargetBill.TableSessionId, result.TargetBill.BillId, cancellationToken);
        await NotifyBillChangedAsync(result.TargetBill.TableSessionId, result.SourceBillId, cancellationToken);
        return result.TargetBill;
    }

    public async Task<BillDto> SplitBillBatchAsync(
        SplitBillBatchRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        BillMoveResult result = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            if (request.Items.Count == 0)
            {
                throw new BusinessException("SPLIT_ITEMS_REQUIRED", "At least one item must be selected for split.");
            }

            Bill sourceBill = await GetUnpaidBillAsync(request.SourceBillId, ct);
            Bill targetBill = await ResolveSplitTargetBillAsync(sourceBill, request, currentUserId, ct);

            Dictionary<int, int> quantitiesByDetailId = request.Items
                .GroupBy(x => x.BillDetailId)
                .ToDictionary(group => group.Key, group => group.Sum(x => x.QuantityToMove));

            ValidateSourceBillWillRemainNonEmpty(sourceBill, quantitiesByDetailId);

            foreach ((int billDetailId, int quantityToMove) in quantitiesByDetailId)
            {
                await MoveQuantityAsync(sourceBill, targetBill, billDetailId, quantityToMove, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);
            return new BillMoveResult(MapBill(targetBill), sourceBill.BillId);
        }, cancellationToken);

        await NotifyBillChangedAsync(result.TargetBill.TableSessionId, result.TargetBill.BillId, cancellationToken);
        await NotifyBillChangedAsync(result.TargetBill.TableSessionId, result.SourceBillId, cancellationToken);
        return result.TargetBill;
    }

    public async Task<BillDto> MergeBillAsync(
        MergeBillRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        BillMoveResult result = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            Bill sourceBill = await GetUnpaidBillAsync(request.SourceBillId, ct);
            Bill targetBill = await GetUnpaidBillAsync(request.TargetBillId, ct);
            EnsureSameSalesChannel(sourceBill, targetBill);

            if (sourceBill.BillId == targetBill.BillId)
            {
                throw new BusinessException("BILL_MERGE_SAME_BILL", "Source and target bill must be different.");
            }

            if (sourceBill.TableSessionId != targetBill.TableSessionId)
            {
                throw new BusinessException("BILL_SESSION_MISMATCH", "Source and target bill must belong to the same table session.");
            }

            List<(int BillDetailId, int Quantity)> sourceLines = sourceBill.BillDetails
                .Select(x => (x.BillDetailId, x.Quantity))
                .ToList();

            foreach ((int billDetailId, int quantity) in sourceLines)
            {
                await MoveQuantityAsync(sourceBill, targetBill, billDetailId, quantity, ct);
            }

            bool targetShouldBecomeDefault = targetBill.IsDefault || sourceBill.IsDefault;

            if (targetShouldBecomeDefault)
            {
                await _billRepository.ClearDefaultBillFlagsAsync(sourceBill.TableSessionId, ct);
                targetBill.IsDefault = true;
            }

            sourceBill.IsDefault = false;
            sourceBill.Status = "Merged";
            sourceBill.CancelledAt = DateTime.UtcNow;
            sourceBill.CancelledBy = currentUserId;
            sourceBill.CancelReason = $"Merged into {targetBill.BillName}";

            Recalculate(targetBill);
            await _unitOfWork.SaveChangesAsync(ct);
            return new BillMoveResult(MapBill(targetBill), sourceBill.BillId);
        }, cancellationToken);

        await NotifyBillChangedAsync(result.TargetBill.TableSessionId, result.TargetBill.BillId, cancellationToken);
        await NotifyBillChangedAsync(result.TargetBill.TableSessionId, result.SourceBillId, cancellationToken);
        return result.TargetBill;
    }

    public async Task<BillDto> MoveItemToBillAsync(
        MoveBillItemRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        BillMoveResult result = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            Bill sourceBill = await GetUnpaidBillAsync(request.SourceBillId, ct);
            Bill targetBill = await GetUnpaidBillAsync(request.TargetBillId, ct);

            ValidateSourceBillWillRemainNonEmpty(sourceBill, new Dictionary<int, int>
            {
                [request.BillDetailId] = request.QuantityToMove
            });

            await MoveQuantityAsync(sourceBill, targetBill, request.BillDetailId, request.QuantityToMove, ct);

            await _unitOfWork.SaveChangesAsync(ct);
            return new BillMoveResult(MapBill(targetBill), sourceBill.BillId);
        }, cancellationToken);

        await NotifyBillChangedAsync(result.TargetBill.TableSessionId, result.TargetBill.BillId, cancellationToken);
        await NotifyBillChangedAsync(result.TargetBill.TableSessionId, result.SourceBillId, cancellationToken);
        return result.TargetBill;
    }

    public async Task<BillDto> CreateEmptyBillForSessionAsync(
        int tableSessionId,
        string billName,
        int currentUserId,
        int? salesChannelId = null,
        CancellationToken cancellationToken = default)
    {
        BillDto dto = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            SalesChannel salesChannel = salesChannelId.HasValue
                ? await ResolveSalesChannelByIdAsync(salesChannelId.Value, ct)
                : await ResolveSalesChannelByCodeAsync(DefaultSalesChannelCode, ct);
            Bill bill = await CreateNewBillAsync(tableSessionId, billName, currentUserId, salesChannel, ct);
            await _billRepository.AddBillAsync(bill, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return MapBill(bill);
        }, cancellationToken);

        await NotifyBillChangedAsync(dto.TableSessionId, dto.BillId, cancellationToken);
        return dto;
    }

    public async Task<bool> ValidateSplitQuantityAsync(
        int billDetailId,
        int quantityToMove,
        CancellationToken cancellationToken = default)
    {
        return await _billRepository.IsSplitQuantityValidAsync(billDetailId, quantityToMove, cancellationToken);
    }

    private async Task<Bill> GetUnpaidBillAsync(int billId, CancellationToken cancellationToken)
    {
        Bill bill = await _billRepository.GetBillByIdAsync(billId, cancellationToken)
            ?? throw new BusinessException("BILL_NOT_FOUND", "Bill does not exist.");

        if (bill.Status != "Unpaid")
        {
            throw new BusinessException("BILL_NOT_UNPAID", "Only unpaid bill can be split or moved.");
        }

        return bill;
    }

    private async Task<Bill> CreateTargetBillAsync(
        int tableSessionId,
        string? billName,
        int currentUserId,
        Bill sourceBill,
        CancellationToken cancellationToken)
    {
        Bill bill = await CreateNewBillAsync(tableSessionId, billName, currentUserId, GetSalesChannelSnapshot(sourceBill), cancellationToken);
        await _billRepository.AddBillAsync(bill, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return bill;
    }

    private async Task<Bill> ResolveSplitTargetBillAsync(
        Bill sourceBill,
        SplitBillBatchRequest request,
        int currentUserId,
        CancellationToken cancellationToken)
    {
        if (request.CreateNewBill)
        {
            return await CreateTargetBillAsync(sourceBill.TableSessionId, request.NewBillName, currentUserId, sourceBill, cancellationToken);
        }

        if (!request.TargetBillId.HasValue)
        {
            throw new BusinessException("TARGET_BILL_REQUIRED", "Target bill is required when not creating a new bill.");
        }

        Bill targetBill = await GetUnpaidBillAsync(request.TargetBillId.Value, cancellationToken);

        if (sourceBill.TableSessionId != targetBill.TableSessionId)
        {
            throw new BusinessException("BILL_SESSION_MISMATCH", "Source and target bill must belong to the same table session.");
        }
        EnsureSameSalesChannel(sourceBill, targetBill);

        return targetBill;
    }

    private async Task<Bill> CreateNewBillAsync(
        int tableSessionId,
        string? billName,
        int currentUserId,
        SalesChannel salesChannel,
        CancellationToken cancellationToken)
    {
        int nextBillNo = await _billRepository.GetNextBillNoAsync(tableSessionId, cancellationToken);

        return new Bill
        {
            TableSessionId = tableSessionId,
            SalesChannelId = salesChannel.SalesChannelId,
            SalesChannelCodeSnapshot = salesChannel.ChannelCode,
            SalesChannelNameSnapshot = salesChannel.ChannelName,
            BillCode = $"B{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            BillNo = nextBillNo,
            BillName = string.IsNullOrWhiteSpace(billName) ? $"Bill {nextBillNo}" : billName.Trim(),
            IsDefault = false,
            Status = "Unpaid",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId
        };
    }

    private async Task MoveQuantityAsync(
        Bill sourceBill,
        Bill targetBill,
        int billDetailId,
        int quantityToMove,
        CancellationToken cancellationToken)
    {
        if (sourceBill.BillId == targetBill.BillId)
        {
            throw new BusinessException("BILL_MOVE_SAME_BILL", "Source and target bill must be different.");
        }

        if (sourceBill.TableSessionId != targetBill.TableSessionId)
        {
            throw new BusinessException("BILL_SESSION_MISMATCH", "Source and target bill must belong to the same table session.");
        }
        EnsureSameSalesChannel(sourceBill, targetBill);

        BillDetail sourceDetail = sourceBill.BillDetails.FirstOrDefault(x => x.BillDetailId == billDetailId)
            ?? throw new BusinessException("BILL_DETAIL_NOT_FOUND", "Bill detail does not exist in source bill.");

        if (quantityToMove <= 0 || quantityToMove > sourceDetail.Quantity)
        {
            throw new BusinessException("SPLIT_QUANTITY_INVALID", "Quantity to move is invalid.");
        }

        BillDetail movingDetail = CloneForTargetBill(targetBill.BillId, sourceDetail, quantityToMove);
        BillDetail? mergeTarget = FindMergeTarget(targetBill.BillDetails, movingDetail);

        if (mergeTarget is null)
        {
            targetBill.BillDetails.Add(movingDetail);
        }
        else
        {
            mergeTarget.Quantity += quantityToMove;
            mergeTarget.NotifiedQuantity += movingDetail.NotifiedQuantity;
            mergeTarget.TotalPrice = mergeTarget.Quantity * mergeTarget.UnitPrice;
        }

        sourceDetail.Quantity -= quantityToMove;
        sourceDetail.NotifiedQuantity = Math.Max(0, sourceDetail.NotifiedQuantity - movingDetail.NotifiedQuantity);

        if (sourceDetail.Quantity == 0)
        {
            sourceBill.BillDetails.Remove(sourceDetail);
            _billRepository.RemoveBillDetail(sourceDetail);
        }
        else
        {
            sourceDetail.TotalPrice = sourceDetail.Quantity * sourceDetail.UnitPrice;
        }

        Recalculate(sourceBill);
        Recalculate(targetBill);
    }

    private static void ValidateSourceBillWillRemainNonEmpty(
        Bill sourceBill,
        IReadOnlyDictionary<int, int> quantitiesByDetailId)
    {
        if (quantitiesByDetailId.Values.Any(quantity => quantity <= 0))
        {
            throw new BusinessException("SPLIT_QUANTITY_INVALID", "Quantity to move is invalid.");
        }

        int remainingQuantity = 0;

        foreach (BillDetail detail in sourceBill.BillDetails)
        {
            quantitiesByDetailId.TryGetValue(detail.BillDetailId, out int quantityToMove);

            if (quantityToMove > detail.Quantity)
            {
                throw new BusinessException("SPLIT_QUANTITY_INVALID", "Quantity to move is invalid.");
            }

            remainingQuantity += detail.Quantity - quantityToMove;
        }

        bool hasUnknownDetail = quantitiesByDetailId.Keys.Any(id => sourceBill.BillDetails.All(detail => detail.BillDetailId != id));

        if (hasUnknownDetail)
        {
            throw new BusinessException("BILL_DETAIL_NOT_FOUND", "Bill detail does not exist in source bill.");
        }

        if (remainingQuantity <= 0)
        {
            throw new BusinessException("SPLIT_SOURCE_EMPTY", "Source bill must keep at least one item after split.");
        }
    }

    private static BillDetail CloneForTargetBill(int targetBillId, BillDetail sourceDetail, int quantity)
    {
        return new BillDetail
        {
            BillId = targetBillId,
            MenuItemId = sourceDetail.MenuItemId,
            SalesChannelId = sourceDetail.SalesChannelId,
            ItemName = sourceDetail.ItemName,
            ChoiceSummary = Normalize(sourceDetail.ChoiceSummary),
            Note = Normalize(sourceDetail.Note),
            Quantity = quantity,
            NotifiedQuantity = Math.Min(sourceDetail.NotifiedQuantity, quantity),
            BasePriceSnapshot = sourceDetail.BasePriceSnapshot,
            MenuItemChannelExtraPriceSnapshot = sourceDetail.MenuItemChannelExtraPriceSnapshot,
            ChoiceExtraPriceSnapshot = sourceDetail.ChoiceExtraPriceSnapshot,
            UnitPrice = sourceDetail.UnitPrice,
            TotalPrice = quantity * sourceDetail.UnitPrice,
            CreatedAt = DateTime.UtcNow
        };
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
        return new BillDto
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
            Details = bill.BillDetails.OrderBy(x => x.BillDetailId).Select(MapBillDetail).ToList(),
            Payments = bill.Payments.OrderBy(x => x.PaidAt).Select(MapPayment).ToList()
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

    private async Task<SalesChannel> ResolveSalesChannelByCodeAsync(string salesChannelCode, CancellationToken cancellationToken)
    {
        List<SalesChannel> channels = await _menuManagementRepository.GetSalesChannelsAsync(cancellationToken);
        return channels.FirstOrDefault(channel =>
                channel.ChannelCode.Equals(salesChannelCode, StringComparison.OrdinalIgnoreCase) &&
                channel.IsActive &&
                !channel.IsDeleted)
            ?? throw new BusinessException("CHANNEL_NOT_FOUND", "Sales channel does not exist.");
    }

    private async Task<SalesChannel> ResolveSalesChannelByIdAsync(int salesChannelId, CancellationToken cancellationToken)
    {
        SalesChannel? channel = await _menuManagementRepository.GetSalesChannelAsync(salesChannelId, cancellationToken);
        return channel is not null && channel.IsActive && !channel.IsDeleted
            ? channel
            : throw new BusinessException("CHANNEL_NOT_FOUND", "Sales channel does not exist.");
    }

    private static SalesChannel GetSalesChannelSnapshot(Bill bill) => new()
    {
        SalesChannelId = bill.SalesChannelId,
        ChannelCode = bill.SalesChannelCodeSnapshot,
        ChannelName = bill.SalesChannelNameSnapshot,
        IsActive = true
    };

    private static void EnsureSameSalesChannel(Bill sourceBill, Bill targetBill)
    {
        if (sourceBill.SalesChannelId != targetBill.SalesChannelId)
        {
            throw new BusinessException(
                "BILL_CHANNEL_MISMATCH",
                "Source and target bill must use the same sales channel.");
        }
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

    private sealed record BillMoveResult(BillDto TargetBill, int SourceBillId);
}
