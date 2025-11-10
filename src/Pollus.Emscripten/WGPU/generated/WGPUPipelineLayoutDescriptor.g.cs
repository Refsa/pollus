namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUPipelineLayoutDescriptor
{
    public WGPUChainedStruct* NextInChain;
    public byte* Label;
    public nuint BindGroupLayoutCount;
    public WGPUBindGroupLayout* BindGroupLayouts;
}
