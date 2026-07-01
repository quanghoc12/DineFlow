using DineFlow.BusinessObjects.Reports;
using DineFlow.Repositories.Reports;
using DineFlow.Services.Common;

namespace DineFlow.Services.Reports;

public sealed class TopSellingItemReportService : ITopSellingItemReportService
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

    private readonly IReportRepository _reportRepository;

    public TopSellingItemReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public Task<IReadOnlyList<TopSellingItemDto>> GetTopSellingItemsAsync(
        DateTime fromLocalDate,
        DateTime toLocalDate,
        int topCount,
        CancellationToken cancellationToken = default)
    {
        if (fromLocalDate.Date > toLocalDate.Date)
        {
            throw new BusinessException("TOP_SELLING_DATE_RANGE_INVALID", "Từ ngày không được lớn hơn đến ngày.");
        }

        if (topCount <= 0)
        {
            throw new BusinessException("TOP_SELLING_TOP_COUNT_INVALID", "Số lượng top phải lớn hơn 0.");
        }

        return _reportRepository.GetTopSellingItemsByLocalDateRangeAsync(
            fromLocalDate.Date,
            toLocalDate.Date,
            VietnamOffset,
            topCount,
            cancellationToken);
    }
}
