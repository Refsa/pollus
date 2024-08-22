using System.Runtime.CompilerServices;
using Pollus.Utils;

namespace Pollus.Graphics.Rendering;

public ref struct BufferDescriptor
{
    public ReadOnlySpan<char> Label;
    public BufferUsage Usage;
    public ulong Size;
    public bool MappedAtCreation;

    public static BufferDescriptor Vertex(ReadOnlySpan<char> label, ulong size)
    {
        return new BufferDescriptor
        {
            Label = label,
            Usage = BufferUsage.Vertex | BufferUsage.CopyDst,
            Size = size,
            MappedAtCreation = false,
        };
    }

    public static BufferDescriptor Index(ReadOnlySpan<char> label, ulong size)
    {
        return new BufferDescriptor
        {
            Label = label,
            Usage = BufferUsage.Index | BufferUsage.CopyDst,
            Size = size,
            MappedAtCreation = false,
        };
    }

    public static BufferDescriptor Uniform<TUniform>(ReadOnlySpan<char> label, ulong? dynamicLength = null)
        where TUniform : unmanaged
    {
        return new BufferDescriptor
        {
            Label = label,
            Usage = BufferUsage.Uniform | BufferUsage.CopyDst,
            Size = Alignment.GPUAlignedSize<TUniform>(1) * (dynamicLength.HasValue ? dynamicLength.Value : 1),
            MappedAtCreation = false,
        };
    }
}