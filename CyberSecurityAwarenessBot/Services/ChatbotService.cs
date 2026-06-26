using CyberSecurityAwarenessBot.Models;
using CyberSecurityAwarenessBot.UI;

namespace CyberSecurityAwarenessBot.Services;

public sealed class ChatbotService
{
    private readonly ResponseService _responseService;

    public ChatbotService(ResponseService responseService)
    {
        _responseService = responseService;
    }

    public void StartChat(UserProfile user)
    {
        bool isRunning = true;

        while (isRunning)
        {
            ConsoleUI.WriteUserPrompt($"{user.Name}: ");
            string input = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                ConsoleUI.WriteError("Invalid input. Please type a valid question.");
                continue;
            }

            string response = _responseService.GetResponse(input, user);
            ConsoleUI.WriteBotMessage(response);

            if (input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                isRunning = false;
        }
    }
}

public sealed class ChatOrchestrator
{
    private readonly ResponseService _responseService;
    private readonly NlpService _nlpService;
    private readonly TaskService _taskService;
    private readonly ActivityLogService _activityLog;
    private readonly UserProfile _user;

    public ChatOrchestrator(
        UserProfile user,
        ResponseService responseService,
        NlpService nlpService,
        TaskService taskService,
        ActivityLogService activityLog)
    {
        _user = user;
        _responseService = responseService;
        _nlpService = nlpService;
        _taskService = taskService;
        _activityLog = activityLog;
    }

    public event Action<UserIntent>? IntentDetected;

    public bool HandleInput(string input, out string response)
    {
        UserIntent intent = _nlpService.DetectIntent(input);
        IntentDetected?.Invoke(intent);

        switch (intent)
        {
            case UserIntent.Exit:
                response = _responseService.GetResponse("exit", _user);
                return true;

            case UserIntent.StartQuiz:
                _activityLog.Log("Quiz started from chat command.");
                response = "Opening the cybersecurity quiz for you. Good luck!";
                return false;

            case UserIntent.ShowActivityLog:
                response = "Here's a summary of recent actions:\n" + _activityLog.FormatSummary();
                return false;

            case UserIntent.ViewTasks:
                response = _taskService.FormatTaskList();
                return false;

            case UserIntent.AddTask:
            case UserIntent.SetReminder:
                response = HandleTaskOrReminder(input);
                return false;

            case UserIntent.CompleteTask:
                response = HandleCompleteTask(input);
                return false;

            case UserIntent.DeleteTask:
                response = HandleDeleteTask(input);
                return false;

            default:
                response = _responseService.GetResponse(input, _user);
                return false;
        }
    }

    private string HandleTaskOrReminder(string input)
    {
        string? title = _nlpService.ExtractTaskTitle(input);
        if (string.IsNullOrWhiteSpace(title))
            return "Please specify a task, for example: Add task - Review privacy settings";

        string description = $"Complete the cybersecurity task: {title}";
        int? days = _nlpService.ExtractDaysFromNow(input);
        DateTime? reminder = days.HasValue ? DateTime.Today.AddDays(days.Value) : null;

        var task = _taskService.Add(title, description, reminder);

        if (reminder.HasValue)
        {
            _activityLog.Log($"Reminder set for '{task.Title}' on {reminder:yyyy-MM-dd}.");
            return $"Got it! I'll remind you about '{task.Title}' on {reminder:yyyy-MM-dd}.";
        }

        _activityLog.Log($"Task added: '{task.Title}'.");
        return $"Task added: '{task.Title}'. Would you like to set a reminder? Try: Remind me in 3 days.";
    }

    private string HandleCompleteTask(string input)
    {
        int? id = _nlpService.ExtractTaskId(input);
        if (!id.HasValue)
            return "Please include a task number, for example: Complete task 1";

        if (!_taskService.MarkCompleted(id.Value))
            return $"Task #{id.Value} was not found.";

        _activityLog.Log($"Task #{id.Value} marked as completed.");
        return $"Task #{id.Value} marked as completed. Well done staying on top of your security!";
    }

    private string HandleDeleteTask(string input)
    {
        int? id = _nlpService.ExtractTaskId(input);
        if (!id.HasValue)
            return "Please include a task number, for example: Delete task 2";

        if (!_taskService.Delete(id.Value))
            return $"Task #{id.Value} was not found.";

        _activityLog.Log($"Task #{id.Value} deleted.");
        return $"Task #{id.Value} has been deleted.";
    }
}
