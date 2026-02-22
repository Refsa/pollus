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

        var chars = new (char c, float advance, float bearingX, float bearingY, int w, int h)[]
        {
            ('A', 10f, 1f, 14f, 8, 14),
            ('B', 10f, 1f, 14f, 8, 14),
            ('H', 10f, 1f, 14f, 8, 14),
            (' ', 5f, 0f, 0f, 0, 0),
        };

        foreach (var (c, advance, bearingX, bearingY, w, h) in chars)
        {
            var key = new GlyphKey(handle, c);
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

        var set = new GlyphSet
        {
            FontHandle = handle,
            SdfRenderSize = 16,
            SdfPadding = 0,
            AtlasWidth = 256,
            AtlasHeight = 256,
            Glyphs = glyphs,
        };

        return new FontAsset
        {
            Handle = handle,
            Name = "TestFont",
            Atlas = Handle<Texture2D>.Null,
            AtlasWidth = 256,
            AtlasHeight = 256,
            GlyphSets = [set],
        };
    }

    [Fact]
    public void YUp_ExistingBehavior_Preserved()
    {
        var font = CreateTestFont();
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("AB");
        var verts = new ArrayList<TextBuilder.TextVertex>();
        var indices = new ArrayList<uint>();

        TextBuilder.BuildMesh(text, set, Vec2f.Zero, Vec4f.One, 16f, verts, indices);

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
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("A");
        var verts = new ArrayList<TextBuilder.TextVertex>();
        var indices = new ArrayList<uint>();

        TextBuilder.BuildMesh(text, set, Vec2f.Zero, Vec4f.One, 16f, verts, indices, TextCoordinateMode.YDown);

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
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("A\nB");
        var verts = new ArrayList<TextBuilder.TextVertex>();
        var indices = new ArrayList<uint>();

        TextBuilder.BuildMesh(text, set, Vec2f.Zero, Vec4f.One, 16f, verts, indices, TextCoordinateMode.YDown);

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
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("AB");

        var vertsUp = new ArrayList<TextBuilder.TextVertex>();
        var indicesUp = new ArrayList<uint>();
        TextBuilder.BuildMesh(text, set, Vec2f.Zero, Vec4f.One, 16f, vertsUp, indicesUp);

        var vertsDown = new ArrayList<TextBuilder.TextVertex>();
        var indicesDown = new ArrayList<uint>();
        TextBuilder.BuildMesh(text, set, Vec2f.Zero, Vec4f.One, 16f, vertsDown, indicesDown, TextCoordinateMode.YDown);

        // X positions should be the same regardless of coordinate mode
        Assert.Equal(vertsUp[0].Position.X, vertsDown[0].Position.X);
        Assert.Equal(vertsUp[4].Position.X, vertsDown[4].Position.X);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_SingleLine_ReturnsAdvanceWidth()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("AB");

        var size = TextBuilder.MeasureText(text, set, 16f);

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
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("A ");

        var size = TextBuilder.MeasureText(text, set, 16f);

        // 'A' advance=10, ' ' advance=5, scale=1 -> width = 15
        Assert.Equal(15f, size.X);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_MultiLine_ReturnsMaxWidthAndTotalHeight()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("AB\nA");

        var size = TextBuilder.MeasureText(text, set, 16f);

        // Line 1: "AB" = 20, Line 2: "A" = 10, max = 20
        Assert.Equal(20f, size.X);
        // Two lines -> height = 2 * lineHeight * scale = 40
        Assert.Equal(40f, size.Y);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_LeadingNewline_CountsBlankFirstLine()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("\nAB");

        var size = TextBuilder.MeasureText(text, set, 16f);

        Assert.Equal(20f, size.X);
        Assert.Equal(40f, size.Y);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_EmptyText_ReturnsZero()
    {
        var font = CreateTestFont();
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("");

        var size = TextBuilder.MeasureText(text, set, 16f);

        Assert.Equal(0f, size.X);
        Assert.Equal(0f, size.Y);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_WithScale_AppliesCorrectly()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("A");

        var size = TextBuilder.MeasureText(text, set, 16f);

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
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("AB");

        var size = TextBuilder.MeasureText(text, set, 16f, maxWidth: 30f);

        // "AB" = 20, fits within 30 -> no wrap
        Assert.Equal(20f, size.X);
        Assert.Equal(20f, size.Y);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_TwoWords_SecondWraps()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("AB AB");

        var size = TextBuilder.MeasureText(text, set, 16f, maxWidth: 25f);

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
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("ABBA");

        var size = TextBuilder.MeasureText(text, set, 16f, maxWidth: 15f);

        // "ABBA" = 40, exceeds 15 but no word break possible -> stays 1 line
        Assert.Equal(40f, size.X);
        Assert.Equal(20f, size.Y);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_MultipleWordsWrap()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("A B A B");

        var size = TextBuilder.MeasureText(text, set, 16f, maxWidth: 15f);

        // "A " = 15, "B " wraps -> "B " = 15, "A " wraps -> "A " = 15, "B" wraps -> "B" = 10
        // 4 lines, maxWidth = 15, height = 80
        Assert.Equal(15f, size.X);
        Assert.Equal(80f, size.Y);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_MaxWidthZero_NoWrap()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("AB AB");

        var size = TextBuilder.MeasureText(text, set, 16f, maxWidth: 0f);

        // maxWidth=0 means no wrapping -> "AB AB" = 10+10+5+10+10 = 45, 1 line
        Assert.Equal(45f, size.X);
        Assert.Equal(20f, size.Y);

        text.Dispose();
    }

    [Fact]
    public void MeasureText_ExplicitNewline_PlusWordWrap()
    {
        var font = CreateTestFont(lineHeight: 20f);
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("AB\nAB AB");

        var size = TextBuilder.MeasureText(text, set, 16f, maxWidth: 25f);

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
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("AB AB");

        var size = TextBuilder.MeasureText(text, set, 16f, maxWidth: 25f);

        // "AB " = 25, exactly at maxWidth -> "AB" wraps to line 2
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
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("AB AB");
        var verts = new ArrayList<TextBuilder.TextVertex>();
        var indices = new ArrayList<uint>();

        TextBuilder.BuildMesh(text, set, Vec2f.Zero, Vec4f.One, 16f, verts, indices, TextCoordinateMode.YDown, maxWidth: 25f);

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
        var set = font.GlyphSets[0];
        var text = new NativeUtf8("ABBA");
        var verts = new ArrayList<TextBuilder.TextVertex>();
        var indices = new ArrayList<uint>();

        TextBuilder.BuildMesh(text, set, Vec2f.Zero, Vec4f.One, 16f, verts, indices, TextCoordinateMode.YDown, maxWidth: 15f);

        // All 4 chars on same line since no word break possible
        Assert.Equal(2f, verts[0].Position.Y);   // A
        Assert.Equal(2f, verts[4].Position.Y);   // B
        Assert.Equal(2f, verts[8].Position.Y);   // B
        Assert.Equal(2f, verts[12].Position.Y);  // A

        text.Dispose();
    }

    [Fact]
    public void GetsetForSize_SelectsSmallestSufficientset()
    {
        var handle = new Handle<FontAsset>(1);
        var glyphs = new Dictionary<GlyphKey, Glyph>();

        GlyphSet Makeset(uint sdfSize) => new GlyphSet
        {
            FontHandle = handle,
            SdfRenderSize = sdfSize,
            SdfPadding = 4,
            AtlasWidth = 256,
            AtlasHeight = 256,
            Glyphs = glyphs,
        };

        var font = new FontAsset
        {
            Handle = handle,
            Name = "TestFont",
            Atlas = Handle<Texture2D>.Null,
            AtlasWidth = 256,
            AtlasHeight = 256,
            GlyphSets = [Makeset(24), Makeset(48), Makeset(64)],
        };

        // Size <= 24 -> set 0
        Assert.Equal(24u, font.GetSetForSize(8f).SdfRenderSize);
        Assert.Equal(24u, font.GetSetForSize(24f).SdfRenderSize);

        // 24 < size <= 48 -> set 1
        Assert.Equal(48u, font.GetSetForSize(25f).SdfRenderSize);
        Assert.Equal(48u, font.GetSetForSize(48f).SdfRenderSize);

        // size > 48 -> set 2
        Assert.Equal(64u, font.GetSetForSize(49f).SdfRenderSize);
        Assert.Equal(64u, font.GetSetForSize(64f).SdfRenderSize);

        // size > all -> largest set
        Assert.Equal(64u, font.GetSetForSize(200f).SdfRenderSize);
    }

    [Fact]
    public void MeasureText_WrappingLineCount_MatchesBuildMesh()
    {
        // Use non-integer advances to stress rounding behavior
        var handle = new Handle<FontAsset>(1);
        var glyphs = new Dictionary<GlyphKey, Glyph>();
        var chars = new (char c, float advance)[]
        {
            ('L', 9.3f), ('o', 7.8f), ('r', 5.2f), ('e', 7.1f), ('m', 9.7f),
            ('i', 4.1f), ('p', 7.8f), ('s', 6.3f), ('u', 7.8f), ('d', 7.8f),
            ('l', 4.1f), ('a', 7.1f), ('t', 5.6f), ('c', 6.5f), ('n', 7.8f),
            ('g', 7.8f), ('v', 7.1f), ('.', 4.5f), (',', 4.5f),
            (' ', 4.2f),
        };
        float lineHeight = 24f;
        float ascender = 18f;

        foreach (var (ch, adv) in chars)
        {
            glyphs[new GlyphKey(handle, ch)] = new Glyph
            {
                Character = ch, Advance = adv,
                BearingX = 1f, BearingY = 14f,
                Bounds = new RectInt(0, 0, (int)adv, 16),
                Scale = 1f, LineHeight = lineHeight,
                Ascender = ascender, Descender = -6f,
            };
        }

        var set = new GlyphSet
        {
            FontHandle = handle, SdfRenderSize = 16, SdfPadding = 0,
            AtlasWidth = 256, AtlasHeight = 256, Glyphs = glyphs,
        };

        var text = new NativeUtf8("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Lorem ipsum dolor sit amet.");
        float fontSize = 16f;
        float maxWidth = 372f;

        var measured = TextBuilder.MeasureText(text, set, fontSize, maxWidth);

        var verts = new ArrayList<TextBuilder.TextVertex>();
        var indices = new ArrayList<uint>();
        TextBuilder.BuildMesh(text, set, Vec2f.Zero, Vec4f.One, fontSize, verts, indices, TextCoordinateMode.YDown, maxWidth);

        // Count rendered lines by finding distinct cursorY values
        float scale = fontSize / set.SdfRenderSize;
        var lineYs = new HashSet<float>();
        for (int i = 0; i < verts.Count; i += 4)
        {
            float y = verts[i].Position.Y - (ascender - 14f) * scale; // undo glyph offset to get cursorY
            lineYs.Add(MathF.Round(y));
        }

        int measuredLineCount = (int)(measured.Y / (lineHeight * scale));
        int renderedLineCount = lineYs.Count;

        Assert.True(measuredLineCount == renderedLineCount,
            $"MeasureText says {measuredLineCount} lines (height={measured.Y}), but BuildMesh rendered {renderedLineCount} lines. lineHeight={lineHeight * scale}");

        // Also verify the rendered glyph extent fits within the measured height
        float minY = float.MaxValue, maxY = float.MinValue;
        for (int i = 0; i < verts.Count; i++)
        {
            float y = verts[i].Position.Y;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }
        float glyphExtent = maxY - minY;

        Assert.True(glyphExtent <= measured.Y,
            $"Rendered glyph extent ({glyphExtent}) exceeds measured height ({measured.Y})");

        text.Dispose();
    }

    [Fact]
    public void MeasureText_WithSdfPadding_HeightAccommodatesVertexExtent()
    {
        var handle = new Handle<FontAsset>(1);
        var glyphs = new Dictionary<GlyphKey, Glyph>();
        float lineHeight = 28f;
        float ascender = 22f;
        float descender = -6f;
        int sdfPadding = 8;

        // BearingY includes SDF padding (matching FontAssetLoader behavior)
        var chars = new (char c, float advance, float bearingY, int boundsH)[]
        {
            ('A', 10f, ascender + sdfPadding, (int)ascender + 2 * sdfPadding),
            ('g', 8f, 14f + sdfPadding, 14 + (int)(-descender) + 2 * sdfPadding),
            (' ', 5f, 0f, 0),
        };

        foreach (var (c, advance, bearingY, boundsH) in chars)
        {
            glyphs[new GlyphKey(handle, c)] = new Glyph
            {
                Character = c, Advance = advance,
                BearingX = 1f + sdfPadding, BearingY = bearingY,
                Bounds = new RectInt(0, 0, (int)advance + 2 * sdfPadding, boundsH),
                Scale = 1f, LineHeight = lineHeight,
                Ascender = ascender, Descender = descender,
            };
        }

        var set = new GlyphSet
        {
            FontHandle = handle, SdfRenderSize = 24, SdfPadding = sdfPadding,
            AtlasWidth = 256, AtlasHeight = 256, Glyphs = glyphs,
        };

        using var text = new NativeUtf8("Ag Ag Ag Ag Ag Ag Ag Ag");
        float fontSize = 16f;
        float maxWidth = 80f;

        var measured = TextBuilder.MeasureText(text, set, fontSize, maxWidth);

        var verts = new ArrayList<TextBuilder.TextVertex>();
        var indices = new ArrayList<uint>();
        TextBuilder.BuildMesh(text, set, Vec2f.Zero, Vec4f.One, fontSize, verts, indices, TextCoordinateMode.YDown, maxWidth);

        float minY = float.MaxValue, maxY = float.MinValue;
        for (int i = 0; i < verts.Count; i++)
        {
            float y = verts[i].Position.Y;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }
        float glyphExtent = maxY - minY;

        Assert.True(glyphExtent <= measured.Y,
            $"With SdfPadding={sdfPadding}: rendered extent ({glyphExtent:F2}) exceeds measured height ({measured.Y:F2})");
    }
}
