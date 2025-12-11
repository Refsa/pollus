namespace Pollus.Graphics.Rendering;

public struct TextureViewDescriptor
{
    public string Label;
    public TextureFormat Format;
    public TextureViewDimension Dimension;
    public uint BaseMipLevel;
    public uint MipLevelCount;
    public uint BaseArrayLayer;
    public uint ArrayLayerCount;
    public TextureAspect Aspect;

    public static TextureViewDescriptor D2 => new()
    {
        Dimension = TextureViewDimension.Dimension2D,
        BaseMipLevel = 0,
        MipLevelCount = 1,
        BaseArrayLayer = 0,
        ArrayLayerCount = 1,
        Aspect = TextureAspect.All
    };

    public override int GetHashCode()
    {
        return HashCode.Combine(Label, Format, Dimension, BaseMipLevel, MipLevelCount, BaseArrayLayer, ArrayLayerCount, Aspect);
    }
}