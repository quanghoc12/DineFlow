using System.Globalization;

namespace DineFlow.Services.Reports;

internal enum DashboardAssistantIntent
{
    Overview,
    Revenue,
    TopSelling,
    Payment,
    Cancellation,
    Operations
}

internal sealed class DashboardAssistantPlan
{
    public DashboardAssistantIntent Intent { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public bool ReusedPreviousRange { get; set; }
    public string RangeLabel { get; set; } = string.Empty;
}

internal interface IDashboardAssistantContextPlanner
{
    DashboardAssistantPlan Plan(
        string message,
        DashboardAssistantSessionState sessionState,
        DateTime localToday);
}

internal sealed class DashboardAssistantContextPlanner : IDashboardAssistantContextPlanner
{
    public DashboardAssistantPlan Plan(
        string message,
        DashboardAssistantSessionState sessionState,
        DateTime localToday)
    {
        string normalized = Normalize(message);
        DashboardAssistantIntent intent = DetermineIntent(normalized);
        (DateTime From, DateTime To, bool HasExplicitRange, string Label) range = DetermineRange(normalized, localToday);

        bool shouldReusePreviousRange = !range.HasExplicitRange &&
                                        sessionState.LastFromDate.HasValue &&
                                        sessionState.LastToDate.HasValue &&
                                        LooksLikeFollowUp(normalized);

        DateTime fromDate = shouldReusePreviousRange ? sessionState.LastFromDate!.Value : range.From;
        DateTime toDate = shouldReusePreviousRange ? sessionState.LastToDate!.Value : range.To;

        return new DashboardAssistantPlan
        {
            Intent = intent,
            FromDate = fromDate.Date,
            ToDate = toDate.Date,
            ReusedPreviousRange = shouldReusePreviousRange,
            RangeLabel = shouldReusePreviousRange
                ? FormatRange(fromDate, toDate)
                : range.Label
        };
    }

    private static DashboardAssistantIntent DetermineIntent(string message)
    {
        if (ContainsAny(message, "so sanh", "compare", "hon", "kem", "chenh lech", "tang", "giam"))
        {
            return DashboardAssistantIntent.Revenue;
        }

        if (ContainsAny(message, "thanh toan", "payment", "tien mat", "chuyen khoan", "the", "card", "bank", "cash"))
        {
            return DashboardAssistantIntent.Payment;
        }

        if (ContainsAny(message, "huy", "cancel", "cancelled"))
        {
            return DashboardAssistantIntent.Cancellation;
        }

        if (ContainsAny(message, "mon", "ban chay", "top", "item", "menu", "best seller", "selling"))
        {
            return DashboardAssistantIntent.TopSelling;
        }

        if (ContainsAny(message, "ban", "cho thanh toan", "dang phuc vu", "loi in", "print", "order loi"))
        {
            return DashboardAssistantIntent.Operations;
        }

        if (ContainsAny(message, "doanh thu", "revenue", "bill", "hoa don", "trung binh", "giam", "tang", "kinh doanh"))
        {
            return DashboardAssistantIntent.Revenue;
        }

        return DashboardAssistantIntent.Overview;
    }

    private static (DateTime From, DateTime To, bool HasExplicitRange, string Label) DetermineRange(
        string message,
        DateTime localToday)
    {
        if (ContainsAny(message, "hom qua", "yesterday"))
        {
            DateTime yesterday = localToday.AddDays(-1);
            bool comparesWithToday = ContainsAny(message, "hom nay", "hien tai", "today", "so sanh", "compare", "hon", "kem", "chenh lech");
            DateTime toDate = comparesWithToday ? localToday : yesterday;
            return (yesterday, toDate, true, FormatRange(yesterday, toDate));
        }

        if (ContainsAny(message, "ngay truoc", "ngay truoc do", "previous day"))
        {
            DateTime previousDay = localToday.AddDays(-1);
            bool comparesWithToday = ContainsAny(message, "hom nay", "hien tai", "today", "so sanh", "compare", "hon", "kem", "chenh lech");
            DateTime toDate = comparesWithToday ? localToday : previousDay;
            return (previousDay, toDate, true, FormatRange(previousDay, toDate));
        }

        if (ContainsAny(message, "7 ngay", "bay ngay", "week", "tuan"))
        {
            DateTime from = localToday.AddDays(-6);
            return (from, localToday, true, FormatRange(from, localToday));
        }

        if (ContainsAny(message, "30 ngay", "ba muoi ngay", "month", "thang nay", "thang hien tai"))
        {
            DateTime from = message.Contains("30 ngay", StringComparison.Ordinal)
                ? localToday.AddDays(-29)
                : new DateTime(localToday.Year, localToday.Month, 1);

            return (from, localToday, true, FormatRange(from, localToday));
        }

        if (ContainsAny(message, "hom nay", "hien tai", "today"))
        {
            return (localToday, localToday, true, FormatRange(localToday, localToday));
        }

        DateTime? explicitDate = TryParseVietnameseDate(message, localToday.Year);
        if (explicitDate.HasValue)
        {
            return (explicitDate.Value, explicitDate.Value, true, FormatRange(explicitDate.Value, explicitDate.Value));
        }

        if (ContainsAny(message, "so sanh", "compare", "hon", "kem", "chenh lech"))
        {
            DateTime yesterday = localToday.AddDays(-1);
            return (yesterday, localToday, false, FormatRange(yesterday, localToday));
        }

        return (localToday, localToday, false, FormatRange(localToday, localToday));
    }

    private static DateTime? TryParseVietnameseDate(string message, int fallbackYear)
    {
        string[] formats = ["d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy", "dd/MM/yyyy", "d-M-yyyy", "dd-MM-yyyy"];
        foreach (string word in message.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (DateTime.TryParseExact(word, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
            {
                return parsed.Date;
            }

            if (DateTime.TryParseExact($"{word}/{fallbackYear}", ["d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy", "dd/MM/yyyy"], CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                return parsed.Date;
            }
        }

        return null;
    }

    private static bool LooksLikeFollowUp(string message) =>
        ContainsAny(message, "vay", "thi sao", "con", "tiep", "no", "do", "range nay", "khoang nay") ||
        !ContainsAny(message, "hom nay", "hien tai", "today", "ngay", "thang", "tuan", "week", "month");

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

    private static string FormatRange(DateTime fromDate, DateTime toDate) =>
        fromDate.Date == toDate.Date
            ? fromDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
            : $"{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}";
}
