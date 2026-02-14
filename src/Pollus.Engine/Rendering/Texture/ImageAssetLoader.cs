namespace Pollus.Engine.Rendering;

using StbImageSharp;
using Pollus.Assets;
using Pollus.Graphics.Rendering;

public class ImageAssetLoader : AssetLoader<Texture2D>
{
    public override string[] Extensions => [".bmp", ".png", ".gif", ".jpg", ".jpeg", ".tiff", ".tga"];

    unsafe protected override void Load(ReadOnlySpan<byte> data, ref LoadContext context)
    {
        fixed (byte* ptr = data)
        {
            using var stream = new UnmanagedMemoryStream(ptr, data.Length);
            var image = ImageResult.FromStream(stream);

            var bpp = image.Comp switch
            {
                ColorComponents.Default => 4,
                ColorComponents.Grey => 1,
                ColorComponents.GreyAlpha => 2,
                ColorComponents.RedGreenBlue => 3,
                ColorComponents.RedGreenBlueAlpha => 4,
                _ => throw new NotSupportedException($"Unsupported color components: {image.Comp}"),
            };

            var asset = new Texture2D
            {
                Name = context.FileName,
                Width = (uint)image.Width,
                Height = (uint)image.Height,
                Format = TextureFormat.Rgba8Unorm,
                Data = new byte[image.Width * image.Height * bpp]
            };

            image.Data.CopyTo(asset.Data);
            context.SetAsset(asset);
        }
    }
}