namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPURenderPipelineDescriptor
{
    public WGPUChainedStruct* nextInChain;
    public char* label;
    public WGPUPipelineLayout layout;
    public WGPUVertexState vertex;
    public WGPUPrimitiveState primitive;
    public WGPUDepthStencilState* depthStencil;
    public WGPUMultisampleState multisample;
    public WGPUFragmentState* fragment;
}
