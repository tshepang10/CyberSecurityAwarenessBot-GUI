using System.Media;

namespace CyberSecurityAwarenessBot.Services;

public static class AudioPlayer
{
    public static void PlayGreeting(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("[Audio file not found. Continuing without voice greeting.]");
                return;
            }

            using var player = new SoundPlayer(path);
            player.Play();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Audio error: {ex.Message}]");
        }
    }
}
