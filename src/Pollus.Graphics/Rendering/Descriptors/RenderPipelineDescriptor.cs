namespace Pollus.Graphics.Rendering;

public struct RenderPipelineDescriptor
{
    public static readonly RenderPipelineDescriptor Default = new()
    {
        Label = "default",
        VertexState = new(),
        FragmentState = new(),
        MultisampleState = MultisampleState.Default,
        PrimitiveState = PrimitiveState.Default,
    };

    public required string Label { get; set; }

    public required VertexState VertexState { get; set; }

    public required FragmentState FragmentState { get; set; }

    public required MultisampleState MultisampleState { get; set; }

    public required PrimitiveState PrimitiveState { get; set; }

    public DepthStencilState? DepthStencilState { get; set; }

    public GPUPipelineLayout? PipelineLayout { get; set; }
}
