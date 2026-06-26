using System.Text.Json;
using CyberSecurityAwarenessBot.Models;

namespace CyberSecurityAwarenessBot.Services;

public sealed class ActivityLogService
{
    private readonly List<ActivityLogEntry> _entries = [];

    public void Log(string description)
    {
        _entries.Add(new ActivityLogEntry { Description = description });
    }

    public IReadOnlyList<ActivityLogEntry> GetRecent(int count = 8)
    {
        return _entries
            .OrderByDescending(e => e.Timestamp)
            .Take(count)
            .Reverse()
            .ToList();
    }

    public IReadOnlyList<ActivityLogEntry> GetAll()
    {
        return _entries.OrderByDescending(e => e.Timestamp).ToList();
    }

    public string FormatSummary(bool showAll = false, int recentCount = 8)
    {
        var entries = showAll ? GetAll() : GetRecent(recentCount);
        if (entries.Count == 0)
            return "No actions recorded yet.";

        var lines = entries.Select((entry, index) =>
            $"{index + 1}. [{entry.Timestamp:yyyy-MM-dd HH:mm}] {entry.Description}");

        return string.Join(Environment.NewLine, lines);
    }
}

public sealed class MemoryService
{
    public string? FavoriteTopic { get; private set; }

    public bool TryRememberTopic(string input)
    {
        string normalized = input.ToLowerInvariant();
        string[] topics = ["privacy", "password", "phishing", "scam", "safe browsing", "2fa", "malware"];

        foreach (string topic in topics)
        {
            if (!normalized.Contains("interested in") && !normalized.Contains("i like") && !normalized.Contains("i love"))
                continue;

            if (!normalized.Contains(topic))
                continue;

            FavoriteTopic = topic;
            return true;
        }

        return false;
    }

    public string? GetPersonalizedPrefix()
    {
        return FavoriteTopic is null
            ? null
            : $"As someone interested in {FavoriteTopic}, ";
    }
}

public sealed class SentimentService
{
    public Sentiment Detect(string input)
    {
        string text = input.ToLowerInvariant();

        if (ContainsAny(text, "worried", "scared", "afraid", "anxious", "nervous"))
            return Sentiment.Worried;

        if (ContainsAny(text, "frustrated", "annoyed", "confused", "overwhelmed", "stuck"))
            return Sentiment.Frustrated;

        if (ContainsAny(text, "curious", "interested", "wonder", "learn", "tell me"))
            return Sentiment.Curious;

        return Sentiment.Neutral;
    }

    public string GetEmpatheticPrefix(Sentiment sentiment)
    {
        return sentiment switch
        {
            Sentiment.Worried =>
                "It is completely understandable to feel that way. Cyber threats can feel overwhelming, but you are taking the right step by learning. ",
            Sentiment.Frustrated =>
                "I hear you — cybersecurity can feel complicated at first. Let me break this down in a simple way. ",
            Sentiment.Curious =>
                "Great question! I love your curiosity about staying safe online. ",
            _ => string.Empty
        };
    }

    private static bool ContainsAny(string text, params string[] keywords)
    {
        return keywords.Any(text.Contains);
    }
}
