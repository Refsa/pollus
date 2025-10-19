namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUConstantEntry
{
    public WGPUChainedStruct* nextInChain;
    public char* key;
    public double value;
}
