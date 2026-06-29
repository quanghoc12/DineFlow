using DineFlow.BusinessObjects.Requests;
using DineFlow.DataAccessObjects.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.Requests;

public class ServiceRequestDao : IServiceRequestDao
{
    private readonly AppDbContext _dbContext;

    public ServiceRequestDao(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ServiceRequest>> GetConfirmedRequestsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceRequests
            .Where(x => x.Status == "Pending")
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceRequest>> GetBySessionAsync(
        int tableSessionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ServiceRequests
            .Where(x => x.TableSessionId == tableSessionId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<ServiceRequest?> GetByIdAsync(int requestId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ServiceRequests.FirstOrDefaultAsync(x => x.RequestId == requestId, cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceRequest>> GetRequestsAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ServiceRequest> query = _dbContext.ServiceRequests;

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= DateTime.SpecifyKind(from.Value, DateTimeKind.Utc));
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= DateTime.SpecifyKind(to.Value, DateTimeKind.Utc));
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddServiceRequestAsync(ServiceRequest serviceRequest, CancellationToken cancellationToken = default)
    {
        await _dbContext.ServiceRequests.AddAsync(serviceRequest, cancellationToken);
    }
}
