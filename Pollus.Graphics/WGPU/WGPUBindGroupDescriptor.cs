namespace Pollus.Graphics.WGPU;

public struct WGPUBindGroupDescriptor
{
    public string Label { get; init; }
    public WGPUBindGroupLayout Layout { get; init; }
    public WGPUBindGroupEntry[] Entries { get; init; }
}
