namespace Pollus.Logging;

public static class Assert
{

    public static void IsTrue(bool condition, string message)
    {
        if (!condition)
        {
            Log.Error(message);
        }
    }

    public static void IsFalse(bool condition, string message)
    {
        if (condition)
        {
            Log.Error(message);
        }
    }

    public static void IsNotNull<T>(T? obj, string message)
    {
        if (obj is null)
        {
            Log.Error(message);
        }
    }

    public static void IsNull<T>(T? obj, string message)
    {
        if (obj is not null)
        {
            Log.Error(message);
        }
    }
}