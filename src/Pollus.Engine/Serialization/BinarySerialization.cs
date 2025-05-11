namespace Pollus.Engine.Serialization;

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Pollus.Core.Serialization;
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

    public void Write(ReadOnlySpan<byte> data)
    {
        Resize<byte>(data.Length + sizeof(int));
        Write(data.Length);
        data.CopyTo(buffer.AsSpan(cursor));
        cursor += data.Length;
    }

    public void Write<T>(ReadOnlySpan<T> data) where T : unmanaged
    {
        Write(MemoryMarshal.Cast<T, byte>(data));
    }

    public void Write<T>(T value) where T : unmanaged
    {
        Resize<T>(1);
        MemoryMarshal.Write(buffer.AsSpan(cursor), in value);
        cursor += Unsafe.SizeOf<T>();
    }

    public void Write<T>(T[] values) where T : unmanaged
    {
        Write(MemoryMarshal.Cast<T, byte>(values.AsSpan()));
    }

    public void Write(string value)
    {
        var byteCount = Encoding.UTF8.GetByteCount(value);
        Resize<byte>(byteCount + sizeof(int));
        Write(byteCount);
        Encoding.UTF8.GetBytes(value, buffer.AsSpan(cursor));
        cursor += byteCount;
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

    public ReadOnlySpan<T> ReadSpan<T>() where T : unmanaged
    {
        Guard.IsNotNull(buffer, "buffer was null");
        var bytes = Read<int>();
        var span = buffer.AsSpan(cursor, bytes);
        cursor += bytes;
        return MemoryMarshal.Cast<byte, T>(span);
    }

    public void ReadTo<T>(Span<T> target) where T : unmanaged
    {
        var span = ReadSpan<T>();
        span.CopyTo(target);
    }

    public T Read<T>() where T : unmanaged
    {
        Guard.IsNotNull(buffer, "buffer was null");
        var size = Unsafe.SizeOf<T>();
        var value = MemoryMarshal.Read<T>(buffer.AsSpan(cursor));
        cursor += size;
        return value;
    }

    public T[] ReadArray<T>() where T : unmanaged
    {
        return ReadSpan<T>().ToArray();
    }

    public string ReadString()
    {
        Guard.IsNotNull(buffer, "buffer was null");
        var length = Read<int>();
        var value = Encoding.UTF8.GetString(buffer.AsSpan(cursor, length));
        cursor += length;
        return value;
    }
}