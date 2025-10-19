namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUBindGroupEntry
{
    public WGPUChainedStruct* NextInChain;
    public uint Binding;
    public WGPUBuffer Buffer;
    public ulong Offset;
    public ulong Size;
    public WGPUSampler Sampler;
    public WGPUTextureView TextureView;
}
