namespace Pollus.Graphics.Rendering;

public class PipelineLayoutDescriptor
{
    public string Label { get; init; }
    public GPUBindGroupLayout[] Layouts { get; init; }
}
