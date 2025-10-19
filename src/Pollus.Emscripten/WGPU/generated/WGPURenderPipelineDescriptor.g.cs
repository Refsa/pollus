namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPURenderPipelineDescriptor
{
    public WGPUChainedStruct* NextInChain;
    public byte* Label;
    public WGPUPipelineLayout Layout;
    public WGPUVertexState Vertex;
    public WGPUPrimitiveState Primitive;
    public WGPUDepthStencilState* DepthStencil;
    public WGPUMultisampleState Multisample;
    public WGPUFragmentState* Fragment;
}
