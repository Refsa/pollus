namespace Pollus.Graphics.Rendering;

using Pollus.Mathematics;

public ref struct TextureViewDescriptor
{
    public ReadOnlySpan<char> Label;
    public TextureFormat Format;
    public TextureViewDimension Dimension;
    public uint BaseMipLevel;
    public uint MipLevelCount;
    public uint BaseArrayLayer;
    public uint ArrayLayerCount;
    public Silk.NET.WebGPU.TextureAspect Aspect;
}