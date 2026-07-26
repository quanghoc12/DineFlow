using DineFlow.BusinessObjects.Reports;

namespace DineFlow.Services.Reports;

internal interface IDashboardAssistantRuleGuard
{
    DashboardAssistantRuleResult Check(string message);
}

internal sealed class DashboardAssistantRuleGuard : IDashboardAssistantRuleGuard
{
    private const string ScopeMessage = "Chatbot chỉ hỗ trợ phân tích dữ liệu nhà hàng DineFlow như doanh thu, order, bill, thanh toán, món bán chạy và hủy món/bill.";

    public DashboardAssistantRuleResult Check(string message)
    {
        string normalized = Normalize(message);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return DashboardAssistantRuleResult.Allow();
        }

        if (IsGreeting(normalized))
        {
            return DashboardAssistantRuleResult.Block("Xin chào! Tôi có thể hỗ trợ phân tích dashboard DineFlow: doanh thu, order, bill, thanh toán, món bán chạy và hủy món/bill.");
        }

        if (AsksCapabilities(normalized))
        {
            return DashboardAssistantRuleResult.Block("Tôi có thể phân tích doanh thu theo ngày/khoảng ngày, so sánh 7 hoặc 30 ngày, xem món bán chạy, phương thức thanh toán, payment correction, hủy món/hủy bill và cảnh báo vận hành.");
        }

        if (IsClearlyOutOfScope(normalized) && !HasDineFlowKeyword(normalized))
        {
            return DashboardAssistantRuleResult.Block(ScopeMessage);
        }

        return DashboardAssistantRuleResult.Allow();
    }

    private static bool IsGreeting(string text) =>
        text is "hi" or "hello" or "xin chao" or "chao" or "hey" or "alo";

    private static bool AsksCapabilities(string text) =>
        ContainsAny(text, "ban lam duoc gi", "co the lam gi", "chatbot lam gi", "help", "tro giup");

    private static bool IsClearlyOutOfScope(string text) =>
        ContainsAny(
            text,
            "thoi tiet",
            "weather",
            "bong da",
            "football",
            "chinh tri",
            "politics",
            "viet code",
            "lap trinh",
            "giai bai",
            "toan hoc",
            "lich su",
            "phim",
            "du lich",
            "tinh yeu",
            "ke chuyen",
            "tin tuc");

    private static bool HasDineFlowKeyword(string text) =>
        ContainsAny(
            text,
            "dineflow",
            "doanh thu",
            "revenue",
            "order",
            "bill",
            "hoa don",
            "thanh toan",
            "payment",
            "mon",
            "menu",
            "ban chay",
            "huy",
            "cancel",
            "dashboard",
            "nha hang",
            "ban",
            "loi in");

    private static bool ContainsAny(string text, params string[] values) =>
        values.Any(value => text.Contains(value, StringComparison.Ordinal));

    private static string Normalize(string text)
    {
        string normalized = text.Trim().ToLowerInvariant();
        string[] source = ["đ", "á", "à", "ả", "ã", "ạ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "é", "è", "ẻ", "ẽ", "ẹ", "ê", "ế", "ề", "ể", "ễ", "ệ", "í", "ì", "ỉ", "ĩ", "ị", "ó", "ò", "ỏ", "õ", "ọ", "ô", "ố", "ồ", "ổ", "ỗ", "ộ", "ơ", "ớ", "ờ", "ở", "ỡ", "ợ", "ú", "ù", "ủ", "ũ", "ụ", "ư", "ứ", "ừ", "ử", "ữ", "ự", "ý", "ỳ", "ỷ", "ỹ", "ỵ"];
        string[] target = ["d", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "i", "i", "i", "i", "i", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "y", "y", "y", "y", "y"];

        for (int i = 0; i < source.Length; i++)
        {
            normalized = normalized.Replace(source[i], target[i], StringComparison.Ordinal);
        }

        return normalized;
    }
}

internal sealed class DashboardAssistantRuleResult
{
    public bool IsBlocked { get; private init; }
    public string Reply { get; private init; } = string.Empty;

    public static DashboardAssistantRuleResult Allow() => new();

    public static DashboardAssistantRuleResult Block(string reply) => new()
    {
        IsBlocked = true,
        Reply = reply
    };
}
