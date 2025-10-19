namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUBufferDescriptor
{
    public WGPUChainedStruct* nextInChain;
    public char* label;
    public WGPUBufferUsage usage;
    public ulong size;
    public bool mappedAtCreation;
}
