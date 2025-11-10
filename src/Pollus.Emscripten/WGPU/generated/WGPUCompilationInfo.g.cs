namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUCompilationInfo
{
    public WGPUChainedStruct* NextInChain;
    public nuint MessageCount;
    public WGPUCompilationMessage* Messages;
}
