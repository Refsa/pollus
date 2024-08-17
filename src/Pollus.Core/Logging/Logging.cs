namespace Pollus.Logging;

public class Log
{
    static readonly Log instance = new();

    public static void Info(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }

    public static void Warn(string message)
    {
        Console.WriteLine($"[WARN] {message}");
    }

    public static void Error(string message)
    {
        Console.WriteLine($"[ERROR] {message}");
    }

    public static void Error(Exception exception, string message)
    {
        Console.WriteLine($"[ERROR] {message}\n{exception}");
    }
}