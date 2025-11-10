namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUComputePassDescriptor
{
    public WGPUChainedStruct* NextInChain;
    public byte* Label;
    public WGPUComputePassTimestampWrites* TimestampWrites;
}
