namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUProgrammableStageDescriptor
{
    public WGPUChainedStruct* NextInChain;
    public WGPUShaderModule Module;
    public byte* EntryPoint;
    public nuint ConstantCount;
    public WGPUConstantEntry* Constants;
}
