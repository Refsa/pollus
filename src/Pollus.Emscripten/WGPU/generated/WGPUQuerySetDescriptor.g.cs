namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUQuerySetDescriptor
{
    public WGPUChainedStruct* nextInChain;
    public char* label;
    public WGPUQueryType type;
    public uint count;
}
