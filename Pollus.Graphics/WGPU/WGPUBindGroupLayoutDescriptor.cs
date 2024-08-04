namespace Pollus.Graphics.WGPU;

public class WGPUBindGroupLayoutDescriptor
{
    public string Label { get; init; }
    public WGPUBindGroupLayoutEntry[] Entries { get; init; }
}
