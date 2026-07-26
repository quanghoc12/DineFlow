using System.Collections.Concurrent;
using System.Text;
using DineFlow.BusinessObjects.Reports;
using DineFlow.Repositories.Reports;

namespace DineFlow.Services.Reports;

internal interface IAssistantEmbeddingService
{
    float[] Embed(string text);
}

internal interface IAssistantVectorSearchService
{
    Task<IReadOnlyList<AssistantRetrievedContextDto>> SearchAsync(
        string query,
        DashboardAssistantPlan plan,
        DashboardAssistantSessionState sessionState,
        CancellationToken cancellationToken = default);
}

internal sealed class LocalHashEmbeddingService : IAssistantEmbeddingService
{
    private const int Dimensions = 128;

    public float[] Embed(string text)
    {
        float[] vector = new float[Dimensions];
        foreach (string token in Tokenize(text))
        {
            int hash = StableHash(token);
            int index = Math.Abs(hash % Dimensions);
            vector[index] += hash % 2 == 0 ? 1f : -1f;
        }

        Normalize(vector);
        return vector;
    }

    private static IEnumerable<string> Tokenize(string text)
    {
        StringBuilder builder = new();
        foreach (char character in NormalizeText(text))
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                continue;
            }

            if (builder.Length > 1)
            {
                yield return builder.ToString();
            }

            builder.Clear();
        }

        if (builder.Length > 1)
        {
            yield return builder.ToString();
        }
    }

    private static string NormalizeText(string text) => text.Trim().ToLowerInvariant();

    private static int StableHash(string value)
    {
        unchecked
        {
            int hash = 23;
            foreach (char character in value)
            {
                hash = hash * 31 + character;
            }

            return hash;
        }
    }

    private static void Normalize(float[] vector)
    {
        double sum = vector.Sum(value => value * value);
        if (sum <= 0)
        {
            return;
        }

        float length = (float)Math.Sqrt(sum);
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] /= length;
        }
    }
}

internal sealed class AssistantVectorSearchService : IAssistantVectorSearchService
{
    private static readonly HashSet<DashboardAssistantIntent> SearchableIntents =
    [
        DashboardAssistantIntent.Revenue,
        DashboardAssistantIntent.Payment,
        DashboardAssistantIntent.Cancellation,
        DashboardAssistantIntent.Operations
    ];

    private readonly IReportRepository _reportRepository;
    private readonly IAssistantEmbeddingService _embeddingService;
    private readonly ConcurrentDictionary<string, AssistantVectorSearchCacheItem> _cache = new();

    public AssistantVectorSearchService(
        IReportRepository reportRepository,
        IAssistantEmbeddingService embeddingService)
    {
        _reportRepository = reportRepository;
        _embeddingService = embeddingService;
    }

    public async Task<IReadOnlyList<AssistantRetrievedContextDto>> SearchAsync(
        string query,
        DashboardAssistantPlan plan,
        DashboardAssistantSessionState sessionState,
        CancellationToken cancellationToken = default)
    {
        if (!SearchableIntents.Contains(plan.Intent))
        {
            return [];
        }

        string cacheKey = $"{plan.Intent}:{plan.FromDate:yyyy-MM-dd}:{plan.ToDate:yyyy-MM-dd}:{NormalizeForKey(query)}";
        if (_cache.TryGetValue(cacheKey, out AssistantVectorSearchCacheItem? cached) &&
            DateTimeOffset.UtcNow - cached.CreatedAt <= TimeSpan.FromMinutes(5))
        {
            return cached.Results;
        }

        IReadOnlyList<AssistantBusinessContextTextDto> sourceTexts =
            await _reportRepository.GetAssistantBusinessContextTextsAsync(
                plan.FromDate,
                plan.ToDate,
                TimeSpan.FromHours(7),
                cancellationToken);

        if (sourceTexts.Count == 0)
        {
            _cache[cacheKey] = new AssistantVectorSearchCacheItem([], DateTimeOffset.UtcNow);
            return [];
        }

        float[] queryVector = _embeddingService.Embed(query);
        List<AssistantRetrievedContextDto> results = sourceTexts
            .Select(source =>
            {
                string searchableText = $"{source.Title}. {source.Text}";
                float score = Cosine(queryVector, _embeddingService.Embed(searchableText));
                return new AssistantRetrievedContextDto
                {
                    SourceType = source.SourceType,
                    SourceId = source.SourceId,
                    OccurredAt = source.OccurredAt,
                    Title = source.Title,
                    Text = source.Text,
                    Score = score
                };
            })
            .Where(x => x.Score > 0.05f)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.OccurredAt)
            .Take(8)
            .ToList();

        _cache[cacheKey] = new AssistantVectorSearchCacheItem(results, DateTimeOffset.UtcNow);
        return results;
    }

    private static float Cosine(float[] left, float[] right)
    {
        float score = 0;
        int length = Math.Min(left.Length, right.Length);
        for (int i = 0; i < length; i++)
        {
            score += left[i] * right[i];
        }

        return score;
    }

    private static string NormalizeForKey(string text) =>
        new(text.Trim().ToLowerInvariant()
            .Where(character => char.IsLetterOrDigit(character) || char.IsWhiteSpace(character))
            .ToArray());
}

internal sealed class AssistantRetrievedContextDto
{
    public string SourceType { get; set; } = string.Empty;
    public int SourceId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public float Score { get; set; }
}

internal sealed record AssistantVectorSearchCacheItem(
    IReadOnlyList<AssistantRetrievedContextDto> Results,
    DateTimeOffset CreatedAt);
