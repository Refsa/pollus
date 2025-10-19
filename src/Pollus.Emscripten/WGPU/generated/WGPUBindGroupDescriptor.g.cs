namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUBindGroupDescriptor
{
    public WGPUChainedStruct* nextInChain;
    public char* label;
    public WGPUBindGroupLayout layout;
    public nuint entryCount;
    public WGPUBindGroupEntry* entries;
}
