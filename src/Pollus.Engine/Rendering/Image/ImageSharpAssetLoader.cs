namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public class ImageSharpAssetLoader : AssetLoader<ImageAsset>
{
    public override string[] Extensions => [".bmp", ".png", ".gif", ".jpg", ".jpeg", ".tiff", ".webp", ".tga"];

    protected override void Load(ReadOnlySpan<byte> data, ref LoadContext<ImageAsset> context)
    {
        var image = Image.Load<Rgba32>(data);
        var bpp = image.PixelType.BitsPerPixel / 8;
        var asset = new ImageAsset
        {
            Name = context.FileName,
            Width = image.Width,
            Height = image.Height,
            Depth = 1,
            Format = ImageAssetFormat.RGBA8,
            Data = new byte[image.Width * image.Height * bpp]
        };
        image.CopyPixelDataTo(asset.Data);

        context.SetAsset(asset);
    }
}