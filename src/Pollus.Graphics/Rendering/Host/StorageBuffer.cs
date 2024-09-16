using System.Runtime.InteropServices;

namespace Pollus.Graphics.Rendering;

public class StorageBuffer : IBufferData
{
    byte[] data;
    uint capacity;

    public uint Stride { get; }
    public uint Alignment { get; }
    public uint Capacity => capacity;

    public BufferType Type => BufferType.Storage;
    public BufferUsage Usage { get; init; } = BufferUsage.Storage;
    public ulong SizeInBytes => (ulong)data.Length;

    public StorageBuffer(uint stride, uint alignment, uint capacity)
    {
        this.capacity = capacity;
        Stride = stride;
        Alignment = alignment;
        data = new byte[stride * capacity];
    }

    public static StorageBuffer From<TElement>(uint capacity, BufferUsage usage)
        where TElement : unmanaged, IShaderType
    {
        return new StorageBuffer(TElement.SizeOf, TElement.AlignOf, capacity)
        {
            Usage = BufferUsage.Storage | usage
        };
    }

    public Span<TElement> AsSpan<TElement>()
        where TElement : unmanaged, IShaderType
    {
        return MemoryMarshal.Cast<byte, TElement>(data);
    }

    public void Write<TElement>(int index, TElement element)
        where TElement : unmanaged, IShaderType
    {
        var span = AsSpan<TElement>();
        span[index] = element;
    }

    public void Write<TElement>(int index, ReadOnlySpan<TElement> elements)
        where TElement : unmanaged, IShaderType
    {
        var span = AsSpan<TElement>();
        elements.CopyTo(span.Slice(index, elements.Length));
    }

    public void WriteTo(GPUBuffer buffer, int offset)
    {
        buffer.Write(data, offset);
    }
}