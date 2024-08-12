namespace Pollus.Graphics.Rendering;

public ref struct BufferDescriptor
{
    public ReadOnlySpan<char> Label;
    public Silk.NET.WebGPU.BufferUsage Usage;
    public ulong Size;
    public ulong Elements;
    public bool MappedAtCreation;
}