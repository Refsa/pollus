namespace Pollus.Engine.Rendering;

using Pollus.Assets;
using Pollus.Graphics.Rendering;
using StbTrueTypeSharp;
using Utils;

public class FontAssetLoader : AssetLoader<FontAsset>
{
    static readonly string[] extensions = [".ttf", ".otf"];
    public override string[] Extensions => extensions;

    static readonly (uint SdfRenderSize, int SdfPadding)[] SetConfigs =
    [
        (24, 8),
        (48, 8),
        (96, 8),
    ];

    const byte OnEdgeValue = 128;
    const int AtlasSize = 4096;

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

        // Single shared atlas for all sets
        var packer = new FontAtlasPacker(AtlasSize, AtlasSize);
        byte[] atlasData = new byte[AtlasSize * AtlasSize];
        var sets = new GlyphSet[SetConfigs.Length];

        fixed (byte* atlasPtr = atlasData)
        {
            for (int t = 0; t < SetConfigs.Length; t++)
            {
                var (sdfRenderSize, sdfPadding) = SetConfigs[t];
                float pixelDistScale = 128.0f / sdfPadding;

                var glyphs = new Dictionary<GlyphKey, Glyph>();
                var scale = StbTrueType.stbtt_ScaleForMappingEmToPixels(info, sdfRenderSize);

                for (char c = (char)32; c < 256; c++)
                {
                    int advance, lsb;
                    StbTrueType.stbtt_GetCodepointHMetrics(info, c, &advance, &lsb);

                    int w, h, xoff, yoff;
                    var sdfData = StbTrueType.stbtt_GetCodepointSDF(info, scale, c, sdfPadding, OnEdgeValue, pixelDistScale, &w, &h, &xoff, &yoff);

                    if (sdfData == null || w == 0 || h == 0)
                    {
                        if (sdfData != null) StbTrueType.stbtt_FreeSDF(sdfData, null);

                        var glyphKey = new GlyphKey(context.Handle, c);
                        glyphs[glyphKey] = new Glyph
                        {
                            Character = c,
                            Bounds = new Mathematics.RectInt(0, 0, 0, 0),
                            Advance = advance * scale,
                            BearingX = 0,
                            BearingY = 0,
                            LineHeight = (ascent - descent + lineGap) * scale,
                            Ascender = ascent * scale,
                            Descender = descent * scale,
                            Scale = scale,
                        };
                        continue;
                    }

                    if (packer.TryPack(w, h, out var bounds))
                    {
                        var boundsPivot = bounds.TopLeft();
                        for (int row = 0; row < h; row++)
                        {
                            int atlasOffset = (boundsPivot.Y + row) * AtlasSize + boundsPivot.X;
                            int sdfOffset = row * w;
                            Buffer.MemoryCopy(sdfData + sdfOffset, atlasPtr + atlasOffset, w, w);
                        }

                        var glyphKey = new GlyphKey(context.Handle, c);
                        glyphs[glyphKey] = new Glyph
                        {
                            Character = c,
                            Bounds = bounds,
                            Advance = advance * scale,
                            BearingX = (float)xoff,
                            BearingY = -(float)yoff,
                            LineHeight = (ascent - descent + lineGap) * scale,
                            Ascender = ascent * scale,
                            Descender = descent * scale,
                            Scale = scale,
                        };
                    }

                    StbTrueType.stbtt_FreeSDF(sdfData, null);
                }

                sets[t] = new GlyphSet
                {
                    FontHandle = context.Handle,
                    SdfRenderSize = sdfRenderSize,
                    SdfPadding = sdfPadding,
                    AtlasWidth = AtlasSize,
                    AtlasHeight = AtlasSize,
                    Glyphs = glyphs,
                };
            }
        }

        var texture = new Texture2D
        {
            Name = context.FileName + "_Atlas",
            Width = AtlasSize,
            Height = AtlasSize,
            Format = TextureFormat.R8Unorm,
            Data = atlasData
        };

        var asset = new FontAsset
        {
            Handle = context.Handle,
            Name = context.FileName,
            Atlas = context.AssetServer.Assets.AddAsset(texture, context.Path + ":atlas.png"),
            AtlasWidth = AtlasSize,
            AtlasHeight = AtlasSize,
            GlyphSets = sets,
        };

        context.SetAsset(asset);
    }
}
