namespace Pollus.Engine.Rendering;

using Pollus.Mathematics;
using Pollus.Graphics.Rendering;
using Pollus.Utils;

public class FontAsset
{
    public required string Name { get; init; }

    public Handle<FontMaterial> Material { get; set; } = Handle<FontMaterial>.Null;
    public required Texture2D Atlas { get; init; }
    public required uint AtlasWidth { get; init; }
    public required uint AtlasHeight { get; init; }
    public required uint GlyphSize { get; init; }

    public required Dictionary<char, Glyph> Glyphs { get; init; }
    public required FontAtlasPacker Packer { get; init; }
    public required float LineHeight { get; init; }
    public required float Ascender { get; init; }
    public required float Descender { get; init; }
}

public struct Glyph
{
    public char Character { get; init; }
    public RectInt Bounds { get; init; }

    public float Advance { get; init; }
    public float BearingX { get; init; }
    public float BearingY { get; init; }
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