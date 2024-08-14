namespace Pollus.Graphics.Rendering;

public ref struct PipelineLayoutDescriptor
{
    public ReadOnlySpan<char> Label { get; init; }
    public GPUBindGroupLayout[] Layouts { get; init; }
}
