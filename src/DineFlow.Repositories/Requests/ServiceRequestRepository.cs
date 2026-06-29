using DineFlow.BusinessObjects.Requests;
using DineFlow.DataAccessObjects.Requests;

namespace DineFlow.Repositories.Requests;

public class ServiceRequestRepository : IServiceRequestRepository
{
    private readonly IServiceRequestDao _serviceRequestDao;

    public ServiceRequestRepository(IServiceRequestDao serviceRequestDao)
    {
        _serviceRequestDao = serviceRequestDao;
    }

    public async Task<IReadOnlyList<ServiceRequest>> GetConfirmedRequestsAsync(CancellationToken cancellationToken = default)
    {
        return await _serviceRequestDao.GetConfirmedRequestsAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceRequest>> GetBySessionAsync(
        int tableSessionId,
        CancellationToken cancellationToken = default)
    {
        return await _serviceRequestDao.GetBySessionAsync(tableSessionId, cancellationToken);
    }

    public Task<ServiceRequest?> GetByIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        return _serviceRequestDao.GetByIdAsync(requestId, cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceRequest>> GetRequestsAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        return await _serviceRequestDao.GetRequestsAsync(from, to, status, cancellationToken);
    }

    public async Task AddServiceRequestAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default)
    {
        await _serviceRequestDao.AddServiceRequestAsync(serviceRequest, cancellationToken);
    }
}
