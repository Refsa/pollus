namespace Pollus.Core.Serialization;

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Pollus.Debugging;
using Pollus.Utils;

public class BinarySerialization : ISerialization
{
    ConcurrentPool<BinaryWriter> serializerPool;
    ConcurrentPool<BinaryReader> deserializerPool;

    public ISerialization.WriterWrapper Writer => new(this, serializerPool.Rent());
    public ISerialization.ReaderWrapper Reader => new(this, deserializerPool.Rent());

    public BinarySerialization()
    {
        serializerPool = new ConcurrentPool<BinaryWriter>(static () => new(), 16);
        deserializerPool = new ConcurrentPool<BinaryReader>(static () => new(), 16);
    }

    public void Return(IWriter writer)
    {
        Guard.IsNotNull(writer as BinaryWriter, "writer was null");
        if (writer is BinaryWriter binaryWriter)
        {
            serializerPool.Return(binaryWriter);
        }
    }

    public void Return(IReader reader)
    {
        Guard.IsNotNull(reader as BinaryReader, "reader was null");
        if (reader is BinaryReader binaryReader)
        {
            deserializerPool.Return(binaryReader);
        }
    }
}

public class BinaryWriter : IWriter, IDisposable
{
    byte[] buffer;
    int cursor;

    public ReadOnlySpan<byte> Buffer => buffer.AsSpan(0, cursor);

    public BinaryWriter()
    {
        buffer = ArrayPool<byte>.Shared.Rent(1024);
        cursor = 0;
    }

    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(buffer);
    }

    public void Clear()
    {
        cursor = 0;
    }

    void Resize<T>(int count)
        where T : unmanaged
    {
        var neededSize = cursor + (count * Unsafe.SizeOf<T>());
        if (neededSize < buffer.Length) return;

        var newSize = Math.Max(neededSize, buffer.Length * 2);
        var newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
        buffer.CopyTo(newBuffer, 0);
        ArrayPool<byte>.Shared.Return(buffer);
        buffer = newBuffer;
    }

    public void Write(ReadOnlySpan<byte> data, string? identifier = null)
    {
        Resize<byte>(data.Length + sizeof(int));
        Write(data.Length);
        data.CopyTo(buffer.AsSpan(cursor));
        cursor += data.Length;
    }

    public void Write<T>(ReadOnlySpan<T> data, string? identifier = null) where T : unmanaged
    {
        var sizeInBytes = data.Length * Unsafe.SizeOf<T>();
        Resize<byte>(sizeof(int) + sizeInBytes);
        Write(data.Length);
        data.CopyTo(MemoryMarshal.Cast<byte, T>(buffer.AsSpan(cursor)));
        cursor += sizeInBytes;
    }

    public void Write<T>(T value, string? identifier = null) where T : unmanaged
    {
        Resize<T>(1);
        MemoryMarshal.Write(buffer.AsSpan(cursor), in value);
        cursor += Unsafe.SizeOf<T>();
    }

    public void Write<T>(T[] values, string? identifier = null) where T : unmanaged
    {
        Write<T>(values.AsSpan());
    }

    public void Write(string value, string? identifier = null)
    {
        var byteCount = Encoding.UTF8.GetByteCount(value);
        Resize<byte>(byteCount + sizeof(int));
        Write(byteCount);
        Encoding.UTF8.GetBytes(value, buffer.AsSpan(cursor));
        cursor += byteCount;
    }

    public void Serialize<T>(in T value, string? identifier = null) where T : notnull
    {
        if (SerializerLookup<DefaultSerializationContext>.GetSerializer<T>() is { } serializer)
        {
            var self = this;
            serializer.Serialize(ref self, in value, new());
        }

        throw new InvalidOperationException($"No serializer found for type {typeof(T).Name}");
    }
}

public class BinaryReader : IReader
{
    byte[]? buffer;
    int cursor;

    public void Init(byte[]? data)
    {
        buffer = data;
        cursor = 0;
    }

    public ReadOnlySpan<T> ReadSpan<T>(string? identifier = null) where T : unmanaged
    {
        Guard.IsNotNull(buffer, "buffer was null");
        var length = Read<int>();
        var bytes = length * Unsafe.SizeOf<T>();
        var span = buffer.AsSpan(cursor, bytes);
        cursor += bytes;
        return MemoryMarshal.Cast<byte, T>(span);
    }

    public T Read<T>(string? identifier = null) where T : unmanaged
    {
        Guard.IsNotNull(buffer, "buffer was null");
        var size = Unsafe.SizeOf<T>();
        var value = MemoryMarshal.Read<T>(buffer.AsSpan(cursor));
        cursor += size;
        return value;
    }

    public T[] ReadArray<T>(string? identifier = null) where T : unmanaged
    {
        return ReadSpan<T>(identifier).ToArray();
    }

    public string? ReadString(string? identifier = null)
    {
        Guard.IsNotNull(buffer, "buffer was null");
        var length = Read<int>();
        var value = Encoding.UTF8.GetString(buffer.AsSpan(cursor, length));
        cursor += length;
        return value;
    }

    public T Deserialize<T>(string? identifier = null)
        where T : notnull
    {
        if (SerializerLookup<DefaultSerializationContext>.GetSerializer<T>() is { } serializer)
        {
            var self = this;
            return serializer.Deserialize(ref self, new());
        }

        throw new InvalidOperationException($"No serializer found for type {typeof(T).Name}");
    }
}