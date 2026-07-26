using DineFlow.BusinessObjects.Reports;

namespace DineFlow.Services.Reports;

public interface IDashboardAssistantService
{
    Task<DashboardAssistantChatResponseDto> ChatAsync(
        DashboardAssistantChatRequestDto request,
        int currentUserId,
        string currentUserRole,
        CancellationToken cancellationToken = default);
}

public interface IDeepSeekChatClient
{
    Task<string> CompleteAsync(
        IReadOnlyList<DeepSeekChatMessage> messages,
        CancellationToken cancellationToken = default);
}

public sealed class DeepSeekChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
