namespace Pollus.Tests.Rendering;

using Pollus.Collections;
using Pollus.Engine.Rendering;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
using Pollus.Utils;

public class TextBuilderTests
{
    static FontAsset CreateTestFont(float lineHeight = 20f, float ascender = 16f, float descender = -4f)
    {
        var handle = new Handle<FontAsset>(1);
        var glyphs = new Dictionary<GlyphKey, Glyph>();

        var sizePow = 16u;
        var chars = new (char c, float advance, float bearingX, float bearingY, int w, int h)[]
        {
            ('A', 10f, 1f, 14f, 8, 14),
            ('B', 10f, 1f, 14f, 8, 14),
            ('H', 10f, 1f, 14f, 8, 14),
            (' ', 5f, 0f, 0f, 0, 0),
        };

        foreach (var (c, advance, bearingX, bearingY, w, h) in chars)
        {
            var key = new GlyphKey(handle, sizePow, c);
            glyphs[key] = new Glyph
            {
                Character = c,
                Bounds = new RectInt(0, 0, w, h),
                Advance = advance,
                BearingX = bearingX,
                BearingY = bearingY,
                Scale = 1f,
                LineHeight = lineHeight,
                Ascender = ascender,
                Descender = descender,
            };
        }

        return new FontAsset
        {
            Handle = handle,
            Name = "TestFont",
            Atlas = Handle<Texture2D>.Null,
            AtlasWidth = 256,
            AtlasHeight = 256,
            Glyphs = glyphs,
            Packer = new FontAtlasPacker(256, 256),
        };
    }

    [Fact]
    public void YUp_ExistingBehavior_Preserved()
    {
        var font = CreateTestFont();
        var text = new NativeUtf8("AB");
        var verts = new ArrayList<TextBuilder.TextVertex>();
        var indices = new ArrayList<uint>();

        TextBuilder.BuildMesh(text, font, Vec2f.Zero, Vec4f.One, 16f, verts, indices);

        // Y-up: bearingY goes up (positive Y)
        Assert.True(verts.Count >= 4, "Should have at least 4 vertices for first char");
        // First char 'A': y = cursorY + bearingY * scale = 0 + 14 * 1 = 14
        Assert.Equal(14f, verts[0].Position.Y);
        // Bottom: y - h = 14 - 14 = 0
        Assert.Equal(0f, verts[2].Position.Y);

        text.Dispose();
    }

    [Fact]
    public void YDown_FirstChar_PositionedCorrectly()
    {
        var font = CreateTestFont();
        var text = new NativeUtf8("A");
        var verts = new ArrayList<TextBuilder.TextVertex>();
        var indices = new ArrayList<uint>();

        TextBuilder.BuildMesh(text, font, Vec2f.Zero, Vec4f.One, 16f, verts, indices, TextCoordinateMode.YDown);

        // Y-down: y = cursorY + (ascender - bearingY) * scale = 0 + (16 - 14) * 1 = 2
        Assert.Equal(2f, verts[0].Position.Y);
        // Bottom: y + h = 2 + 14 = 16
        Assert.Equal(16f, verts[2].Position.Y);

        text.Dispose();
    }

    [Fact]
    public void YDown_Newline_CursorMovesDown()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var text = new NativeUtf8("A\nB");
        var verts = new ArrayList<TextBuilder.TextVertex>();
        var indices = new ArrayList<uint>();

        TextBuilder.BuildMesh(text, font, Vec2f.Zero, Vec4f.One, 16f, verts, indices, TextCoordinateMode.YDown);

        // First char 'A' at line 0: y = 0 + (16 - 14) = 2
        Assert.Equal(2f, verts[0].Position.Y);

        // Second char 'B' at line 1: cursorY = 0 + lineHeight*scale = 20
        // y = 20 + (16 - 14) = 22
        Assert.Equal(22f, verts[4].Position.Y);

        text.Dispose();
    }

    [Fact]
    public void YDown_XPositionSameAsYUp()
    {
        var font = CreateTestFont();
        var text = new NativeUtf8("AB");

        var vertsUp = new ArrayList<TextBuilder.TextVertex>();
        var indicesUp = new ArrayList<uint>();
        TextBuilder.BuildMesh(text, font, Vec2f.Zero, Vec4f.One, 16f, vertsUp, indicesUp);

        var vertsDown = new ArrayList<TextBuilder.TextVertex>();
        var indicesDown = new ArrayList<uint>();
        TextBuilder.BuildMesh(text, font, Vec2f.Zero, Vec4f.One, 16f, vertsDown, indicesDown, TextCoordinateMode.YDown);

        // X positions should be the same regardless of coordinate mode
        Assert.Equal(vertsUp[0].Position.X, vertsDown[0].Position.X);
        Assert.Equal(vertsUp[4].Position.X, vertsDown[4].Position.X);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_SingleLine_ReturnsAdvanceWidth()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var text = new NativeUtf8("AB");

        var size = TextBuilder.MeasureText(text, font, 16f);

        // Two chars, each with advance=10, scale=1 -> width = 20
        Assert.Equal(20f, size.X);
        // Single line -> height = lineHeight * scale = 20
        Assert.Equal(20f, size.Y);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_TrailingSpaces_IncludedInWidth()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var text = new NativeUtf8("A ");

        var size = TextBuilder.MeasureText(text, font, 16f);

        // 'A' advance=10, ' ' advance=5, scale=1 -> width = 15
        Assert.Equal(15f, size.X);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_MultiLine_ReturnsMaxWidthAndTotalHeight()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var text = new NativeUtf8("AB\nA");

        var size = TextBuilder.MeasureText(text, font, 16f);

        // Line 1: "AB" = 20, Line 2: "A" = 10, max = 20
        Assert.Equal(20f, size.X);
        // Two lines -> height = 2 * lineHeight * scale = 40
        Assert.Equal(40f, size.Y);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_EmptyText_ReturnsZero()
    {
        var font = CreateTestFont();
        var text = new NativeUtf8("");

        var size = TextBuilder.MeasureText(text, font, 16f);

        Assert.Equal(0f, size.X);
        Assert.Equal(0f, size.Y);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_WithScale_AppliesCorrectly()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var text = new NativeUtf8("A");

        var size = TextBuilder.MeasureText(text, font, 16f);

        // advance=10, scale=1 -> width = 10
        Assert.Equal(10f, size.X);
        Assert.Equal(20f, size.Y);

        text.Dispose();
    }
}
