namespace Pollus.Debugging;

using System.Runtime.CompilerServices;

public class Log
{
    public enum Level
    {
        None = 0,
        Debug,
        Info,
        Warn,
        Error,
    }

    public static Level LogLevel { get; set; } = Level.Info;

    public static void Debug(string message)
    {
        if (LogLevel > Level.Debug) return;
        using var color = new ColorScope(ConsoleColor.White);
        Console.WriteLine(FormattableString.Invariant($"[{DateTime.Now:HH:mm:ss.fff}][DEBUG] {message}"));
    }

    public static void Debug(FormattableString message)
    {
        if (LogLevel > Level.Debug) return;
        Debug(message.ToString());
    }

    public static void Info(string message)
    {
        if (LogLevel > Level.Info) return;
        using var color = new ColorScope(ConsoleColor.Cyan);
        Console.WriteLine(FormattableString.Invariant($"[{DateTime.Now:HH:mm:ss.fff}][INFO] {message}"));
    }

    public static void Info(FormattableString message)
    {
        if (LogLevel > Level.Info) return;
        Info(message.ToString());
    }

    public static void Warn(string message)
    {
        if (LogLevel > Level.Warn) return;
        using var color = new ColorScope(ConsoleColor.Yellow);
        Console.WriteLine(FormattableString.Invariant($"[{DateTime.Now:HH:mm:ss.fff}][WARN] {message}"));
    }

    public static void Warn(FormattableString message)
    {
        if (LogLevel > Level.Warn) return;
        Warn(message.ToString());
    }

    public static void Error(string message)
    {
        if (LogLevel > Level.Error) return;
        using var color = new ColorScope(ConsoleColor.Red);
        Console.WriteLine(FormattableString.Invariant($"[{DateTime.Now:HH:mm:ss.fff}][ERROR] {message}"));
    }

    public static void Error(FormattableString message)
    {
        if (LogLevel > Level.Error) return;
        Error(message.ToString());
    }

    public static void Exception(Exception exception, string message)
    {
        if (LogLevel > Level.Error) return;
        using var color = new ColorScope(ConsoleColor.DarkMagenta);
        Console.WriteLine(FormattableString.Invariant($"[{DateTime.Now:HH:mm:ss.fff}][EXCEPTION] {message}\n{exception}"));
    }

    public static void Exception(Exception exception, FormattableString message)
    {
        if (LogLevel > Level.Error) return;
        Exception(exception, message.ToString());
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
            if (enabled
                && OperatingSystem.IsBrowser() is false
                && OperatingSystem.IsAndroid() is false
                && OperatingSystem.IsIOS() is false)
            {
#pragma warning disable CA1416
                Console.ForegroundColor = previousColor;
#pragma warning restore CA1416
            }
        }
    }
}