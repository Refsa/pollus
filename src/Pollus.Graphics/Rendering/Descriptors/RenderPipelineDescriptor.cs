namespace Pollus.Graphics.Rendering;

public struct RenderPipelineDescriptor
{
    public required string Label { get; set; }

    public VertexState? VertexState { get; set; }

    public FragmentState? FragmentState { get; set; }

    public MultisampleState? MultisampleState { get; set; }

    public PrimitiveState? PrimitiveState { get; set; }

    public DepthStencilState? DepthStencilState { get; set; }

    public GPUPipelineLayout? PipelineLayout { get; set; }
}
