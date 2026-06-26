namespace CyberSecurityAwarenessBot.UI;

public static class ConsoleUI
{
    public static void DisplayHeader()
    {
        Console.Clear();
        DrawBorder();

        SetColor(ConsoleColor.Magenta);
        Console.WriteLine("            CYBERSECURITY AWARENESS BOT");
        Reset();

        DrawBorder();

        SetColor(ConsoleColor.Blue);
        Console.WriteLine(@"
    ____  _                 _       _____                 
   / ___|| |__   ___   ___ | | __  | ____|_ __ ___  _ __  
   \___ \| '_ \ / _ \ / _ \| |/ /  |  _| | '_ ` _ \| '_ \ 
    ___) | | | | (_) | (_) |   <   | |___| | | | | | |_) |
   |____/|_| |_|\___/ \___/|_|\_\  |_____|_| |_| |_| .__/ 
                                                  |_|     

        [ PROTECT • DETECT • PREVENT ]
");
        Reset();

        SetColor(ConsoleColor.Yellow);
        Console.WriteLine("Stay alert. Stay secure. Stay informed.");
        Console.WriteLine();
        Reset();
    }

    public static void WriteBotMessage(string message)
    {
        SetColor(ConsoleColor.Cyan);
        Console.Write("Bot: ");
        Reset();
        TypeText(message);
        Console.WriteLine();
    }

    public static void WriteUserPrompt(string message)
    {
        SetColor(ConsoleColor.White);
        Console.Write(message);
        Reset();
    }

    public static void WriteError(string message)
    {
        SetColor(ConsoleColor.Red);
        Console.WriteLine("Error: " + message);
        Reset();
    }

    public static void WriteSuccess(string message)
    {
        SetColor(ConsoleColor.Green);
        Console.WriteLine("✔ " + message);
        Reset();
    }

    private static void DrawBorder()
    {
        SetColor(ConsoleColor.Cyan);
        Console.WriteLine("==============================================================");
        Reset();
    }

    private static void SetColor(ConsoleColor color) => Console.ForegroundColor = color;
    private static void Reset() => Console.ResetColor();

    private static void TypeText(string text, int delay = 15)
    {
        foreach (char c in text)
        {
            Console.Write(c);
            Thread.Sleep(delay);
        }
    }
}
