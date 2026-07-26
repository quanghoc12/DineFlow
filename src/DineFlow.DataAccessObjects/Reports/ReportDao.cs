using DineFlow.BusinessObjects.Reports;
using DineFlow.BusinessObjects.Tables;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Reports;

public sealed class ReportDao : IReportDao
{
    private readonly AppDbContext _dbContext;

    public ReportDao(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardDto> GetDashboardByLocalDateAsync(
        DateTime localDate,
        TimeSpan localOffset,
        int topItemCount,
        CancellationToken cancellationToken = default)
    {
        (DateTime startUtc, DateTime endUtc) = BuildUtcRange(localDate, localOffset);

        IQueryable<DineFlow.BusinessObjects.Bills.Bill> paidBillsQuery = _dbContext.Bills
            .Where(x => x.Status == "Paid" &&
                        x.PaidAt.HasValue &&
                        x.PaidAt.Value >= startUtc &&
                        x.PaidAt.Value < endUtc);

        decimal revenueToday = (await paidBillsQuery
            .SumAsync(x => (decimal?)x.FinalAmount, cancellationToken)) ?? 0m;

        int paidBillCount = await paidBillsQuery.CountAsync(cancellationToken);

        int orderCountToday = await _dbContext.Orders
            .Where(x => x.CreatedAt >= startUtc && x.CreatedAt < endUtc)
            .CountAsync(cancellationToken);

        int printFailedOrderCount = await _dbContext.Orders
            .Where(x => x.CreatedAt >= startUtc &&
                        x.CreatedAt < endUtc &&
                        x.PrintStatus == "PrintFailed")
            .CountAsync(cancellationToken);

        int servingTableCount = await _dbContext.DiningTables
            .Where(x => x.IsActive && x.Status == TableStatuses.Occupied)
            .CountAsync(cancellationToken);

        int waitingPaymentTableCount = await _dbContext.DiningTables
            .Where(x => x.IsActive && x.Status == TableStatuses.WaitingPayment)
            .CountAsync(cancellationToken);

        List<TopSellingItemDto> topSellingItems = await _dbContext.BillDetails
            .Where(detail => detail.Bill != null &&
                             detail.Bill.Status == "Paid" &&
                             detail.Bill.PaidAt.HasValue &&
                             detail.Bill.PaidAt.Value >= startUtc &&
                             detail.Bill.PaidAt.Value < endUtc)
            .GroupBy(detail => new { detail.MenuItemId, detail.ItemName })
            .Select(group => new TopSellingItemDto
            {
                MenuItemId = group.Key.MenuItemId,
                ItemName = group.Key.ItemName,
                TotalQuantity = group.Sum(x => x.Quantity),
                TotalRevenue = group.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .ThenByDescending(x => x.TotalRevenue)
            .ThenBy(x => x.ItemName)
            .Take(topItemCount)
            .ToListAsync(cancellationToken);

        List<PaymentMethodRevenueDto> revenueByPaymentMethods = await _dbContext.Payments
            .Where(payment => payment.Bill != null &&
                              payment.Bill.Status == "Paid" &&
                              payment.Bill.PaidAt.HasValue &&
                              payment.Bill.PaidAt.Value >= startUtc &&
                              payment.Bill.PaidAt.Value < endUtc)
            .GroupBy(payment => payment.PaymentMethod)
            .Select(group => new PaymentMethodRevenueDto
            {
                PaymentMethod = group.Key,
                PaymentCount = group.Count(),
                TotalAmount = group.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.TotalAmount)
            .ThenBy(x => x.PaymentMethod)
            .ToListAsync(cancellationToken);

        return new DashboardDto
        {
            Date = localDate.Date,
            RevenueToday = revenueToday,
            PaidBillCountToday = paidBillCount,
            AverageBillValue = paidBillCount == 0 ? 0 : revenueToday / paidBillCount,
            OrderCountToday = orderCountToday,
            ServingTableCount = servingTableCount,
            WaitingPaymentTableCount = waitingPaymentTableCount,
            PrintFailedOrderCount = printFailedOrderCount,
            TopSellingItems = topSellingItems,
            RevenueByPaymentMethods = revenueByPaymentMethods
        };
    }

    public async Task<DashboardDto> GetDashboardByLocalDateRangeAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        TimeSpan localOffset,
        int topItemCount,
        CancellationToken cancellationToken = default)
    {
        (DateTime startUtc, DateTime endUtc) = BuildUtcRange(fromLocalDate, toLocalDate, localOffset);

        IQueryable<DineFlow.BusinessObjects.Bills.Bill> paidBillsQuery = _dbContext.Bills
            .Where(x => x.Status == "Paid" &&
                        x.PaidAt.HasValue &&
                        x.PaidAt.Value >= startUtc &&
                        x.PaidAt.Value < endUtc);

        decimal revenueToday = (await paidBillsQuery
            .SumAsync(x => (decimal?)x.FinalAmount, cancellationToken)) ?? 0m;

        int paidBillCount = await paidBillsQuery.CountAsync(cancellationToken);

        int orderCountToday = await _dbContext.Orders
            .Where(x => x.CreatedAt >= startUtc && x.CreatedAt < endUtc)
            .CountAsync(cancellationToken);

        int printFailedOrderCount = await _dbContext.Orders
            .Where(x => x.CreatedAt >= startUtc &&
                        x.CreatedAt < endUtc &&
                        x.PrintStatus == "PrintFailed")
            .CountAsync(cancellationToken);

        int servingTableCount = await _dbContext.DiningTables
            .Where(x => x.IsActive && x.Status == TableStatuses.Occupied)
            .CountAsync(cancellationToken);

        int waitingPaymentTableCount = await _dbContext.DiningTables
            .Where(x => x.IsActive && x.Status == TableStatuses.WaitingPayment)
            .CountAsync(cancellationToken);

        List<TopSellingItemDto> topSellingItems = await _dbContext.BillDetails
            .Where(detail => detail.Bill != null &&
                             detail.Bill.Status == "Paid" &&
                             detail.Bill.PaidAt.HasValue &&
                             detail.Bill.PaidAt.Value >= startUtc &&
                             detail.Bill.PaidAt.Value < endUtc)
            .GroupBy(detail => new { detail.MenuItemId, detail.ItemName })
            .Select(group => new TopSellingItemDto
            {
                MenuItemId = group.Key.MenuItemId,
                ItemName = group.Key.ItemName,
                TotalQuantity = group.Sum(x => x.Quantity),
                TotalRevenue = group.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .ThenByDescending(x => x.TotalRevenue)
            .ThenBy(x => x.ItemName)
            .Take(topItemCount)
            .ToListAsync(cancellationToken);

        List<PaymentMethodRevenueDto> revenueByPaymentMethods = await _dbContext.Payments
            .Where(payment => payment.Bill != null &&
                              payment.Bill.Status == "Paid" &&
                              payment.Bill.PaidAt.HasValue &&
                              payment.Bill.PaidAt.Value >= startUtc &&
                              payment.Bill.PaidAt.Value < endUtc)
            .GroupBy(payment => payment.PaymentMethod)
            .Select(group => new PaymentMethodRevenueDto
            {
                PaymentMethod = group.Key,
                PaymentCount = group.Count(),
                TotalAmount = group.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.TotalAmount)
            .ThenBy(x => x.PaymentMethod)
            .ToListAsync(cancellationToken);

        return new DashboardDto
        {
            Date = fromLocalDate.Date,
            RevenueToday = revenueToday,
            PaidBillCountToday = paidBillCount,
            AverageBillValue = paidBillCount == 0 ? 0 : revenueToday / paidBillCount,
            OrderCountToday = orderCountToday,
            ServingTableCount = servingTableCount,
            WaitingPaymentTableCount = waitingPaymentTableCount,
            PrintFailedOrderCount = printFailedOrderCount,
            TopSellingItems = topSellingItems,
            RevenueByPaymentMethods = revenueByPaymentMethods
        };
    }

    public async Task<RevenueSummaryDto> GetRevenueSummaryByLocalDateRangeAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        TimeSpan localOffset,
        CancellationToken cancellationToken = default)
    {
        (DateTime startUtc, DateTime endUtcExclusive) = BuildUtcRange(fromLocalDate, toLocalDate, localOffset);

        IQueryable<DineFlow.BusinessObjects.Bills.Bill> paidBillsQuery = _dbContext.Bills
            .Where(x => x.Status == "Paid" &&
                        x.PaidAt.HasValue &&
                        x.PaidAt.Value >= startUtc &&
                        x.PaidAt.Value < endUtcExclusive);

        decimal totalRevenue = (await paidBillsQuery
            .SumAsync(x => (decimal?)x.FinalAmount, cancellationToken)) ?? 0m;

        int paidBillCount = await paidBillsQuery.CountAsync(cancellationToken);

        List<DineFlow.BusinessObjects.Bills.Bill> paidBills = await paidBillsQuery
            .Select(x => new DineFlow.BusinessObjects.Bills.Bill
            {
                FinalAmount = x.FinalAmount,
                PaidAt = x.PaidAt
            })
            .ToListAsync(cancellationToken);

        List<RevenueByDayDto> revenueByDays = paidBills
            .Where(x => x.PaidAt.HasValue)
            .GroupBy(x => x.PaidAt!.Value.Add(localOffset).Date)
            .Select(group => new RevenueByDayDto
            {
                Date = group.Key,
                Revenue = group.Sum(x => x.FinalAmount),
                PaidBillCount = group.Count()
            })
            .OrderBy(x => x.Date)
            .ToList();

        return new RevenueSummaryDto
        {
            FromDate = fromLocalDate.Date,
            ToDate = toLocalDate.Date,
            TotalRevenue = totalRevenue,
            PaidBillCount = paidBillCount,
            AverageBillValue = paidBillCount == 0 ? 0 : totalRevenue / paidBillCount,
            RevenueByDays = revenueByDays
        };
    }

    public async Task<IReadOnlyList<TopSellingItemDto>> GetTopSellingItemsByLocalDateRangeAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        TimeSpan localOffset,
        int topCount,
        CancellationToken cancellationToken = default)
    {
        (DateTime startUtc, DateTime endUtcExclusive) = BuildUtcRange(fromLocalDate, toLocalDate, localOffset);

        return await _dbContext.BillDetails
            .Where(detail => detail.Bill != null &&
                             detail.Bill.Status == "Paid" &&
                             detail.Bill.PaidAt.HasValue &&
                             detail.Bill.PaidAt.Value >= startUtc &&
                             detail.Bill.PaidAt.Value < endUtcExclusive)
            .GroupBy(detail => new { detail.MenuItemId, detail.ItemName })
            .Select(group => new TopSellingItemDto
            {
                MenuItemId = group.Key.MenuItemId,
                ItemName = group.Key.ItemName,
                TotalQuantity = group.Sum(x => x.Quantity),
                TotalRevenue = group.Sum(x => x.TotalPrice)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .ThenByDescending(x => x.TotalRevenue)
            .ThenBy(x => x.ItemName)
            .Take(topCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentMethodRevenueDto>> GetRevenueByPaymentMethodByLocalDateRangeAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        TimeSpan localOffset,
        CancellationToken cancellationToken = default)
    {
        (DateTime startUtc, DateTime endUtcExclusive) = BuildUtcRange(fromLocalDate, toLocalDate, localOffset);

        return await _dbContext.Payments
            .Where(payment => payment.Bill != null &&
                              payment.Bill.Status == "Paid" &&
                              payment.Bill.PaidAt.HasValue &&
                              payment.Bill.PaidAt.Value >= startUtc &&
                              payment.Bill.PaidAt.Value < endUtcExclusive)
            .GroupBy(payment => payment.PaymentMethod)
            .Select(group => new PaymentMethodRevenueDto
            {
                PaymentMethod = group.Key,
                PaymentCount = group.Count(),
                TotalAmount = group.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.TotalAmount)
            .ThenBy(x => x.PaymentMethod)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaidBillHistoryItemDto>> GetPaidBillHistoryByLocalDateRangeAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        TimeSpan localOffset,
        string? paymentMethod,
        string? tableName,
        string? area,
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        (DateTime startUtc, DateTime endUtcExclusive) = BuildUtcRange(fromLocalDate, toLocalDate, localOffset);

        IQueryable<PaidBillHistoryProjection> query = _dbContext.Payments
            .AsNoTracking()
            .Where(payment => payment.Bill != null &&
                              payment.Bill.Status == "Paid" &&
                              payment.PaidAt >= startUtc &&
                              payment.PaidAt < endUtcExclusive)
            .Select(payment => new PaidBillHistoryProjection
            {
                PaymentId = payment.PaymentId,
                BillId = payment.BillId,
                BillIdText = payment.BillId.ToString(),
                BillCode = payment.Bill != null ? payment.Bill.BillCode : string.Empty,
                BillName = payment.Bill != null ? payment.Bill.BillName : string.Empty,
                TableName = payment.Bill != null &&
                            payment.Bill.TableSession != null &&
                            payment.Bill.TableSession.Table != null
                    ? payment.Bill.TableSession.Table.TableName
                    : string.Empty,
                Area = payment.Bill != null &&
                       payment.Bill.TableSession != null &&
                       payment.Bill.TableSession.Table != null
                    ? payment.Bill.TableSession.Table.Area
                    : string.Empty,
                PaymentMethod = payment.PaymentMethod,
                PaymentAmount = payment.Amount,
                BillFinalAmount = payment.Bill != null ? payment.Bill.FinalAmount : 0m,
                PaidAt = payment.PaidAt,
                ConfirmedByUserId = payment.ConfirmedBy,
                UpdatedAt = payment.UpdatedAt,
                UpdatedByUserId = payment.UpdatedBy,
                ChangeReason = payment.ChangeReason
            });

        if (!string.IsNullOrWhiteSpace(paymentMethod))
        {
            string normalizedPaymentMethod = paymentMethod.Trim();
            query = query.Where(x => x.PaymentMethod == normalizedPaymentMethod);
        }

        if (!string.IsNullOrWhiteSpace(tableName))
        {
            string normalizedTableName = tableName.Trim().ToLower();
            query = query.Where(x => x.TableName.ToLower().Contains(normalizedTableName));
        }

        if (!string.IsNullOrWhiteSpace(area))
        {
            string normalizedArea = area.Trim().ToLower();
            query = query.Where(x => x.Area.ToLower().Contains(normalizedArea));
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string normalizedKeyword = keyword.Trim().ToLower();
            query = query.Where(x =>
                x.BillIdText.Contains(normalizedKeyword) ||
                x.BillCode.ToLower().Contains(normalizedKeyword) ||
                x.BillName.ToLower().Contains(normalizedKeyword));
        }

        List<PaidBillHistoryProjection> items = await query
            .OrderByDescending(x => x.PaidAt)
            .ThenByDescending(x => x.PaymentId)
            .ToListAsync(cancellationToken);

        int[] userIds = items
            .SelectMany(x => new[] { x.ConfirmedByUserId, x.UpdatedByUserId ?? 0 })
            .Where(x => x > 0)
            .Distinct()
            .ToArray();

        Dictionary<int, string> userNames = await _dbContext.Users
            .AsNoTracking()
            .Where(x => userIds.Contains(x.UserId))
            .ToDictionaryAsync(x => x.UserId, x => x.FullName, cancellationToken);

        return items
            .Select(item => new PaidBillHistoryItemDto
            {
                PaymentId = item.PaymentId,
                BillId = item.BillId,
                BillCode = item.BillCode,
                BillName = item.BillName,
                TableName = item.TableName,
                Area = item.Area,
                PaymentMethod = item.PaymentMethod,
                PaymentAmount = item.PaymentAmount,
                BillFinalAmount = item.BillFinalAmount,
                PaidAt = item.PaidAt.Add(localOffset),
                ConfirmedByUserId = item.ConfirmedByUserId,
                ConfirmedByName = userNames.GetValueOrDefault(item.ConfirmedByUserId, $"User {item.ConfirmedByUserId}"),
                UpdatedAt = item.UpdatedAt?.Add(localOffset),
                UpdatedByUserId = item.UpdatedByUserId,
                UpdatedByName = item.UpdatedByUserId.HasValue
                    ? userNames.GetValueOrDefault(item.UpdatedByUserId.Value, $"User {item.UpdatedByUserId.Value}")
                    : string.Empty,
                ChangeReason = item.ChangeReason ?? string.Empty,
                IsCorrected = item.UpdatedAt.HasValue || item.UpdatedByUserId.HasValue || !string.IsNullOrWhiteSpace(item.ChangeReason)
            })
            .ToList();
    }

    public async Task<CancellationSummaryDto> GetCancellationSummaryByLocalDateAsync(
        DateTime localDate,
        TimeSpan localOffset,
        CancellationToken cancellationToken = default)
    {
        (DateTime startUtc, DateTime endUtc) = BuildUtcRange(localDate, localOffset);

        var cancelledBillsRaw = await _dbContext.Bills
            .AsNoTracking()
            .Where(x => x.Status == "Cancelled" &&
                        x.CancelledAt.HasValue &&
                        x.CancelledAt.Value >= startUtc &&
                        x.CancelledAt.Value < endUtc)
            .Select(x => new
            {
                x.BillId,
                x.BillCode,
                x.BillName,
                TableName = x.TableSession != null && x.TableSession.Table != null ? x.TableSession.Table.TableName : "",
                Area = x.TableSession != null && x.TableSession.Table != null ? x.TableSession.Table.Area : "",
                x.FinalAmount,
                x.CancelledAt,
                x.CancelledBy,
                CancelReason = x.CancelReason ?? ""
            })
            .ToListAsync(cancellationToken);

        var cancelledBillDetailsRaw = await _dbContext.BillDetailAdjustments
            .AsNoTracking()
            .Where(x => x.CreatedAt >= startUtc &&
                        x.CreatedAt < endUtc &&
                        (x.ChangeType == "CancelItem" || x.ChangeType == "ReduceQuantity"))
            .Select(x => new
            {
                ItemName = x.ItemName,
                Quantity = x.ChangedQuantity,
                TableName = x.Bill != null && x.Bill.TableSession != null && x.Bill.TableSession.Table != null ? x.Bill.TableSession.Table.TableName : "",
                Area = x.Bill != null && x.Bill.TableSession != null && x.Bill.TableSession.Table != null ? x.Bill.TableSession.Table.Area : "",
                CancelledAt = x.CreatedAt,
                CancelledBy = (int?)x.CreatedBy,
                CancelReason = x.Reason ?? "Khác",
                Source = "Bill"
            })
            .ToListAsync(cancellationToken);

        var cancelledOrderItemsRaw = await _dbContext.OrderItems
            .AsNoTracking()
            .Where(x => x.Order != null &&
                        x.Order.Status == "Cancelled" &&
                        x.Order.CancelledAt.HasValue &&
                        x.Order.CancelledAt.Value >= startUtc &&
                        x.Order.CancelledAt.Value < endUtc)
            .Select(x => new
            {
                ItemName = x.MenuItemNameSnapshot,
                Quantity = x.Quantity,
                TableName = x.Order != null && x.Order.TableSession != null && x.Order.TableSession.Table != null ? x.Order.TableSession.Table.TableName : "",
                Area = x.Order != null && x.Order.TableSession != null && x.Order.TableSession.Table != null ? x.Order.TableSession.Table.Area : "",
                CancelledAt = x.Order!.CancelledAt!.Value,
                CancelledBy = x.Order.CancelledBy,
                CancelReason = x.Order.CancelReason ?? "Hủy Order",
                Source = "Order"
            })
            .ToListAsync(cancellationToken);

        var allUserIds = cancelledBillsRaw.Select(x => x.CancelledBy ?? 0)
            .Concat(cancelledBillDetailsRaw.Select(x => x.CancelledBy ?? 0))
            .Concat(cancelledOrderItemsRaw.Select(x => x.CancelledBy ?? 0))
            .Where(x => x > 0)
            .Distinct()
            .ToArray();

        var userNames = await _dbContext.Users
            .AsNoTracking()
            .Where(x => allUserIds.Contains(x.UserId))
            .ToDictionaryAsync(x => x.UserId, x => x.FullName, cancellationToken);

        var cancelledBills = cancelledBillsRaw.Select(x => new CancelledBillDto
        {
            BillId = x.BillId,
            BillCode = x.BillCode,
            BillName = x.BillName,
            TableName = x.TableName,
            Area = x.Area,
            FinalAmount = x.FinalAmount,
            CancelledAt = x.CancelledAt.Value.Add(localOffset),
            CancelledByName = x.CancelledBy.HasValue ? userNames.GetValueOrDefault(x.CancelledBy.Value, $"User {x.CancelledBy.Value}") : "System",
            CancelReason = x.CancelReason
        }).ToList();

        var cancelledItems = cancelledBillDetailsRaw.Select(x => new CancelledItemDto
        {
            ItemName = x.ItemName,
            Quantity = x.Quantity,
            TableName = x.TableName,
            Area = x.Area,
            CancelledAt = x.CancelledAt.Add(localOffset),
            CancelledByName = x.CancelledBy.HasValue ? userNames.GetValueOrDefault(x.CancelledBy.Value, $"User {x.CancelledBy.Value}") : "System",
            CancelReason = x.CancelReason,
            Source = x.Source
        }).Concat(cancelledOrderItemsRaw.Select(x => new CancelledItemDto
        {
            ItemName = x.ItemName,
            Quantity = x.Quantity,
            TableName = x.TableName,
            Area = x.Area,
            CancelledAt = x.CancelledAt.Add(localOffset),
            CancelledByName = x.CancelledBy.HasValue ? userNames.GetValueOrDefault(x.CancelledBy.Value, $"User {x.CancelledBy.Value}") : "System",
            CancelReason = x.CancelReason,
            Source = x.Source
        })).OrderByDescending(x => x.CancelledAt).ToList();

        return new CancellationSummaryDto
        {
            CancelledBillCount = cancelledBills.Count,
            CancelledItemCount = cancelledItems.Count,
            CancelledBills = cancelledBills,
            CancelledItems = cancelledItems
        };
    }

    public async Task<IReadOnlyList<AssistantBusinessContextTextDto>> GetAssistantBusinessContextTextsAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        TimeSpan localOffset,
        CancellationToken cancellationToken = default)
    {
        (DateTime startUtc, DateTime endUtcExclusive) = BuildUtcRange(fromLocalDate, toLocalDate, localOffset);
        List<AssistantBusinessContextTextDto> items = [];

        var cancelledBills = await _dbContext.Bills
            .AsNoTracking()
            .Where(x => x.CancelledAt.HasValue &&
                        x.CancelledAt.Value >= startUtc &&
                        x.CancelledAt.Value < endUtcExclusive &&
                        x.CancelReason != null &&
                        x.CancelReason != "")
            .Select(x => new AssistantBusinessContextTextDto
            {
                SourceType = "BillCancellation",
                SourceId = x.BillId,
                OccurredAt = x.CancelledAt!.Value.Add(localOffset),
                Title = "Lý do hủy bill " + x.BillCode,
                Text = x.CancelReason!
            })
            .Take(100)
            .ToListAsync(cancellationToken);
        items.AddRange(cancelledBills);

        var billAdjustments = await _dbContext.BillDetailAdjustments
            .AsNoTracking()
            .Where(x => x.CreatedAt >= startUtc &&
                        x.CreatedAt < endUtcExclusive &&
                        x.Reason != "")
            .Select(x => new AssistantBusinessContextTextDto
            {
                SourceType = "BillItemAdjustment",
                SourceId = x.BillDetailAdjustmentId,
                OccurredAt = x.CreatedAt.Add(localOffset),
                Title = x.ChangeType + " - " + x.ItemName,
                Text = x.Reason
            })
            .Take(150)
            .ToListAsync(cancellationToken);
        items.AddRange(billAdjustments);

        var paymentChanges = await _dbContext.Payments
            .AsNoTracking()
            .Where(x => x.UpdatedAt.HasValue &&
                        x.UpdatedAt.Value >= startUtc &&
                        x.UpdatedAt.Value < endUtcExclusive &&
                        x.ChangeReason != null &&
                        x.ChangeReason != "")
            .Select(x => new AssistantBusinessContextTextDto
            {
                SourceType = "PaymentCorrection",
                SourceId = x.PaymentId,
                OccurredAt = x.UpdatedAt!.Value.Add(localOffset),
                Title = "Lý do sửa payment " + x.PaymentMethod,
                Text = x.ChangeReason!
            })
            .Take(100)
            .ToListAsync(cancellationToken);
        items.AddRange(paymentChanges);

        var orderNotes = await _dbContext.Orders
            .AsNoTracking()
            .Where(x => x.CreatedAt >= startUtc &&
                        x.CreatedAt < endUtcExclusive &&
                        ((x.CustomerNote != null && x.CustomerNote != "") ||
                         (x.SystemNote != null && x.SystemNote != "") ||
                         (x.CancelReason != null && x.CancelReason != "") ||
                         (x.PrintError != null && x.PrintError != "")))
            .Select(x => new
            {
                x.OrderId,
                x.OrderCode,
                x.CreatedAt,
                x.CustomerNote,
                x.SystemNote,
                x.CancelReason,
                x.PrintError
            })
            .Take(150)
            .ToListAsync(cancellationToken);

        foreach (var order in orderNotes)
        {
            AddText(items, "OrderCustomerNote", order.OrderId, order.CreatedAt.Add(localOffset), "Ghi chú khách order " + order.OrderCode, order.CustomerNote);
            AddText(items, "OrderSystemNote", order.OrderId, order.CreatedAt.Add(localOffset), "Ghi chú hệ thống order " + order.OrderCode, order.SystemNote);
            AddText(items, "OrderCancellation", order.OrderId, order.CreatedAt.Add(localOffset), "Lý do hủy order " + order.OrderCode, order.CancelReason);
            AddText(items, "OrderPrintError", order.OrderId, order.CreatedAt.Add(localOffset), "Lỗi in order " + order.OrderCode, order.PrintError);
        }

        var itemNotes = await _dbContext.OrderItems
            .AsNoTracking()
            .Where(x => x.CreatedAt >= startUtc &&
                        x.CreatedAt < endUtcExclusive &&
                        x.Note != null &&
                        x.Note != "")
            .Select(x => new AssistantBusinessContextTextDto
            {
                SourceType = "OrderItemNote",
                SourceId = x.OrderItemId,
                OccurredAt = x.CreatedAt.Add(localOffset),
                Title = "Ghi chú món " + x.MenuItemNameSnapshot,
                Text = x.Note!
            })
            .Take(150)
            .ToListAsync(cancellationToken);
        items.AddRange(itemNotes);

        return items
            .Where(x => !string.IsNullOrWhiteSpace(x.Text))
            .OrderByDescending(x => x.OccurredAt)
            .Take(250)
            .ToList();
    }

    private static void AddText(
        List<AssistantBusinessContextTextDto> items,
        string sourceType,
        int sourceId,
        DateTime occurredAt,
        string title,
        string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        items.Add(new AssistantBusinessContextTextDto
        {
            SourceType = sourceType,
            SourceId = sourceId,
            OccurredAt = occurredAt,
            Title = title,
            Text = text.Trim()
        });
    }

    private static (DateTime StartUtc, DateTime EndUtc) BuildUtcRange(DateTime localDate, TimeSpan localOffset)
    {
        DateTime localStart = DateTime.SpecifyKind(localDate.Date, DateTimeKind.Unspecified);
        DateTime localEnd = localStart.AddDays(1);
        return (
            new DateTimeOffset(localStart, localOffset).UtcDateTime,
            new DateTimeOffset(localEnd, localOffset).UtcDateTime);
    }

    private static (DateTime StartUtc, DateTime EndUtcExclusive) BuildUtcRange(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        TimeSpan localOffset)
    {
        DateTime localStart = DateTime.SpecifyKind(fromLocalDate.Date, DateTimeKind.Unspecified);
        DateTime localEndExclusive = DateTime.SpecifyKind(toLocalDate.Date.AddDays(1), DateTimeKind.Unspecified);
        return (
            new DateTimeOffset(localStart, localOffset).UtcDateTime,
            new DateTimeOffset(localEndExclusive, localOffset).UtcDateTime);
    }

    private sealed class PaidBillHistoryProjection
    {
        public int PaymentId { get; set; }
        public int BillId { get; set; }
        public string BillIdText { get; set; } = string.Empty;
        public string BillCode { get; set; } = string.Empty;
        public string BillName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal PaymentAmount { get; set; }
        public decimal BillFinalAmount { get; set; }
        public DateTime PaidAt { get; set; }
        public int ConfirmedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }
        public string? ChangeReason { get; set; }
    }
}
