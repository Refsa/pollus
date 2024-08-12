namespace Pollus.Graphics.Rendering;

using Pollus.Mathematics;

public ref struct TextureViewDescriptor
{
    public ReadOnlySpan<char> Label;
    public Silk.NET.WebGPU.TextureFormat Format;
    public Silk.NET.WebGPU.TextureViewDimension Dimension;
    public uint BaseMipLevel;
    public uint MipLevelCount;
    public uint BaseArrayLayer;
    public uint ArrayLayerCount;
    public Silk.NET.WebGPU.TextureAspect Aspect;
}