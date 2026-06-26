namespace CyberSecurityAwarenessBot.Models;

public enum QuestionType
{
    MultipleChoice,
    TrueFalse
}

public class QuizQuestion
{
    public string Question { get; set; } = string.Empty;
    public string[] Options { get; set; } = [];
    public int CorrectIndex { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public QuestionType Type { get; set; } = QuestionType.MultipleChoice;
}
