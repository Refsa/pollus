namespace Pollus.Graphics.Rendering;

public struct BindGroupEntry
{
    public uint Binding;

    public GPUBuffer? Buffer;
    public ulong Offset;
    public ulong Size;

    public GPUSampler? Sampler;
    public GPUTextureView? TextureView;
}
