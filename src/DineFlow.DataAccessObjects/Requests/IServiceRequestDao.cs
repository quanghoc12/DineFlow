using DineFlow.BusinessObjects.Requests;

namespace DineFlow.DataAccessObjects.Requests;

public interface IServiceRequestDao
{
    Task<IReadOnlyList<ServiceRequest>> GetConfirmedRequestsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceRequest>> GetBySessionAsync(int tableSessionId, CancellationToken cancellationToken = default);
    Task<ServiceRequest?> GetByIdAsync(int requestId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceRequest>> GetRequestsAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? status = null,
        CancellationToken cancellationToken = default);
    Task AddServiceRequestAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default);
}
