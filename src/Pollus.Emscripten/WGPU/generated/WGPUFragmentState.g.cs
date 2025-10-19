namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUFragmentState
{
    public WGPUChainedStruct* nextInChain;
    public WGPUShaderModule module;
    public char* entryPoint;
    public nuint constantCount;
    public WGPUConstantEntry* constants;
    public nuint targetCount;
    public WGPUColorTargetState* targets;
}
