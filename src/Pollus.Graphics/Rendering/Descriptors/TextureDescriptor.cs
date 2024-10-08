namespace Pollus.Graphics.Rendering;

using System.Runtime.CompilerServices;
using Pollus.Mathematics;

public struct TextureDescriptor
{
    public string Label;

    public TextureUsage Usage;
    public TextureDimension Dimension;
    public Extent3D Size;
    public TextureFormat Format;

    public uint MipLevelCount;
    public uint SampleCount;
    public ViewFormatArray ViewFormats;

    public override int GetHashCode()
    {
        return HashCode.Combine(Label, Usage, Dimension, Size, Format, MipLevelCount, SampleCount, ViewFormats);
    }

    public static TextureDescriptor D1(string label, TextureUsage usage, TextureFormat format, uint size, uint mipLevelCount = 1, uint sampleCount = 1)
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

    public static TextureDescriptor D2(string label, TextureUsage usage, TextureFormat format, Vec2<uint> size, uint mipLevelCount = 1, uint sampleCount = 1)
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

    public static TextureDescriptor D3(string label, TextureUsage usage, TextureFormat format, Vec3<uint> size, uint mipLevelCount = 1, uint sampleCount = 1)
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

    [InlineArray(4)]
    public struct ViewFormatArray
    {
        TextureFormat _first;

        public override int GetHashCode()
        {
            return HashCode.Combine(this[0], this[1], this[2], this[3]);
        }
    }
}