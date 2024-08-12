namespace Pollus.Graphics.Rendering;

using Pollus.Mathematics;

public ref struct TextureDescriptor
{
    public ReadOnlySpan<char> Label;

    public Silk.NET.WebGPU.TextureUsage Usage;
    public Silk.NET.WebGPU.TextureDimension Dimension;
    public Silk.NET.WebGPU.Extent3D Size;
    public Silk.NET.WebGPU.TextureFormat Format;

    public uint MipLevelCount;
    public uint SampleCount;
    public ReadOnlySpan<Silk.NET.WebGPU.TextureFormat> ViewFormats;

    public static TextureDescriptor D1(ReadOnlySpan<char> label, Silk.NET.WebGPU.TextureUsage usage, Silk.NET.WebGPU.TextureFormat format, uint size, uint mipLevelCount = 1, uint sampleCount = 1)
    {
        return new()
        {
            Label = label,
            Usage = usage,
            Dimension = Silk.NET.WebGPU.TextureDimension.Dimension1D,
            Size = new Silk.NET.WebGPU.Extent3D { Width = size, Height = 1, DepthOrArrayLayers = 1 },
            Format = format,
            MipLevelCount = mipLevelCount,
            SampleCount = sampleCount,
        };
    }

    public static TextureDescriptor D2(ReadOnlySpan<char> label, Silk.NET.WebGPU.TextureUsage usage, Silk.NET.WebGPU.TextureFormat format, Vec2<uint> size, uint mipLevelCount = 1, uint sampleCount = 1)
    {
        return new()
        {
            Label = label,
            Usage = usage,
            Dimension = Silk.NET.WebGPU.TextureDimension.Dimension2D,
            Size = new Silk.NET.WebGPU.Extent3D { Width = size.X, Height = size.Y, DepthOrArrayLayers = 1 },
            Format = format,
            MipLevelCount = mipLevelCount,
            SampleCount = sampleCount,
        };
    }

    public static TextureDescriptor D3(ReadOnlySpan<char> label, Silk.NET.WebGPU.TextureUsage usage, Silk.NET.WebGPU.TextureFormat format, Vec3<uint> size, uint mipLevelCount = 1, uint sampleCount = 1)
    {
        return new()
        {
            Label = label,
            Usage = usage,
            Dimension = Silk.NET.WebGPU.TextureDimension.Dimension3D,
            Size = new Silk.NET.WebGPU.Extent3D { Width = size.X, Height = size.Y, DepthOrArrayLayers = size.Z },
            Format = format,
            MipLevelCount = mipLevelCount,
            SampleCount = sampleCount,
        };
    }
}