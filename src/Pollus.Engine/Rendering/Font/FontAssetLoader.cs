namespace Pollus.Engine.Rendering;

using Pollus.Assets;
using Pollus.Graphics.Rendering;
using StbTrueTypeSharp;
using Utils;

public class FontAssetLoader : AssetLoader<FontAsset>
{
    static readonly string[] extensions = [".ttf", ".otf"];
    public override string[] Extensions => extensions;

    unsafe protected override void Load(ReadOnlySpan<byte> data, ref LoadContext context)
    {
        var info = new StbTrueType.stbtt_fontinfo();
        fixed (byte* ptr = data)
        {
            if (StbTrueType.stbtt_InitFont(info, ptr, 0) == 0)
                throw new Exception("Failed to init font");
        }

        int ascent, descent, lineGap;
        StbTrueType.stbtt_GetFontVMetrics(info, &ascent, &descent, &lineGap);

        int atlasWidth = 4096;
        int atlasHeight = 4096;
        var packer = new FontAtlasPacker(atlasWidth, atlasHeight);

        var glyphs = new Dictionary<GlyphKey, Glyph>();
        byte[] atlasData = new byte[atlasWidth * atlasHeight];

        fixed (byte* atlasPtr = atlasData)
        {
            for (uint glyphSize = 8; glyphSize <= 128; glyphSize += 4)
            {
                var scale = StbTrueType.stbtt_ScaleForMappingEmToPixels(info, glyphSize);

                for (char c = (char)32; c < 256; c++)
                {
                    int advance, lsb;
                    StbTrueType.stbtt_GetCodepointHMetrics(info, c, &advance, &lsb);

                    int x0, y0, x1, y1;
                    StbTrueType.stbtt_GetCodepointBitmapBox(info, c, scale, scale, &x0, &y0, &x1, &y1);

                    int w = x1 - x0;
                    int h = y1 - y0;

                    if (packer.TryPack(w, h, out var bounds))
                    {
                        var boundsPivot = bounds.TopLeft();
                        int offset = boundsPivot.Y * atlasWidth + boundsPivot.X;
                        StbTrueType.stbtt_MakeCodepointBitmap(info, atlasPtr + offset, w, h, atlasWidth, scale, scale, c);

                        var glyphKey = new GlyphKey(context.Handle, glyphSize, c);
                        glyphs[glyphKey] = new Glyph
                        {
                            Character = c,
                            Bounds = bounds,
                            Advance = advance * scale,
                            BearingX = lsb * scale,
                            BearingY = -y0,
                            LineHeight = (ascent - descent + lineGap) * scale,
                            Ascender = ascent * scale,
                            Descender = descent * scale,
                            Scale = scale,
                        };
                    }
                    else break;
                }
            }
        }

        var texture = new Texture2D
        {
            Name = context.FileName + "_Atlas",
            Width = (uint)atlasWidth,
            Height = (uint)atlasHeight,
            Format = TextureFormat.R8Unorm,
            Data = atlasData
        };

        var asset = new FontAsset
        {
            Handle = context.Handle,
            Name = context.FileName,
            Atlas = context.AssetServer.Assets.AddAsset(texture, context.Path + ":atlas.png"),
            AtlasWidth = (uint)atlasWidth,
            AtlasHeight = (uint)atlasHeight,
            Glyphs = glyphs,
            Packer = packer,
            Material = Handle<FontMaterial>.Null,
        };

        context.SetAsset(asset);
    }
}