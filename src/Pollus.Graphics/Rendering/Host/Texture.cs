namespace Pollus.Graphics.Rendering;

using Core.Assets;

public interface ITexture
{
    public string Name { get; }
    public TextureFormat Format { get; }
    public TextureDimension Dimension { get; }
    public uint BytesPerPixel => Format.BytesPerPixel();

    public uint Width { get; }
    public uint Height { get; }
    public uint Depth { get; }

    public uint MipCount => 1;
    public uint SampleCount => 1;

    public byte[] Data { get; }
}

[Asset]
public partial class Texture2D : ITexture
{
    public required string Name { get; init; }
    public required TextureFormat Format { get; init; }
    public TextureDimension Dimension => TextureDimension.Dimension2D;
    public required uint Width { get; init; }
    public required uint Height { get; init; }
    public uint Depth => 1;

    public byte[] Data { get; init; } = [];
}