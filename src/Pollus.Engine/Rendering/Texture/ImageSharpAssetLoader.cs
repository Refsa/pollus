namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public class ImageSharpAssetLoader : AssetLoader<Texture2D>
{
    public override string[] Extensions => [".bmp", ".png", ".gif", ".jpg", ".jpeg", ".tiff", ".webp", ".tga"];

    protected override void Load(ReadOnlySpan<byte> data, ref LoadContext<Texture2D> context)
    {
        var image = Image.Load<Rgba32>(data);
        var bpp = image.PixelType.BitsPerPixel / 8;

        var asset = new Texture2D
        {
            Name = context.FileName,
            Width = (uint)image.Width,
            Height = (uint)image.Height,
            Format = TextureFormat.Rgba8Unorm,
            Data = new byte[image.Width * image.Height * bpp]
        };
        image.CopyPixelDataTo(asset.Data);

        context.SetAsset(asset);
    }
}