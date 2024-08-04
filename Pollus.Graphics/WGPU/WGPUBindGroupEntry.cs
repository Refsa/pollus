namespace Pollus.Graphics.WGPU;

public struct WGPUBindGroupEntry
{
    public uint Binding;

    public WGPUBuffer? Buffer;
    public ulong Offset;
    public ulong Size;

    public WGPUSampler? Sampler;
    public WGPUTextureView? TextureView;
}
