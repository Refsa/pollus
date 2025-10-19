namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUConstantEntry
{
    public WGPUChainedStruct* NextInChain;
    public byte* Key;
    public double Value;
}
