namespace Pollus.Graphics.Rendering;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public struct VertexData : IBufferData
{
    public const int MAX_ATTRIBUTES = 8;

    [InlineArray(MAX_ATTRIBUTES)]
    struct AttributeArray
    {
        Attribute _first;
    }

    public record struct Attribute(int Offset, VertexFormat VertexFormat);

    byte[] data;
    int attributeCount;
    AttributeArray attributes;
    uint stride;

    public ulong SizeInBytes => (ulong)data.Length;
    public uint Stride => stride;
    public uint Count => (uint)SizeInBytes / stride;
    public int AttributeCount => attributeCount;
    public BufferType Usage => BufferType.Vertex;

    // create array accessor
    public Span<byte> this[int index]
    {
        get => data.AsSpan().Slice((int)(index * stride), (int)stride);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public VertexData(uint capacity, uint stride, ReadOnlySpan<Attribute> attributes)
    {
        this.stride = stride;
        attributeCount = attributes.Length;
        attributes.CopyTo(this.attributes);
        data = new byte[(int)(capacity * stride)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Span<byte> AsSpan()
    {
        return data.AsSpan();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Span<byte> Slice(int start)
    {
        return data.AsSpan().Slice(start * (int)stride);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Span<byte> Slice(int start, int length)
    {
        return data.AsSpan().Slice(start * (int)stride, length * (int)stride);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Write<T0>(int index, in T0 value, int elementIndex = 0)
        where T0 : unmanaged
    {
        MemoryMarshal.Write(this[index][attributes[elementIndex].Offset..], value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Write<T0>(int offset, ReadOnlySpan<T0> values)
        where T0 : unmanaged
    {
        var dst = MemoryMarshal.Cast<byte, T0>(data.AsSpan().Slice(offset));
        values.CopyTo(dst);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Write<T0, T1>(int index, in T0 value0, in T1 value1)
        where T0 : unmanaged
        where T1 : unmanaged
    {
        Write(index, value0, 0);
        Write(index, value1, 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Write<T0, T1>(int offset, ReadOnlySpan<(T0, T1)> values)
        where T0 : unmanaged
        where T1 : unmanaged
    {
        Write<(T0, T1)>(offset, values);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Write<T0, T1, T2>(int index, in T0 value0, in T1 value1, in T2 value2)
        where T0 : unmanaged
        where T1 : unmanaged
        where T2 : unmanaged
    {
        Write(index, value0, 0);
        Write(index, value1, 1);
        Write(index, value2, 2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Write<T0, T1, T2>(int offset, ReadOnlySpan<(T0, T1, T2)> values)
        where T0 : unmanaged
        where T1 : unmanaged
        where T2 : unmanaged
    {
        Write<(T0, T1, T2)>(offset, values);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Write<T0, T1, T2, T3>(int index, in T0 value0, in T1 value1, in T2 value2, in T3 value3)
        where T0 : unmanaged
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
    {
        Write(index, value0, 0);
        Write(index, value1, 1);
        Write(index, value2, 2);
        Write(index, value3, 3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Write<T0, T1, T2, T3>(int offset, ReadOnlySpan<(T0, T1, T2, T3)> values)
        where T0 : unmanaged
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
    {
        Write<(T0, T1, T2, T3)>(offset, values);
    }

    public static VertexData From(uint capacity, ReadOnlySpan<VertexFormat> formats)
    {
        if (formats.Length > MAX_ATTRIBUTES)
        {
            throw new ArgumentOutOfRangeException(nameof(formats), "Too many attributes");
        }

        Span<Attribute> attributes = stackalloc Attribute[formats.Length];
        uint stride = 0;
        for (int i = 0; i < formats.Length; i++)
        {
            attributes[i] = new Attribute((int)stride, formats[i]);
            stride += formats[i].Stride();
        }
        return new VertexData(capacity, stride, attributes);
    }
}