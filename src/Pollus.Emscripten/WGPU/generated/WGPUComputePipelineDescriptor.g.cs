namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUComputePipelineDescriptor
{
    public WGPUChainedStruct* NextInChain;
    public byte* Label;
    public WGPUPipelineLayout* Layout;
    public WGPUProgrammableStageDescriptor Compute;
}
