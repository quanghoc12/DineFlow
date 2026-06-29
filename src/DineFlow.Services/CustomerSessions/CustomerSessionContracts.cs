namespace DineFlow.Services.CustomerSessions;

public class ScanCustomerSessionRequest
{
    public string QrToken { get; set; } = string.Empty;
    public string? ClientToken { get; set; }
}

public class UpdateCustomerNameRequest
{
    public string ClientToken { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class CustomerSessionDto
{
    public int TableId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public int TableSessionId { get; set; }
    public int SessionCustomerId { get; set; }
    public string ClientToken { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool RequiresName { get; set; }
}

public class CustomerMessageDto
{
    public string MessageType { get; set; } = string.Empty;
    public int SourceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CustomerMessageItemDto> Items { get; set; } = [];
}

public class CustomerMessageItemDto
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public decimal LineTotal { get; set; }
    public List<string> Choices { get; set; } = [];
}

public interface ICustomerSessionService
{
    Task<CustomerSessionDto> ScanAsync(ScanCustomerSessionRequest request, CancellationToken cancellationToken = default);
    Task<CustomerSessionDto> UpdateCustomerNameAsync(UpdateCustomerNameRequest request, CancellationToken cancellationToken = default);
    Task<CustomerSessionDto> GetCurrentAsync(string clientToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerMessageDto>> GetMessagesAsync(string clientToken, CancellationToken cancellationToken = default);
}
