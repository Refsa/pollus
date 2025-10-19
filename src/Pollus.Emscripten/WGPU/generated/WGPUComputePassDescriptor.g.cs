namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUComputePassDescriptor
{
    public WGPUChainedStruct* nextInChain;
    public char* label;
    public WGPUComputePassTimestampWrites* timestampWrites;
}
