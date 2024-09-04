namespace Pollus.Utils;

using System.Collections.Concurrent;

public record struct Handle(int Type, int ID)
{
    public static Handle Null => new(-1, -1);

    readonly int hashCode = HashCode.Combine(Type, ID);
    public override int GetHashCode() => hashCode;
}

public record struct Handle<T>(int ID)
{
    static readonly int typeId = TypeLookup.ID<T>();
    public static Handle<T> Null => new(-1);

    public static implicit operator Handle(Handle<T> handle) => new(typeId, handle.ID);
    public static implicit operator Handle<T>(Handle handle) => new(handle.ID);

    public override int GetHashCode()
    {
        return HashCode.Combine(typeId, ID);
    }
}

public static class TypeLookup
{
    static class Type<T>
    {
        public static int ID = Interlocked.Increment(ref counter);

        static Type()
        {
            lookup.TryAdd(ID, typeof(T));
        }
    }

    static readonly ConcurrentDictionary<int, Type> lookup = new();
    static volatile int counter = 0;
    public static int ID<T>() => Type<T>.ID;

    public static Type? GetType(int typeId)
    {
        if (lookup.TryGetValue(typeId, out var type))
        {
            return type;
        }
        return null;
    }
}