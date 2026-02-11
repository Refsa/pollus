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

    // --- Word wrapping: MeasureText tests ---

    [Fact]
    public void MeasureText_SingleWord_FitsWithinMaxWidth_NoWrap()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var text = new NativeUtf8("AB");

        var size = TextBuilder.MeasureText(text, font, 16f, maxWidth: 30f);

        // "AB" = 20, fits within 30 → no wrap
        Assert.Equal(20f, size.X);
        Assert.Equal(20f, size.Y);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_TwoWords_SecondWraps()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var text = new NativeUtf8("AB AB");

        var size = TextBuilder.MeasureText(text, font, 16f, maxWidth: 25f);

        // "AB " = 25, "AB" wraps to line 2
        // Width = max(25, 20) = 25, Height = 2 * 20 = 40
        Assert.Equal(25f, size.X);
        Assert.Equal(40f, size.Y);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_LongWord_ExceedsMaxWidth_NoCharBreak()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var text = new NativeUtf8("ABBA");

        var size = TextBuilder.MeasureText(text, font, 16f, maxWidth: 15f);

        // "ABBA" = 40, exceeds 15 but no word break possible → stays 1 line
        Assert.Equal(40f, size.X);
        Assert.Equal(20f, size.Y);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_MultipleWordsWrap()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var text = new NativeUtf8("A B A B");

        var size = TextBuilder.MeasureText(text, font, 16f, maxWidth: 15f);

        // "A " = 15, "B " wraps → "B " = 15, "A " wraps → "A " = 15, "B" wraps → "B" = 10
        // 4 lines, maxWidth = 15, height = 80
        Assert.Equal(15f, size.X);
        Assert.Equal(80f, size.Y);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_MaxWidthZero_NoWrap()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var text = new NativeUtf8("AB AB");

        var size = TextBuilder.MeasureText(text, font, 16f, maxWidth: 0f);

        // maxWidth=0 means no wrapping → "AB AB" = 10+10+5+10+10 = 45, 1 line
        Assert.Equal(45f, size.X);
        Assert.Equal(20f, size.Y);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_ExplicitNewline_PlusWordWrap()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var text = new NativeUtf8("AB\nAB AB");

        var size = TextBuilder.MeasureText(text, font, 16f, maxWidth: 25f);

        // Line 1: "AB" = 20 (explicit \n)
        // Line 2: "AB " = 25, then "AB" wraps to line 3
        // 3 lines, maxWidth = 25, height = 60
        Assert.Equal(25f, size.X);
        Assert.Equal(60f, size.Y);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_WordAtExactMaxWidth_Wraps()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var text = new NativeUtf8("AB AB");

        var size = TextBuilder.MeasureText(text, font, 16f, maxWidth: 25f);

        // "AB " = 25, exactly at maxWidth → "AB" wraps to line 2
        // 2 lines
        Assert.Equal(25f, size.X);
        Assert.Equal(40f, size.Y);

        text.Dispose();
    }

    // --- Word wrapping: BuildMesh tests ---

    [Fact]
    public void BuildMesh_WordWrap_YDown_SecondWordOnNewLine()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var text = new NativeUtf8("AB AB");
        var verts = new ArrayList<TextBuilder.TextVertex>();
        var indices = new ArrayList<uint>();

        TextBuilder.BuildMesh(text, font, Vec2f.Zero, Vec4f.One, 16f, verts, indices, TextCoordinateMode.YDown, maxWidth: 25f);

        // 5 chars produce quads (A, B, space, A, B): 5 * 4 = 20 verts
        // verts[0..3]=A, [4..7]=B, [8..11]=space, [12..15]=A, [16..19]=B
        // First line: A at Y=2, B at Y=2
        // Second line (after wrap): A at Y=22 (cursorY=20, + (16-14)=2), B at Y=22
        Assert.Equal(2f, verts[0].Position.Y);   // first 'A' line 1
        Assert.Equal(2f, verts[4].Position.Y);   // 'B' line 1
        Assert.Equal(22f, verts[12].Position.Y);  // second 'A' line 2
        Assert.Equal(22f, verts[16].Position.Y);  // second 'B' line 2

        // X positions: second 'A' should reset to startPos.X + bearingX = 0 + 1 = 1
        Assert.Equal(1f, verts[12].Position.X);

        text.Dispose();
    }

    [Fact]
    public void BuildMesh_WordWrap_YDown_LongWord_StaysOnLine()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var text = new NativeUtf8("ABBA");
        var verts = new ArrayList<TextBuilder.TextVertex>();
        var indices = new ArrayList<uint>();

        TextBuilder.BuildMesh(text, font, Vec2f.Zero, Vec4f.One, 16f, verts, indices, TextCoordinateMode.YDown, maxWidth: 15f);

        // All 4 chars on same line since no word break possible
        Assert.Equal(2f, verts[0].Position.Y);   // A
        Assert.Equal(2f, verts[4].Position.Y);   // B
        Assert.Equal(2f, verts[8].Position.Y);   // B
        Assert.Equal(2f, verts[12].Position.Y);  // A

        text.Dispose();
    }
}
