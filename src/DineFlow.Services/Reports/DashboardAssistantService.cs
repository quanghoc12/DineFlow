using System.Text.Json;
using DineFlow.BusinessObjects.Auth;
using DineFlow.BusinessObjects.Reports;
using DineFlow.Services.Common;

namespace DineFlow.Services.Reports;

internal sealed class DashboardAssistantService : IDashboardAssistantService
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private readonly IDashboardAssistantSessionCache _sessionCache;
    private readonly IDashboardAssistantContextPlanner _contextPlanner;
    private readonly IDashboardAssistantDataProvider _dataProvider;
    private readonly IDashboardAssistantRuleGuard _ruleGuard;
    private readonly IAssistantVectorSearchService _vectorSearchService;
    private readonly IDeepSeekChatClient _deepSeekChatClient;

    public DashboardAssistantService(
        IDashboardAssistantSessionCache sessionCache,
        IDashboardAssistantContextPlanner contextPlanner,
        IDashboardAssistantDataProvider dataProvider,
        IDashboardAssistantRuleGuard ruleGuard,
        IAssistantVectorSearchService vectorSearchService,
        IDeepSeekChatClient deepSeekChatClient)
    {
        _sessionCache = sessionCache;
        _contextPlanner = contextPlanner;
        _dataProvider = dataProvider;
        _ruleGuard = ruleGuard;
        _vectorSearchService = vectorSearchService;
        _deepSeekChatClient = deepSeekChatClient;
    }

    public async Task<DashboardAssistantChatResponseDto> ChatAsync(
        DashboardAssistantChatRequestDto request,
        int currentUserId,
        string currentUserRole,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!AuthRoles.CanManage(currentUserRole))
        {
            throw new BusinessException("DASHBOARD_ASSISTANT_FORBIDDEN", "Chỉ Admin hoặc Chủ nhà hàng được sử dụng chatbot AI.");
        }

        string message = request.Message.Trim();
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new BusinessException("DASHBOARD_ASSISTANT_MESSAGE_EMPTY", "Nội dung chat không được để trống.");
        }

        if (message.Length > 2000)
        {
            throw new BusinessException("DASHBOARD_ASSISTANT_MESSAGE_TOO_LONG", "Nội dung chat tối đa 2.000 ký tự.");
        }

        DashboardAssistantRuleResult ruleResult = _ruleGuard.Check(message);
        if (ruleResult.IsBlocked)
        {
            return new DashboardAssistantChatResponseDto
            {
                Reply = ruleResult.Reply,
                SuggestedQuestions = BuildSuggestedQuestions(DashboardAssistantIntent.Overview),
                UsedDataRange = "Rule",
                UsedCachedData = true
            };
        }

        DashboardAssistantSessionState sessionState = _sessionCache.GetOrCreate(request.SessionId, currentUserId);
        DateTime localToday = DateTimeOffset.UtcNow.ToOffset(VietnamOffset).DateTime.Date;
        DashboardAssistantPlan plan = _contextPlanner.Plan(message, sessionState, localToday);
        if ((plan.ToDate - plan.FromDate).TotalDays > 89)
        {
            plan.FromDate = plan.ToDate.AddDays(-89);
            plan.RangeLabel = $"{plan.FromDate:dd/MM/yyyy} - {plan.ToDate:dd/MM/yyyy}";
        }

        DashboardAssistantDataContext dataContext = await _dataProvider.GetDataAsync(
            plan,
            sessionState,
            cancellationToken);

        IReadOnlyList<AssistantRetrievedContextDto> retrievedContext = await _vectorSearchService.SearchAsync(
            message,
            plan,
            sessionState,
            cancellationToken);

        List<DeepSeekChatMessage> deepSeekMessages = BuildDeepSeekMessages(request, dataContext, retrievedContext);
        string reply = await _deepSeekChatClient.CompleteAsync(deepSeekMessages, cancellationToken);

        return new DashboardAssistantChatResponseDto
        {
            Reply = reply,
            SuggestedQuestions = BuildSuggestedQuestions(plan.Intent),
            UsedDataRange = dataContext.RangeLabel,
            UsedCachedData = dataContext.UsedCachedData,
            Warnings = dataContext.ReusedPreviousRange
                ? ["Đang dùng lại khoảng thời gian từ câu hỏi trước trong cùng phiên chat."]
                : []
        };
    }

    private static List<DeepSeekChatMessage> BuildDeepSeekMessages(
        DashboardAssistantChatRequestDto request,
        DashboardAssistantDataContext dataContext,
        IReadOnlyList<AssistantRetrievedContextDto> retrievedContext)
    {
        string dataJson = JsonSerializer.Serialize(dataContext, JsonOptions);
        string retrievedContextJson = JsonSerializer.Serialize(retrievedContext, JsonOptions);

        List<DeepSeekChatMessage> messages =
        [
            new()
            {
                Role = "system",
                Content = """
Bạn là trợ lý phân tích kinh doanh cho nhà hàng DineFlow.
Chỉ dùng dữ liệu JSON được cung cấp trong prompt. Nếu dữ liệu chưa đủ, hãy nói rõ chưa đủ dữ liệu.
Không bịa số liệu, không tự tạo giả định về database, không đề xuất thao tác ghi dữ liệu.
Nếu dữ liệu có DailyRevenue thì danh sách đó đã bao gồm đủ từng ngày trong range; Revenue = 0 nghĩa là ngày đó đã được kiểm tra và không có bill đã thanh toán, không phải thiếu dữ liệu.
StructuredMetricsJson là nguồn duy nhất cho mọi con số. RetrievedBusinessContext chỉ là ngữ cảnh văn bản bổ sung để giải thích nguyên nhân hoặc ý nghĩa nghiệp vụ; không dùng RetrievedBusinessContext để tự tạo số doanh thu/bill/order.
Trả lời bằng tiếng Việt, ngắn gọn, thực dụng cho Admin/Chủ nhà hàng.
Ưu tiên cấu trúc: nhận định chính, số liệu nổi bật, rủi ro nếu có, 2-4 gợi ý hành động.
"""
            },
            new()
            {
                Role = "system",
                Content = "StructuredMetricsJson:\n" + dataJson
            },
            new()
            {
                Role = "system",
                Content = "RetrievedBusinessContext:\n" + retrievedContextJson
            }
        ];

        foreach (DashboardAssistantMessageDto historyItem in request.Messages
                     .Where(x => IsAllowedRole(x.Role) && !string.IsNullOrWhiteSpace(x.Content))
                     .TakeLast(12))
        {
            messages.Add(new DeepSeekChatMessage
            {
                Role = historyItem.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase) ? "assistant" : "user",
                Content = historyItem.Content.Length > 1500
                    ? historyItem.Content[..1500]
                    : historyItem.Content
            });
        }

        messages.Add(new DeepSeekChatMessage
        {
            Role = "user",
            Content = request.Message.Trim()
        });

        return messages;
    }

    private static bool IsAllowedRole(string role) =>
        role.Equals("user", StringComparison.OrdinalIgnoreCase) ||
        role.Equals("assistant", StringComparison.OrdinalIgnoreCase);

    private static List<string> BuildSuggestedQuestions(DashboardAssistantIntent intent)
    {
        return intent switch
        {
            DashboardAssistantIntent.TopSelling =>
            [
                "Món nào nên đẩy bán thêm?",
                "Top món này đóng góp doanh thu ra sao?",
                "Có món bán nhiều nhưng doanh thu thấp không?"
            ],
            DashboardAssistantIntent.Payment =>
            [
                "Phương thức thanh toán nào chiếm nhiều nhất?",
                "Có payment nào đã bị chỉnh sửa không?",
                "Tỷ trọng tiền mặt và chuyển khoản ra sao?"
            ],
            DashboardAssistantIntent.Cancellation =>
            [
                "Lý do hủy nào đáng chú ý?",
                "Món nào bị hủy nhiều nhất?",
                "Hủy bill hôm nay có bất thường không?"
            ],
            DashboardAssistantIntent.Operations =>
            [
                "Bàn chờ thanh toán có đáng lo không?",
                "Có lỗi in order cần xử lý không?",
                "Tình hình vận hành hiện tại thế nào?"
            ],
            _ =>
            [
                "So sánh 7 ngày gần đây giúp tôi",
                "Vì sao doanh thu thay đổi?",
                "Món nào bán tốt nhất?"
            ]
        };
    }
}
