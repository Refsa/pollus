namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUPrimitiveState
{
    public WGPUChainedStruct* NextInChain;
    public WGPUPrimitiveTopology Topology;
    public WGPUIndexFormat StripIndexFormat;
    public WGPUFrontFace FrontFace;
    public WGPUCullMode CullMode;
}
