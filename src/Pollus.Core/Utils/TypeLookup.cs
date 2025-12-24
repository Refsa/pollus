namespace Pollus.Utils;

using System.Collections.Concurrent;

public record struct TypeID(int ID)
{
    public static implicit operator int(TypeID typeId) => typeId.ID;
    public static implicit operator TypeID(int id) => new(id);

    public override int GetHashCode() => ID;
}

public static class TypeLookup
{
    static class Type<T>
    {
        public static readonly TypeID ID = Interlocked.Increment(ref counter);
        public static readonly string Name = typeof(T).AssemblyQualifiedName ?? typeof(T).FullName ?? throw new InvalidOperationException($"Type {typeof(T)} has no assembly qualified name");

        static Type()
        {
            lookup.TryAdd(ID, typeof(T));
        }
    }

    static readonly ConcurrentDictionary<TypeID, Type> lookup = new();
    static volatile int counter = 0;
    public static int ID<T>() => Type<T>.ID;
    public static string Name<T>() => Type<T>.Name;

    public static Type? GetType(TypeID typeId)
    {
        return lookup.GetValueOrDefault(typeId);
    }
}