namespace Pollus.Graphics.Rendering;

public ref struct ComputePipelineDescriptor
{
    public string Label { get; init; }
    public GPUPipelineLayout? Layout { get; init; }
    public ProgrammableStageDescriptor Compute { get; init; }
}
