namespace Pollus.Graphics.Rendering;

public struct BindGroupDescriptor
{
    public string Label { get; init; }
    public GPUBindGroupLayout Layout { get; init; }
    public BindGroupEntry[] Entries { get; init; }
}
