namespace Pollus.Engine.Rendering;

using Core.Assets;
using Pollus.Mathematics;
using Pollus.Graphics.Rendering;
using Pollus.Utils;

public class GlyphSet
{
    public required Handle FontHandle { get; init; }
    public required uint SdfRenderSize { get; init; }
    public required int SdfPadding { get; init; }

    public required uint AtlasWidth { get; init; }
    public required uint AtlasHeight { get; init; }

    public required Dictionary<GlyphKey, Glyph> Glyphs { get; init; }
}

[Asset]
public partial class FontAsset
{
    public required Handle Handle { get; init; }
    public required string Name { get; init; }

    public Handle<FontMaterial> Material { get; set; } = Handle<FontMaterial>.Null;
    public required Handle<Texture2D> Atlas { get; init; }
    public required uint AtlasWidth { get; init; }
    public required uint AtlasHeight { get; init; }

    public required GlyphSet[] GlyphSets { get; init; }

    public GlyphSet GetSetForSize(float size)
    {
        var set = GlyphSets;
        for (int i = 0; i < set.Length; i++)
        {
            if (set[i].SdfRenderSize >= size)
                return set[i];
        }
        return set[^1];
    }
}

public record struct GlyphKey(Handle Font, char Character);

public class Glyph
{
    public char Character { get; init; }
    public RectInt Bounds { get; init; }

    public float Advance { get; init; }
    public float BearingX { get; init; }
    public float BearingY { get; init; }

    public required float Scale { get; init; }
    public required float LineHeight { get; init; }
    public required float Ascender { get; init; }
    public required float Descender { get; init; }
}

public class FontAtlasPacker(int width, int height)
{
    private int currentX;
    private int currentY;
    private int rowHeight;
    private int padding = 1;

    public bool TryPack(int glyphWidth, int glyphHeight, out RectInt bounds)
    {
        if (currentX + glyphWidth + padding > width)
        {
            currentX = 0;
            currentY += rowHeight + padding;
            rowHeight = 0;
        }

        if (currentY + glyphHeight + padding > height)
        {
            bounds = default;
            return false;
        }

        bounds = new RectInt(currentX, currentY, currentX + glyphWidth, currentY + glyphHeight);

        currentX += glyphWidth + padding;
        rowHeight = int.Max(rowHeight, glyphHeight);

        return true;
    }
}
