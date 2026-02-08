namespace Pollus.Debugging;

using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

public static class Guard
{
    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IsTrue(bool condition, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        if (condition) return;
        var fmessage = message;
        Log.Error($"{fmessage}\n\tMember: {memberName}\n\tFile: {filePath}:{lineNumber}");
        throw new GuardException(fmessage, new StackTrace(new StackTrace(1, true).GetFrames().TakeWhile(e => e.HasSource())).ToString());
    }

    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IsTrue(bool condition, FormattableString message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        if (condition) return;
        IsTrue(condition, message.ToString(), filePath, lineNumber, memberName);
    }

    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IsFalse(bool condition, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        if (!condition) return;
        var fmessage = message;
        Log.Error($"{fmessage}\n\tMember: {memberName}\n\tFile: {filePath}:{lineNumber}");
        throw new GuardException(fmessage, new StackTrace(new StackTrace(1, true).GetFrames().TakeWhile(e => e.HasSource())).ToString());
    }

    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IsFalse(bool condition, FormattableString message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        if (condition) return;
        IsFalse(condition, message.ToString(), filePath, lineNumber, memberName);
    }

    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IsNotNull<T>([NotNull] T? obj, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        if (obj is not null) return;
        Log.Error($"{message}\n\tMember: {memberName}\n\tFile: {filePath}:{lineNumber}");
        throw new GuardException(message, new StackTrace(new StackTrace(1, true).GetFrames().TakeWhile(e => e.HasSource())).ToString());
    }

    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IsNotNull<T>([NotNull] T? obj, FormattableString message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        if (obj is not null) return;
        IsNotNull(obj, message.ToString(), filePath, lineNumber, memberName);
    }

    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IsNull<T>(T? obj, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        if (obj is null) return;
        Log.Error($"{message}\n\tMember: {memberName}\n\tFile: {filePath}:{lineNumber}");
        throw new GuardException(message, new StackTrace(new StackTrace(1, true).GetFrames().TakeWhile(e => e.HasSource())).ToString());
    }

    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IsNull<T>(T? obj, FormattableString message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "")
    {
        if (obj is null) return;
        IsNull(obj, message.ToString(), filePath, lineNumber, memberName);
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