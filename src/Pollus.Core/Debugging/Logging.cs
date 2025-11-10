namespace Pollus.Debugging;

public class Log
{
    public static void Info(string message)
    {
        using var color = new ColorScope(ConsoleColor.Cyan);
        Console.WriteLine($"[INFO] {message}");
    }

    public static void Warn(string message)
    {
        using var color = new ColorScope(ConsoleColor.Yellow);
        Console.WriteLine($"[WARN] {message}");
    }

    public static void Error(string message)
    {
        using var color = new ColorScope(ConsoleColor.Red);
        Console.WriteLine($"[ERROR] {message}");
    }

    public static void Error(Exception exception, string message)
    {
        using var color = new ColorScope(ConsoleColor.DarkMagenta);
        Console.WriteLine($"[EXCEPTION] {message}\n{exception}");
    }

    ref struct ColorScope
    {
        readonly bool enabled;
        readonly ConsoleColor previousColor;

        public ColorScope(ConsoleColor color)
        {
            if (OperatingSystem.IsBrowser())
            {
                enabled = false;
                previousColor = default;
            }
            else
            {
                enabled = true;
                previousColor = Console.ForegroundColor;
                Console.ForegroundColor = color;
            }
        }

        public void Dispose()
        {
            if (enabled) Console.ForegroundColor = previousColor;
        }
    }
}