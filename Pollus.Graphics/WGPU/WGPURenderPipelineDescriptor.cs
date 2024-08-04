namespace Pollus.Graphics.WGPU;

public struct WGPURenderPipelineDescriptor
{
    public string Label { get; init; }

    public WGPUVertexState? VertexState { get; init; }

    public WGPUFragmentState? FragmentState { get; init; }

    public WGPUMultisampleState? MultisampleState { get; init; }

    public WGPUPrimitiveState? PrimitiveState { get; init; }

    public WGPUDepthStencilState? DepthStencilState { get; init; }

    public WGPUPipelineLayout? PipelineLayout { get; init; }
}
