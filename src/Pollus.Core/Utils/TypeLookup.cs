namespace Pollus.Utils;

using System.Collections.Concurrent;
using System.Diagnostics;

[DebuggerDisplay("{TypeName}")]
public record struct TypeID(int ID)
{
    public static implicit operator int(TypeID typeId) => typeId.ID;
    public static implicit operator TypeID(int id) => new(id);

    public string TypeName => TypeLookup.GetInfo(ID)?.AssemblyQualifiedName ?? "<unknown>";

    public override int GetHashCode() => ID;
}

public static class TypeLookup
{
    public class Info
    {
        public required TypeID ID { get; init; }
        public required Type Type { get; init; }
        public required string Name { get; init; }
        public required string AssemblyQualifiedName { get; init; }
    }

    static class Type<T>
    {
        public static readonly TypeID ID = Interlocked.Increment(ref counter);
        public static readonly Info Info;
        public static readonly string Name = typeof(T).AssemblyQualifiedName ?? typeof(T).FullName ?? throw new InvalidOperationException($"Type {typeof(T)} has no assembly qualified name");

        static Type()
        {
            Info = new Info
            {
                ID = ID,
                Type = typeof(T),
                Name = Name,
                AssemblyQualifiedName = typeof(T).AssemblyQualifiedName ?? throw new InvalidOperationException($"Type {typeof(T)} has no assembly qualified name")
            };
            lookup.TryAdd(ID, Info);
            reverseLookup.TryAdd(typeof(T), ID);
        }
    }

    static readonly ConcurrentDictionary<TypeID, Info> lookup = new();
    static readonly ConcurrentDictionary<Type, TypeID> reverseLookup = new();
    static volatile int counter;

    public static int ID<T>() => Type<T>.ID;
    public static string Name<T>() => Type<T>.Name;

    public static TypeID? ID(Type type)
    {
        return reverseLookup.TryGetValue(type, out var id) ? id : null;
    }

    public static Info? GetInfo(TypeID typeId)
    {
        return lookup.GetValueOrDefault(typeId);
    }

    public static Type? GetType(TypeID typeId)
    {
        return lookup.GetValueOrDefault(typeId)?.Type;
    }
}