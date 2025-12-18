namespace Pollus.Core.Serialization;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public interface IBlittableSerializer<TContext>
    where TContext : allows ref struct
{
    byte[] DeserializeBytes<TReader>(ref TReader reader, in TContext context) where TReader : IReader, allows ref struct;
}

public interface IBlittableSerializer<TData, TContext> : IBlittableSerializer<TContext>, ISerializer<TData, TContext>
    where TData : unmanaged
    where TContext : allows ref struct
{
    public new TData Deserialize<TReader>(ref TReader reader, in TContext context) where TReader : IReader, allows ref struct;
    public new void Serialize<TWriter>(ref TWriter reader, in TData value, in TContext context) where TWriter : IWriter, allows ref struct;

    byte[] IBlittableSerializer<TContext>.DeserializeBytes<TReader>(ref TReader reader, in TContext context)
    {
        var value = Deserialize(ref reader, in context);
        var bytes = new byte[Unsafe.SizeOf<TData>()];
        MemoryMarshal.Write(bytes, in value);
        return bytes;
    }
}

public static class BlittableSerializerLookup<TContext>
    where TContext : allows ref struct
{
    static readonly ConcurrentDictionary<Type, IBlittableSerializer<TContext>> serializers = new();

    public static void RegisterSerializer<T>(IBlittableSerializer<T, TContext> serializer)
        where T : unmanaged
    {
        SerializerLookup<TContext>.RegisterSerializer(serializer);
        serializers.TryAdd(typeof(T), serializer);
    }

    public static IBlittableSerializer<TData, TContext>? GetSerializer<TData>()
        where TData : unmanaged
    {
        if (serializers.TryGetValue(typeof(TData), out var serializer))
        {
            return (IBlittableSerializer<TData, TContext>)serializer;
        }

        return null;
    }

    public static IBlittableSerializer<TContext>? GetSerializer(Type type)
    {
        return serializers.GetValueOrDefault(type);
    }
}