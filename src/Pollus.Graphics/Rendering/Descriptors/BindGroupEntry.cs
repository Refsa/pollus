namespace Pollus.Graphics.Rendering;

public struct BindGroupEntry
{
    public uint Binding;

    public GPUBuffer? Buffer;
    public ulong Offset;
    public ulong Size;

    public GPUSampler? Sampler;
    public GPUTextureView? TextureView;

    BindGroupEntry(uint binding)
    {
        Binding = binding;
    }

    BindGroupEntry(uint binding, GPUTextureView? textureView) : this(binding)
    {
        TextureView = textureView;
    }

    BindGroupEntry(uint binding, GPUSampler? sampler) : this(binding)
    {
        Sampler = sampler;
    }

    BindGroupEntry(uint binding, GPUBuffer buffer, ulong offset, ulong size) : this(binding)
    {
        Buffer = buffer;
        Offset = offset;
        Size = size;
    }

    public static BindGroupEntry BufferEntry<T>(uint binding, GPUBuffer buffer, ulong offset)
        where T : unmanaged, IShaderType
    {
        return new BindGroupEntry(binding, buffer, offset, buffer.Size);
    }

    public static BindGroupEntry SamplerEntry(uint binding, GPUSampler sampler)
    {
        return new BindGroupEntry(binding, sampler);
    }

    public static BindGroupEntry TextureEntry(uint binding, GPUTextureView textureView)
    {
        return new BindGroupEntry(binding, textureView);
    }
}
