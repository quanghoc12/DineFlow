using System.Collections.ObjectModel;
using DineFlow.BusinessObjects.Reports;

namespace DineFlow.WPFApp.Features.Dashboard;

public sealed class DashboardChatSessionStore
{
    public string SessionId { get; } = Guid.NewGuid().ToString("N");
    public string DataSessionId { get; private set; } = Guid.NewGuid().ToString("N");
    public ObservableCollection<DashboardAssistantMessageDto> Messages { get; } = [];

    public void InvalidateDataCache()
    {
        DataSessionId = Guid.NewGuid().ToString("N");
    }
}
