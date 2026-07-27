using System.Collections.ObjectModel;
using System.Windows.Input;
using DineFlow.BusinessObjects.Reports;
using DineFlow.WPFApp.Core;
using DineFlow.WPFApp.Services.Api;

namespace DineFlow.WPFApp.Features.Reports.Dashboard;

public sealed class DashboardAssistantViewModel : BaseViewModel
{
    private readonly StaffOrderApiClient _apiClient;
    private readonly DashboardChatSessionStore _sessionStore;

    private string _currentMessage = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;
    private DashboardAssistantContextDto _context = new();

    public DashboardAssistantViewModel(DashboardChatSessionStore sessionStore)
    {
        _sessionStore = sessionStore;
        _apiClient = new StaffOrderApiClient();
        SendCommand = new AsyncRelayCommand(SendAsync, CanSend);
    }

    public ObservableCollection<DashboardAssistantMessageDto> Messages => _sessionStore.Messages;
    public ObservableCollection<string> SuggestedQuestions { get; } = [];

    public string CurrentMessage
    {
        get => _currentMessage;
        set
        {
            if (SetProperty(ref _currentMessage, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public ICommand SendCommand { get; }

    public void UpdateContext(DashboardAssistantContextDto context)
    {
        _context = context;
    }

    public void InvalidateDataCache()
    {
        _sessionStore.InvalidateDataCache();
    }

    public void UseSuggestedQuestion(string question)
    {
        CurrentMessage = question;
    }

    private bool CanSend() => !IsBusy && !string.IsNullOrWhiteSpace(CurrentMessage);

    private async Task SendAsync()
    {
        string message = CurrentMessage.Trim();
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        ErrorMessage = string.Empty;
        IsBusy = true;
        CurrentMessage = string.Empty;

        List<DashboardAssistantMessageDto> history = Messages.ToList();
        Messages.Add(new DashboardAssistantMessageDto
        {
            Role = "user",
            Content = message,
            CreatedAt = DateTime.Now
        });

        try
        {
            DashboardAssistantChatResponseDto response = await _apiClient.ChatWithDashboardAssistantAsync(
                new DashboardAssistantChatRequestDto
                {
                    SessionId = $"{_sessionStore.SessionId}:{_sessionStore.DataSessionId}",
                    Message = message,
                    Messages = history,
                    Context = _context
                });

            Messages.Add(new DashboardAssistantMessageDto
            {
                Role = "assistant",
                Content = BuildAssistantContent(response),
                CreatedAt = DateTime.Now
            });

            SuggestedQuestions.Clear();
            foreach (string question in response.SuggestedQuestions)
            {
                SuggestedQuestions.Add(question);
            }
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            Messages.Add(new DashboardAssistantMessageDto
            {
                Role = "assistant",
                Content = "Không thể lấy phản hồi từ chatbot AI. " + exception.Message,
                CreatedAt = DateTime.Now
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string BuildAssistantContent(DashboardAssistantChatResponseDto response)
    {
        List<string> lines = [];
        if (!string.IsNullOrWhiteSpace(response.UsedDataRange))
        {
            string cacheText = response.UsedCachedData ? "cache session" : "dữ liệu mới";
            lines.Add($"Dữ liệu: {response.UsedDataRange} ({cacheText})");
        }

        lines.Add(response.Reply);

        foreach (string warning in response.Warnings)
        {
            lines.Add($"Lưu ý: {warning}");
        }

        return string.Join(Environment.NewLine + Environment.NewLine, lines.Where(x => !string.IsNullOrWhiteSpace(x)));
    }
}
