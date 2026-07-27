using DineFlow.BusinessObjects.Tables;

namespace DineFlow.Services.Tables;

public interface ITableOtpService
{
    Task<IReadOnlyList<StaffTableOtpDto>> GetAsync(
        TableOtpFilter filter,
        string currentUserRole,
        CancellationToken cancellationToken = default);

    Task<StaffTableOtpDto> ResetAsync(
        int tableId,
        string currentUserRole,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StaffTableOtpDto>> ResetBatchAsync(
        ResetTableOtpBatchRequest request,
        string currentUserRole,
        CancellationToken cancellationToken = default);

    Task RotateForClosedSessionAsync(int tableId, CancellationToken cancellationToken = default);
}
