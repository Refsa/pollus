namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUPrimitiveState
{
    public WGPUChainedStruct* nextInChain;
    public WGPUPrimitiveTopology topology;
    public WGPUIndexFormat stripIndexFormat;
    public WGPUFrontFace frontFace;
    public WGPUCullMode cullMode;
}
