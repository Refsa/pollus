namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUFragmentState
{
    public WGPUChainedStruct* NextInChain;
    public WGPUShaderModule Module;
    public byte* EntryPoint;
    public nuint ConstantCount;
    public WGPUConstantEntry* Constants;
    public nuint TargetCount;
    public WGPUColorTargetState* Targets;
}
