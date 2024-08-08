namespace Pollus.Graphics.Rendering;

public struct BufferDescriptor
{
    public string? Label;
    public Silk.NET.WebGPU.BufferUsage Usage;
    public ulong Size;
    public ulong Elements;
    public bool MappedAtCreation;
}