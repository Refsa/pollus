namespace Pollus.Graphics.Rendering;

public ref struct BindGroupDescriptor
{
    public ReadOnlySpan<char> Label { get; init; }
    public GPUBindGroupLayout Layout { get; init; }
    public BindGroupEntry[] Entries { get; init; }
}
