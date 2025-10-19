namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUBufferDescriptor
{
    public WGPUChainedStruct* NextInChain;
    public byte* Label;
    public WGPUBufferUsage Usage;
    public ulong Size;
    public bool MappedAtCreation;
}
