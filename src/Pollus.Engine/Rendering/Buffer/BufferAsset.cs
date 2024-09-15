namespace Pollus.Engine.Rendering;

using System.Runtime.InteropServices;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;

public class Buffer
{
    byte[] data;
    uint capacity;

    public uint Stride { get; }
    public uint Alignment { get; }
    public uint Capacity => capacity;

    public Buffer(uint stride, uint alignment, uint capacity)
    {
        this.capacity = capacity;
        Stride = stride;
        Alignment = alignment;
        data = new byte[stride * capacity];
    }

    public static Buffer From<TElement>(uint capacity)
        where TElement : unmanaged, IShaderType
    {
        return new Buffer(TElement.SizeOf, TElement.AlignOf, capacity);
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