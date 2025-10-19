namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUBindGroupLayoutDescriptor
{
    public WGPUChainedStruct* nextInChain;
    public char* label;
    public nuint entryCount;
    public WGPUBindGroupLayoutEntry* entries;
}
