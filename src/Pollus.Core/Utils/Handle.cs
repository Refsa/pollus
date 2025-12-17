namespace Pollus.Utils;

using Pollus.Core.Serialization;

public record struct Handle(int Type, int ID)
{
    static Handle()
    {
        BlittableSerializerLookup.RegisterSerializer(new HandleSerializer());
    }

    public static Handle Null => new(-1, -1);

    readonly int hashCode = HashCode.Combine(Type, ID);
    public override int GetHashCode() => hashCode;
}

public record struct Handle<T>(int ID)
{
    static Handle()
    {
        BlittableSerializerLookup.RegisterSerializer(new HandleSerializer<T>());
    }

    static readonly int typeId = TypeLookup.ID<T>();
    public static Handle<T> Null => new(-1);

    public static implicit operator Handle(Handle<T> handle) => new(typeId, handle.ID);
    public static implicit operator Handle<T>(Handle handle) => new(handle.ID);

    public override int GetHashCode()
    {
        return HashCode.Combine(typeId, ID);
    }
}

public static class HandleExtensions
{
    public static bool IsNull<T>(this Handle<T> handle) => handle == Handle<T>.Null;
    public static bool IsNull(this Handle handle) => handle == Handle.Null;
}

public class HandleSerializer<T> : IBlittableSerializer<Handle<T>>
{
    public Handle<T> Deserialize<TReader>(ref TReader reader) where TReader : IReader
    {
        var c = new Handle();
        return c;
    }

    public void Serialize<TWriter>(ref TWriter writer, ref Handle<T> value) where TWriter : IWriter
    {
    }
}

public class HandleSerializer : IBlittableSerializer<Handle>
{
    public Handle Deserialize<TReader>(ref TReader reader) where TReader : IReader
    {
        var c = new Handle();
        return c;
    }

    public void Serialize<TWriter>(ref TWriter writer, ref Handle value) where TWriter : IWriter
    {
        
    }
}