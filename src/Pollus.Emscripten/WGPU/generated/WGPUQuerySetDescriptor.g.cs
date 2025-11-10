namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUQuerySetDescriptor
{
    public WGPUChainedStruct* NextInChain;
    public byte* Label;
    public WGPUQueryType Type;
    public uint Count;
}
