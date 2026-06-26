using CyberSecurityAwarenessBot.Models;

namespace CyberSecurityAwarenessBot.Services;

public sealed class ResponseService
{
    private readonly Random _random = new();
    private readonly MemoryService _memory;
    private readonly SentimentService _sentiment;
    private ConversationTopic _currentTopic = ConversationTopic.None;

    private readonly Dictionary<ConversationTopic, List<string>> _topicResponses = new()
    {
        [ConversationTopic.Phishing] =
        [
            "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
            "Check the sender's email address carefully. Official companies rarely use free email domains.",
            "Never click links in unexpected emails. Visit the official website directly instead.",
            "Look for spelling mistakes and generic greetings like 'Dear Customer' — common phishing signs."
        ],
        [ConversationTopic.Password] =
        [
            "Use strong, unique passwords for each account. Avoid using personal details in your passwords.",
            "Consider a reputable password manager to generate and store complex passwords securely.",
            "Enable two-factor authentication wherever possible for an extra layer of protection.",
            "Never share your password via email, SMS, or phone — legitimate services will never ask for it."
        ],
        [ConversationTopic.Privacy] =
        [
            "Review privacy settings on social media and limit what strangers can see about you.",
            "Be careful about sharing personal information online — scammers can use it against you.",
            "Regularly audit app permissions on your phone and revoke access you no longer need.",
            "Use privacy-focused browser settings and clear cookies from untrusted sites."
        ],
        [ConversationTopic.Scam] =
        [
            "Online scams often create urgency or fear. Be cautious of messages asking for money, OTPs, or banking details.",
            "If an offer sounds too good to be true, it probably is. Verify before you act.",
            "Never share one-time PINs or verification codes with anyone — not even 'bank officials'.",
            "Report suspected scams to your bank and the SAPS cybercrime unit."
        ],
        [ConversationTopic.SafeBrowsing] =
        [
            "Only visit trusted websites, check for HTTPS, and avoid clicking unknown pop-ups.",
            "Keep your browser and extensions updated to patch known security vulnerabilities.",
            "Use an ad blocker and avoid downloading software from unofficial sources.",
            "Bookmark important sites instead of searching for them — typosquatting is a real threat."
        ]
    };

    public ResponseService(MemoryService memory, SentimentService sentiment)
    {
        _memory = memory;
        _sentiment = sentiment;
    }

    public string GetResponse(string userInput, UserProfile user)
    {
        string input = userInput.ToLowerInvariant().Trim();

        if (string.IsNullOrWhiteSpace(input))
            return "You entered nothing. Please type a question so I can help you.";

        Sentiment mood = _sentiment.Detect(input);
        string empathy = _sentiment.GetEmpatheticPrefix(mood);

        if (_memory.TryRememberTopic(input))
        {
            _currentTopic = MapTopic(_memory.FavoriteTopic!);
            return $"{empathy}Great! I'll remember that you're interested in {_memory.FavoriteTopic}. It's a crucial part of staying safe online.";
        }

        if (IsFollowUpRequest(input))
            return empathy + GetFollowUpResponse(user.Name);

        if (Contains(input, "how are you"))
            return $"{empathy}I am doing well, {user.Name}. Thank you for asking. I am ready to help you with cybersecurity awareness.";

        if (Contains(input, "what's your purpose", "what is your purpose"))
            return $"{empathy}My purpose is to educate South African citizens about cybersecurity risks and help them stay safe online.";

        if (Contains(input, "what can i ask you about", "help", "what do you know"))
            return GetHelpResponse();

        if (Contains(input, "password"))
        {
            _currentTopic = ConversationTopic.Password;
            return empathy + PrefixPersonalized() + GetRandomResponse(ConversationTopic.Password);
        }

        if (Contains(input, "phishing", "phishing tip"))
        {
            _currentTopic = ConversationTopic.Phishing;
            return empathy + PrefixPersonalized() + GetRandomResponse(ConversationTopic.Phishing);
        }

        if (Contains(input, "privacy"))
        {
            _currentTopic = ConversationTopic.Privacy;
            return empathy + PrefixPersonalized() + GetRandomResponse(ConversationTopic.Privacy);
        }

        if (Contains(input, "scam"))
        {
            _currentTopic = ConversationTopic.Scam;
            return empathy + PrefixPersonalized() + GetRandomResponse(ConversationTopic.Scam);
        }

        if (Contains(input, "safe browsing", "browse safely", "browsing"))
        {
            _currentTopic = ConversationTopic.SafeBrowsing;
            return empathy + PrefixPersonalized() + GetRandomResponse(ConversationTopic.SafeBrowsing);
        }

        if (Contains(input, "suspicious link", "link"))
            return empathy + "Before clicking a link, hover over it to inspect the URL. If it looks strange or unrelated to the sender, do not click it.";

        if (Contains(input, "2fa", "two-factor", "two factor"))
            return empathy + "Two-factor authentication adds a second verification step. Even if someone steals your password, they still cannot access your account.";

        if (IsExit(input))
            return $"Goodbye, {user.Name}. Stay safe online!";

        return "I didn't quite understand that. Could you rephrase? You can ask about passwords, phishing, privacy, scams, tasks, or say 'start quiz'.";
    }

    private string GetFollowUpResponse(string userName)
    {
        if (_currentTopic == ConversationTopic.None)
            return $"Sure, {userName}. Ask me about passwords, phishing, privacy, scams, or safe browsing and I'll share more tips.";

        return PrefixPersonalized() + GetRandomResponse(_currentTopic);
    }

    private string GetRandomResponse(ConversationTopic topic)
    {
        var responses = _topicResponses[topic];
        return responses[_random.Next(responses.Count)];
    }

    private string PrefixPersonalized()
    {
        return _memory.GetPersonalizedPrefix() ?? string.Empty;
    }

    private string GetHelpResponse()
    {
        return """
            You can ask me about:
            - Password safety, phishing, privacy, scams, and safe browsing
            - Tasks: "Add task - Enable two-factor authentication"
            - Reminders: "Remind me to update my password in 3 days"
            - Quiz: "Start quiz"
            - Activity log: "What have you done for me?"
            Say "tell me more" to continue the current topic.
            """;
    }

    private static ConversationTopic MapTopic(string topic)
    {
        if (topic.Contains("password")) return ConversationTopic.Password;
        if (topic.Contains("phishing")) return ConversationTopic.Phishing;
        if (topic.Contains("privacy")) return ConversationTopic.Privacy;
        if (topic.Contains("scam")) return ConversationTopic.Scam;
        return ConversationTopic.SafeBrowsing;
    }

    private static bool IsFollowUpRequest(string input) =>
        Contains(input, "tell me more", "explain more", "give me another tip", "another tip", "more info", "go on");

    private static bool IsExit(string input) =>
        input is "exit" or "quit" or "bye" or "goodbye";

    private static bool Contains(string input, params string[] keywords) =>
        keywords.Any(keyword => input.Contains(keyword, StringComparison.OrdinalIgnoreCase));
}
