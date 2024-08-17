namespace Pollus.Graphics.Rendering;

using Pollus.Mathematics;

public ref struct TextureDescriptor
{
    public ReadOnlySpan<char> Label;

    public TextureUsage Usage;
    public TextureDimension Dimension;
    public Extent3D Size;
    public TextureFormat Format;

    public uint MipLevelCount;
    public uint SampleCount;
    public ReadOnlySpan<TextureFormat> ViewFormats;

    public static TextureDescriptor D1(ReadOnlySpan<char> label, TextureUsage usage, TextureFormat format, uint size, uint mipLevelCount = 1, uint sampleCount = 1)
    {
        return new()
        {
            Label = label,
            Usage = usage,
            Dimension = TextureDimension.Dimension1D,
            Size = new Extent3D { Width = size, Height = 1, DepthOrArrayLayers = 1 },
            Format = format,
            MipLevelCount = mipLevelCount,
            SampleCount = sampleCount,
        };
    }

    public static TextureDescriptor D2(ReadOnlySpan<char> label, TextureUsage usage, TextureFormat format, Vec2<uint> size, uint mipLevelCount = 1, uint sampleCount = 1)
    {
        return new()
        {
            Label = label,
            Usage = usage,
            Dimension = TextureDimension.Dimension2D,
            Size = new Extent3D { Width = size.X, Height = size.Y, DepthOrArrayLayers = 1 },
            Format = format,
            MipLevelCount = mipLevelCount,
            SampleCount = sampleCount,
        };
    }

    public static TextureDescriptor D3(ReadOnlySpan<char> label, TextureUsage usage, TextureFormat format, Vec3<uint> size, uint mipLevelCount = 1, uint sampleCount = 1)
    {
        return new()
        {
            Label = label,
            Usage = usage,
            Dimension = TextureDimension.Dimension3D,
            Size = new Extent3D { Width = size.X, Height = size.Y, DepthOrArrayLayers = size.Z },
            Format = format,
            MipLevelCount = mipLevelCount,
            SampleCount = sampleCount,
        };
    }
}