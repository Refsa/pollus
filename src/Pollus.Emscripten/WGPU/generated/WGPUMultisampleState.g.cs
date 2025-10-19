namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUMultisampleState
{
    public WGPUChainedStruct* nextInChain;
    public uint count;
    public uint mask;
    public bool alphaToCoverageEnabled;
}
