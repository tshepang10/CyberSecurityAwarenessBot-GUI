namespace CyberSecurityAwarenessBot.Models;

public enum Sentiment
{
    Neutral,
    Worried,
    Curious,
    Frustrated
}

public enum ConversationTopic
{
    None,
    Password,
    Phishing,
    Privacy,
    Scam,
    SafeBrowsing
}

public enum UserIntent
{
    Chat,
    AddTask,
    ViewTasks,
    CompleteTask,
    DeleteTask,
    StartQuiz,
    ShowActivityLog,
    SetReminder,
    Exit
}
