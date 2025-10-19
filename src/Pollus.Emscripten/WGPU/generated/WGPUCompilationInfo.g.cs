namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUCompilationInfo
{
    public WGPUChainedStruct* nextInChain;
    public nuint messageCount;
    public WGPUCompilationMessage* messages;
}
