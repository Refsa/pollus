namespace Pollus.Utils;

using System.Collections.Concurrent;

public static class TypeLookup
{
    static class Type<T>
    {
        public static readonly int ID = Interlocked.Increment(ref counter);
        public static readonly string Name = typeof(T).AssemblyQualifiedName ?? typeof(T).FullName ?? throw new InvalidOperationException($"Type {typeof(T)} has no assembly qualified name");

        static Type()
        {
            lookup.TryAdd(ID, typeof(T));
        }
    }

    static readonly ConcurrentDictionary<int, Type> lookup = new();
    static volatile int counter = 0;
    public static int ID<T>() => Type<T>.ID;
    public static string Name<T>() => Type<T>.Name;

    public static Type? GetType(int typeId)
    {
        if (lookup.TryGetValue(typeId, out var type))
        {
            return type;
        }

        return null;
    }
}