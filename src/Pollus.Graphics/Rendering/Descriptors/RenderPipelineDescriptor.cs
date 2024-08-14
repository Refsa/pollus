namespace Pollus.Graphics.Rendering;

public struct RenderPipelineDescriptor
{
    public string Label { get; init; }

    public VertexState? VertexState { get; init; }

    public FragmentState? FragmentState { get; init; }

    public MultisampleState? MultisampleState { get; init; }

    public PrimitiveState? PrimitiveState { get; init; }

    public DepthStencilState? DepthStencilState { get; init; }

    public GPUPipelineLayout? PipelineLayout { get; init; }
}
