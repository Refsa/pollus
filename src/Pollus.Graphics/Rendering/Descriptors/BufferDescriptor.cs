using System.Runtime.CompilerServices;
using Pollus.Utils;

namespace Pollus.Graphics.Rendering;

public struct BufferDescriptor
{
    public string Label;
    public BufferUsage Usage;
    public ulong Size;
    public bool MappedAtCreation;

    public static BufferDescriptor Vertex(string label, ulong size)
    {
        return new BufferDescriptor
        {
            Label = label,
            Usage = BufferUsage.Vertex | BufferUsage.CopyDst,
            Size = size,
            MappedAtCreation = false,
        };
    }

    public static BufferDescriptor Index(string label, ulong size)
    {
        return new BufferDescriptor
        {
            Label = label,
            Usage = BufferUsage.Index | BufferUsage.CopyDst,
            Size = size,
            MappedAtCreation = false,
        };
    }

    public static BufferDescriptor Uniform<TUniform>(string label, ulong? dynamicLength = null)
        where TUniform : unmanaged, IShaderType
    {
        return new BufferDescriptor
        {
            Label = label,
            Usage = BufferUsage.Uniform | BufferUsage.CopyDst,
            Size = Alignment.AlignedSize<TUniform>((uint)(dynamicLength ?? 1)),
            MappedAtCreation = false,
        };
    }
}