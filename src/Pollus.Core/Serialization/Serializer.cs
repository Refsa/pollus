namespace Pollus.Core.Serialization;

using System.Collections.Concurrent;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class SerializeAttribute() : Attribute;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class SerializeIgnoreAttribute : Attribute;

public interface IWriter
{
    public ReadOnlySpan<byte> Buffer { get; }

    void Clear();
    void Write(ReadOnlySpan<byte> data);
    void Write<T>(ReadOnlySpan<T> data) where T : unmanaged;
    void Write<T>(T value) where T : unmanaged;
    void Write<T>(T[] values) where T : unmanaged;
    void Write(string value);
}

public interface IReader
{
    void Init(byte[]? data);
    string ReadString();
    T Read<T>() where T : unmanaged;
    T[] ReadArray<T>() where T : unmanaged;
}

public struct DefaultSerializationContext;

public interface ISerializable<TContext>
{
    void Serialize<TWriter>(ref TWriter writer, in TContext context) where TWriter : IWriter;
    void Deserialize<TReader>(ref TReader reader, in TContext context) where TReader : IReader;
}

public interface ISerializer<TContext>
{
    public object? DeserializeBoxed<TReader>(ref TReader reader, in TContext context) where TReader : IReader;
}

public interface ISerializer<TData, TContext> : ISerializer<TContext>
{
    public TData Deserialize<TReader>(ref TReader reader, in TContext context) where TReader : IReader;
    public void Serialize<TWriter>(ref TWriter reader, ref TData value, in TContext context) where TWriter : IWriter;

    object? ISerializer<TContext>.DeserializeBoxed<TReader>(ref TReader reader, in TContext context)
    {
        return Deserialize(ref reader, in context);
    }
}

public static class SerializerLookup<TContext>
{
    static readonly ConcurrentDictionary<Type, ISerializer<TContext>> serializers = new();

    public static void RegisterSerializer<TData>(ISerializer<TData, TContext> serializer)
        where TData : unmanaged
    {
        serializers.TryAdd(typeof(TData), serializer);
    }

    public static ISerializer<TData, TContext>? GetSerializer<TData>()
        where TData : unmanaged
    {
        if (serializers.TryGetValue(typeof(TData), out var serializer))
        {
            return (ISerializer<TData, TContext>)serializer;
        }

        return null;
    }

    public static ISerializer<TContext>? GetSerializer(Type type)
    {
        return serializers.GetValueOrDefault(type);
    }
}