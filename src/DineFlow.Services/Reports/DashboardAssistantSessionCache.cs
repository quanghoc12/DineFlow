using System.Collections.Concurrent;

namespace DineFlow.Services.Reports;

internal interface IDashboardAssistantSessionCache
{
    DashboardAssistantSessionState GetOrCreate(string sessionId, int userId);
}

internal sealed class DashboardAssistantSessionCache : IDashboardAssistantSessionCache
{
    private readonly ConcurrentDictionary<string, DashboardAssistantSessionState> _sessions = new();

    public DashboardAssistantSessionState GetOrCreate(string sessionId, int userId)
    {
        string normalizedSessionId = string.IsNullOrWhiteSpace(sessionId)
            ? "default"
            : sessionId.Trim();

        return _sessions.GetOrAdd(
            $"{userId}:{normalizedSessionId}",
            _ => new DashboardAssistantSessionState());
    }
}

internal sealed class DashboardAssistantSessionState
{
    private readonly Dictionary<string, DashboardAssistantCachedSnapshot> _snapshots = [];

    public DateTime? LastFromDate { get; set; }
    public DateTime? LastToDate { get; set; }

    public bool TryGetSnapshot(string key, DateTimeOffset now, out object? payload)
    {
        payload = null;
        if (!_snapshots.TryGetValue(key, out DashboardAssistantCachedSnapshot? cached))
        {
            return false;
        }

        TimeSpan ttl = cached.IsRealtime ? TimeSpan.FromMinutes(1) : TimeSpan.FromMinutes(5);
        if (now - cached.CreatedAt > ttl)
        {
            _snapshots.Remove(key);
            return false;
        }

        payload = cached.Payload;
        return true;
    }

    public void SetSnapshot(string key, object payload, bool isRealtime, DateTimeOffset now)
    {
        _snapshots[key] = new DashboardAssistantCachedSnapshot
        {
            Payload = payload,
            IsRealtime = isRealtime,
            CreatedAt = now
        };
    }
}

internal sealed class DashboardAssistantCachedSnapshot
{
    public object Payload { get; set; } = new();
    public bool IsRealtime { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
