namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUBindGroupLayoutDescriptor
{
    public WGPUChainedStruct* NextInChain;
    public byte* Label;
    public nuint EntryCount;
    public WGPUBindGroupLayoutEntry* Entries;
}
