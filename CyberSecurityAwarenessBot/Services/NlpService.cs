using System.Text.RegularExpressions;
using CyberSecurityAwarenessBot.Models;

namespace CyberSecurityAwarenessBot.Services;

public sealed class NlpService
{
    public UserIntent DetectIntent(string input)
    {
        string text = input.ToLowerInvariant().Trim();

        if (IsExit(text))
            return UserIntent.Exit;

        if (IsActivityLogRequest(text))
            return UserIntent.ShowActivityLog;

        if (IsQuizRequest(text))
            return UserIntent.StartQuiz;

        if (IsViewTasksRequest(text))
            return UserIntent.ViewTasks;

        if (IsCompleteTaskRequest(text))
            return UserIntent.CompleteTask;

        if (IsDeleteTaskRequest(text))
            return UserIntent.DeleteTask;

        if (IsAddTaskRequest(text))
            return UserIntent.AddTask;

        if (IsReminderRequest(text))
            return UserIntent.SetReminder;

        return UserIntent.Chat;
    }

    public string? ExtractTaskTitle(string input)
    {
        string text = input.Trim();

        Match match = Regex.Match(
            text,
            @"(?:add task|create task|new task|add a task)\s*[-:]\s*(.+)$",
            RegexOptions.IgnoreCase);

        if (match.Success)
            return match.Groups[1].Value.Trim();

        match = Regex.Match(
            text,
            @"(?:add a task to|add task to|set a reminder to|remind me to)\s+(.+?)(?:\s+(?:in|on|tomorrow|today)\b.*)?$",
            RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    public int? ExtractDaysFromNow(string input)
    {
        Match match = Regex.Match(input, @"in\s+(\d+)\s+days?", RegexOptions.IgnoreCase);
        if (match.Success && int.TryParse(match.Groups[1].Value, out int days))
            return days;

        if (input.Contains("tomorrow", StringComparison.OrdinalIgnoreCase))
            return 1;

        if (input.Contains("today", StringComparison.OrdinalIgnoreCase))
            return 0;

        return null;
    }

    public int? ExtractTaskId(string input)
    {
        Match match = Regex.Match(input, @"(?:task|#)\s*(\d+)", RegexOptions.IgnoreCase);
        return match.Success && int.TryParse(match.Groups[1].Value, out int id) ? id : null;
    }

    private static bool IsExit(string text) =>
        text is "exit" or "quit" or "bye" or "goodbye";

    private static bool IsActivityLogRequest(string text) =>
        ContainsAny(text, "activity log", "what have you done", "show log", "recent actions");

    private static bool IsQuizRequest(string text) =>
        ContainsAny(text, "start quiz", "play quiz", "quiz game", "take a quiz", "test me");

    private static bool IsViewTasksRequest(string text) =>
        ContainsAny(text, "view tasks", "show tasks", "list tasks", "my tasks");

    private static bool IsCompleteTaskRequest(string text) =>
        ContainsAny(text, "complete task", "mark task", "task done", "finished task");

    private static bool IsDeleteTaskRequest(string text) =>
        ContainsAny(text, "delete task", "remove task");

    private static bool IsAddTaskRequest(string text) =>
        ContainsAny(text, "add task", "create task", "new task", "add a task");

    private static bool IsReminderRequest(string text) =>
        ContainsAny(text, "remind me", "set a reminder", "reminder for");

    private static bool ContainsAny(string text, params string[] keywords) =>
        keywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
}
