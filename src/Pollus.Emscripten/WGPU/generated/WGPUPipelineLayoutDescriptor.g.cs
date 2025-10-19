namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUPipelineLayoutDescriptor
{
    public WGPUChainedStruct* nextInChain;
    public char* label;
    public nuint bindGroupLayoutCount;
    public WGPUBindGroupLayout* bindGroupLayouts;
}
