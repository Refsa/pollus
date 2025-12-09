namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
using StbTrueTypeSharp;

public class FontAssetLoader : AssetLoader<FontAsset>
{
    static readonly string[] extensions = [".ttf", ".otf"];
    public override string[] Extensions => extensions;

    unsafe protected override void Load(ReadOnlySpan<byte> data, ref LoadContext<FontAsset> context)
    {
        var info = new StbTrueType.stbtt_fontinfo();
        fixed (byte* ptr = data)
        {
            if (StbTrueType.stbtt_InitFont(info, ptr, 0) == 0)
                throw new Exception("Failed to init font");
        }

        var glyphSize = 64f;
        int ascent, descent, lineGap;
        StbTrueType.stbtt_GetFontVMetrics(info, &ascent, &descent, &lineGap);

        var scale = StbTrueType.stbtt_ScaleForPixelHeight(info, glyphSize);

        var scaledLineHeight = (ascent - descent + lineGap) * scale;
        var scaledAscent = ascent * scale;
        var scaledDescent = descent * scale;

        int atlasWidth = 4096;
        int atlasHeight = 4096;
        var packer = new FontAtlasPacker(atlasWidth, atlasHeight);

        byte[] atlasData = new byte[atlasWidth * atlasHeight];
        var glyphs = new Dictionary<char, Glyph>();

        fixed (byte* atlasPtr = atlasData)
        {
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

                    glyphs[c] = new Glyph
                    {
                        Character = c,
                        Bounds = bounds,
                        Advance = advance * scale,
                        BearingX = lsb * scale,
                        BearingY = -y0
                    };
                }
                else break;
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
            Name = context.FileName,
            Atlas = texture,
            AtlasWidth = (uint)atlasWidth,
            AtlasHeight = (uint)atlasHeight,
            GlyphSize = (uint)glyphSize,
            Glyphs = glyphs,
            Packer = packer,
            LineHeight = scaledLineHeight,
            Ascender = scaledAscent,
            Descender = scaledDescent
        };

        context.SetAsset(asset);
    }
}