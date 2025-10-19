namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUComputePipelineDescriptor
{
    public WGPUChainedStruct* nextInChain;
    public char* label;
    public WGPUPipelineLayout layout;
    public WGPUProgrammableStageDescriptor compute;
}
