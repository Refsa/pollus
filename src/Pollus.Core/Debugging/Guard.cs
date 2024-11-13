namespace Pollus.Debugging;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

public static class Guard
{
    public static void IsTrue(bool condition, string message)
    {
        if (!condition)
        {
            var fmessage = message.ToString();
            Log.Error(fmessage);
            throw new GuardException(fmessage, new StackTrace(new StackTrace(1, true).GetFrames().TakeWhile(e => e.HasSource())).ToString());
        }
    }

    public static void IsFalse(bool condition, string message)
    {
        if (condition)
        {
            var fmessage = message.ToString();
            Log.Error(fmessage);
            throw new GuardException(fmessage, new StackTrace(new StackTrace(1, true).GetFrames().TakeWhile(e => e.HasSource())).ToString());
        }
    }

    public static void IsNotNull<T>([NotNull] T? obj, string message)
    {
        if (obj is null)
        {
            var fmessage = message.ToString();
            Log.Error(fmessage);
            throw new GuardException(fmessage, new StackTrace(new StackTrace(1, true).GetFrames().TakeWhile(e => e.HasSource())).ToString());
        }
    }

    public static void IsNull<T>(T? obj, string message)
    {
        if (obj is not null)
        {
            var fmessage = message.ToString();
            Log.Error(fmessage);
            throw new GuardException(fmessage, new StackTrace(new StackTrace(1, true).GetFrames().TakeWhile(e => e.HasSource())).ToString());
        }
    }
}

public sealed class GuardException : Exception
{
    readonly string stackTrace;
    public GuardException(string message, string stackTrace) : base(message)
    {
        this.stackTrace = stackTrace;
    }

    public override string StackTrace => stackTrace;
}