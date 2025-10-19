namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUProgrammableStageDescriptor
{
    public WGPUChainedStruct* nextInChain;
    public WGPUShaderModule module;
    public char* entryPoint;
    public nuint constantCount;
    public WGPUConstantEntry* constants;
}
