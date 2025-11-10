namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUBindGroupDescriptor
{
    public WGPUChainedStruct* NextInChain;
    public byte* Label;
    public WGPUBindGroupLayout* Layout;
    public nuint EntryCount;
    public WGPUBindGroupEntry* Entries;
}
