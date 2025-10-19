namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUBindGroupEntry
{
    public WGPUChainedStruct* nextInChain;
    public uint binding;
    public WGPUBuffer buffer;
    public ulong offset;
    public ulong size;
    public WGPUSampler sampler;
    public WGPUTextureView textureView;
}
