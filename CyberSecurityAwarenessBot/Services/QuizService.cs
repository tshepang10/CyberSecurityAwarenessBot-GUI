using CyberSecurityAwarenessBot.Models;

namespace CyberSecurityAwarenessBot.Services;

public sealed class QuizService
{
    private readonly List<QuizQuestion> _questions =
    [
        new()
        {
            Question = "What should you do if you receive an email asking for your password?",
            Options = ["Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it"],
            CorrectIndex = 2,
            Explanation = "Reporting phishing emails helps prevent scams and protects others."
        },
        new()
        {
            Question = "A strong password should include:",
            Options = ["Only lowercase letters", "Your birth date", "A mix of letters, numbers, and symbols", "Your pet's name"],
            CorrectIndex = 2,
            Explanation = "Complex passwords are harder for attackers to guess or crack."
        },
        new()
        {
            Question = "Phishing attacks often create a sense of urgency.",
            Options = ["True", "False"],
            CorrectIndex = 0,
            Type = QuestionType.TrueFalse,
            Explanation = "Scammers use urgency to pressure victims into acting without thinking."
        },
        new()
        {
            Question = "Two-factor authentication adds an extra layer of security.",
            Options = ["True", "False"],
            CorrectIndex = 0,
            Type = QuestionType.TrueFalse,
            Explanation = "2FA requires a second verification step beyond your password."
        },
        new()
        {
            Question = "Which URL is safest to enter banking details on?",
            Options = ["http://bank-login.net", "https://www.mybank.co.za", "www.secure-bank-update.com", "bit.ly/bank123"],
            CorrectIndex = 1,
            Explanation = "Look for HTTPS and a domain that matches the official institution."
        },
        new()
        {
            Question = "What is social engineering?",
            Options = [
                "Updating antivirus software",
                "Manipulating people into revealing information",
                "Encrypting files on a disk",
                "Creating strong Wi-Fi passwords"
            ],
            CorrectIndex = 1,
            Explanation = "Social engineering targets human trust rather than technical flaws."
        },
        new()
        {
            Question = "You should reuse the same password across all accounts to remember it easily.",
            Options = ["True", "False"],
            CorrectIndex = 1,
            Type = QuestionType.TrueFalse,
            Explanation = "Unique passwords limit damage if one account is compromised."
        },
        new()
        {
            Question = "What should you do before clicking a shortened link?",
            Options = [
                "Click immediately if it looks familiar",
                "Hover or inspect where it leads first",
                "Forward it to friends",
                "Disable your antivirus"
            ],
            CorrectIndex = 1,
            Explanation = "Inspecting links helps you avoid malicious destinations."
        },
        new()
        {
            Question = "Public Wi-Fi is always safe for online banking.",
            Options = ["True", "False"],
            CorrectIndex = 1,
            Type = QuestionType.TrueFalse,
            Explanation = "Public networks can be monitored; use a VPN or wait for a trusted connection."
        },
        new()
        {
            Question = "Which is a sign of a phishing SMS?",
            Options = [
                "A message from your bank with no action requested",
                "A random prize message asking you to click a link",
                "A delivery update you were expecting",
                "A calendar reminder you created"
            ],
            CorrectIndex = 1,
            Explanation = "Unexpected prize or urgency messages are common smishing tactics."
        },
        new()
        {
            Question = "Keeping software updated helps protect against known vulnerabilities.",
            Options = ["True", "False"],
            CorrectIndex = 0,
            Type = QuestionType.TrueFalse,
            Explanation = "Updates often patch security flaws attackers exploit."
        },
        new()
        {
            Question = "What should you do if you suspect your account was compromised?",
            Options = [
                "Wait and see if anything happens",
                "Change your password and enable 2FA",
                "Share your login details with support via email",
                "Post about it on social media with screenshots"
            ],
            CorrectIndex = 1,
            Explanation = "Act quickly: reset credentials and add extra protection."
        }
    ];

    public IReadOnlyList<QuizQuestion> Questions => _questions;

    public string GetClosingMessage(int score, int total)
    {
        double percentage = total == 0 ? 0 : (double)score / total * 100;

        return percentage >= 80
            ? "Great job! You are a cybersecurity pro!"
            : percentage >= 50
                ? "Good effort! Review the explanations and keep learning to stay safe online."
                : "Keep learning to stay safe online! Practice makes perfect.";
    }
}
