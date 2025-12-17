namespace Pollus.Core.Serialization;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public interface IBlittableSerializer
{
    byte[] DeserializeBytes<TReader>(ref TReader reader) where TReader : IReader;
}

public interface IBlittableSerializer<T> : IBlittableSerializer, ISerializer<T>
    where T : unmanaged
{
    public new T Deserialize<TReader>(ref TReader reader) where TReader : IReader;
    public new void Serialize<TWriter>(ref TWriter reader, ref T value) where TWriter : IWriter;

    byte[] IBlittableSerializer.DeserializeBytes<TReader>(ref TReader reader)
    {
        var value = Deserialize(ref reader);
        var bytes = new byte[Unsafe.SizeOf<T>()];
        MemoryMarshal.Write(bytes, in value);
        return bytes;
    }
}

public static class BlittableSerializerLookup
{
    static readonly ConcurrentDictionary<Type, IBlittableSerializer> serializers = new();

    public static void RegisterSerializer<T>(IBlittableSerializer<T> serializer)
        where T : unmanaged
    {
        SerializerLookup.RegisterSerializer(serializer);
        serializers.TryAdd(typeof(T), serializer);
    }

    public static IBlittableSerializer<T>? GetSerializer<T>()
        where T : unmanaged
    {
        if (serializers.TryGetValue(typeof(T), out var serializer))
        {
            return (IBlittableSerializer<T>)serializer;
        }

        return null;
    }

    public static IBlittableSerializer? GetSerializer(Type type)
    {
        return serializers.GetValueOrDefault(type);
    }
}