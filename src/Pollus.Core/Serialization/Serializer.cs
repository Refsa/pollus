namespace Pollus.Core.Serialization;

using System.Collections.Concurrent;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public sealed class SerializeAttribute : Attribute;

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

public interface ISerializable
{
    void Serialize<TWriter>(ref TWriter writer) where TWriter : IWriter;
    void Deserialize<TReader>(ref TReader reader) where TReader : IReader;
}

public interface ISerializer
{
    public object? DeserializeBoxed<TReader>(ref TReader reader) where TReader : IReader;
}

public interface ISerializer<T> : ISerializer
{
    public T Deserialize<TReader>(ref TReader reader) where TReader : IReader;
    public void Serialize<TWriter>(ref TWriter reader, ref T value) where TWriter : IWriter;

    object? ISerializer.DeserializeBoxed<TReader>(ref TReader reader)
    {
        return Deserialize(ref reader);
    }
}

public static class SerializerLookup
{
    static readonly ConcurrentDictionary<Type, ISerializer> serializers = new();

    public static void RegisterSerializer<T>(ISerializer<T> serializer)
        where T : unmanaged
    {
        serializers.TryAdd(typeof(T), serializer);
    }

    public static ISerializer<T>? GetSerializer<T>()
        where T : unmanaged
    {
        if (serializers.TryGetValue(typeof(T), out var serializer))
        {
            return (ISerializer<T>)serializer;
        }

        return null;
    }

    public static ISerializer? GetSerializer(Type type)
    {
        if (serializers.TryGetValue(type, out var serializer))
        {
            return serializer;
        }

        return null;
    }
}