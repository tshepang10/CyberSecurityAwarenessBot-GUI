using CyberSecurityAwarenessBot.Models;
using CyberSecurityAwarenessBot.Services;
using CyberSecurityAwarenessBot.UI;

namespace CyberSecurityAwarenessBot;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Contains("--console", StringComparer.OrdinalIgnoreCase))
        {
            RunConsoleMode();
            return;
        }

        RunGuiMode();
    }

    private static void RunGuiMode()
    {
        ApplicationConfiguration.Initialize();

        using var welcomeForm = new WelcomeForm();
        if (welcomeForm.ShowDialog() != DialogResult.OK || welcomeForm.User is null)
            return;

        Application.Run(new MainForm(welcomeForm.User));
    }

    private static void RunConsoleMode()
    {
        Console.Title = "Cybersecurity Awareness Bot";
        ConsoleUI.DisplayHeader();
        AudioPlayer.PlayGreeting("Assets/greetings.wav");

        var user = new UserProfile();
        ConsoleUI.WriteBotMessage("Hello! Welcome to the Cybersecurity Awareness Bot.");
        ConsoleUI.WriteBotMessage("I am here to help you stay safe online.");
        ConsoleUI.WriteBotMessage("What is your name?");

        string name = ReadValidName();
        user.Name = name;
        ConsoleUI.WriteSuccess($"Nice to meet you, {user.Name}!");

        ConsoleUI.WriteBotMessage("You can ask me about passwords, phishing, privacy, scams, or safe browsing.");
        ConsoleUI.WriteBotMessage("Type 'exit' to close the chatbot.");

        var memory = new MemoryService();
        var sentiment = new SentimentService();
        var responseService = new ResponseService(memory, sentiment);
        var chatbot = new ChatbotService(responseService);
        chatbot.StartChat(user);
    }

    private static string ReadValidName()
    {
        string? input = Console.ReadLine();
        while (string.IsNullOrWhiteSpace(input))
        {
            ConsoleUI.WriteError("Name cannot be empty. Please enter your name:");
            input = Console.ReadLine();
        }

        return input.Trim();
    }
}
