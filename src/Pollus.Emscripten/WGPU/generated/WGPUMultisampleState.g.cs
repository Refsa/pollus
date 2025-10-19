namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUMultisampleState
{
    public WGPUChainedStruct* NextInChain;
    public uint Count;
    public uint Mask;
    public bool AlphaToCoverageEnabled;
}
