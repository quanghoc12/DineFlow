namespace DineFlow.Services.Requests;

public class CreateServiceRequestRequest
{
    public int TableSessionId { get; set; }
    public int? SessionCustomerId { get; set; }
    public string? ClientToken { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Message { get; set; }
}

public class ServiceRequestDto
{
    public int RequestId { get; set; }
    public int TableSessionId { get; set; }
    public int? SessionCustomerId { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public interface IServiceRequestService
{
    Task<ServiceRequestDto> CreateServiceRequestAsync(CreateServiceRequestRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceRequestDto>> GetConfirmedRequestsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceRequestDto>> GetRequestsAsync(
        DateTime? from = null,
        DateTime? to = null,
        string? status = null,
        CancellationToken cancellationToken = default);
    Task<ServiceRequestDto> ConfirmRequestAsync(int requestId, int currentUserId, CancellationToken cancellationToken = default);
}
