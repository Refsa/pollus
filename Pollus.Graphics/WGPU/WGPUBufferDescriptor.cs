namespace Pollus.Graphics.WGPU;

public struct WGPUBufferDescriptor
{
    public string? Label;
    public Silk.NET.WebGPU.BufferUsage Usage;
    public ulong Size;
    public ulong Elements;
    public bool MappedAtCreation;
}