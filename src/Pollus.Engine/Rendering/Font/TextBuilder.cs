namespace Pollus.Engine.Rendering;

using System.Runtime.CompilerServices;
using Pollus.Collections;
using Pollus.Graphics;
using Pollus.Mathematics;

public enum TextCoordinateMode : byte
{
    YUp = 0,
    YDown = 1,
}

public partial class TextBuilder
{
    [ShaderType]
    public partial struct TextVertex
    {
        public Vec2f Position;
        public Vec2f UV;
        public Vec4f Color;
    }

    public ref struct Result
    {
        public Rect Bounds;
    }

    public static Result BuildMesh(
        NativeUtf8 text,
        SdfTier tier,
        Vec2f startPos,
        Vec4f color,
        float size,
        ArrayList<TextVertex> vertices,
        ArrayList<uint> indices,
        TextCoordinateMode mode = TextCoordinateMode.YUp,
        float maxWidth = 0f,
        float lineHeightOverride = 0f)
    {
        var bounds = Rect.Zero;
        foreach (scoped ref readonly var quad in BuildMesh(text, tier, startPos, color, size, (uint)vertices.Count, mode, maxWidth, lineHeightOverride))
        {
            foreach (scoped ref readonly var point in quad.Vertices) bounds.Expand(point.Position);

            vertices.AddRange(quad.Vertices);
            indices.AddRange(quad.Indices);
        }

        return new()
        {
            Bounds = bounds
        };
    }

    public static Enumerable BuildMesh(NativeUtf8 text, SdfTier tier, Vec2f startPos, Vec4f color, float size, uint indexOffset = 0, TextCoordinateMode mode = TextCoordinateMode.YUp, float maxWidth = 0f, float lineHeightOverride = 0f)
    {
        return new Enumerable(text, tier, startPos, color, size, indexOffset, mode, maxWidth, lineHeightOverride);
    }

    public static Vec2f MeasureText(NativeUtf8 text, SdfTier tier, float size, float maxWidth = 0f, float? lineHeightOverride = null)
    {
        var scale = size / tier.SdfRenderSize;
        var glyphKey = new GlyphKey(tier.FontHandle, '\0');
        var enumerator = text.GetEnumerator();

        float cursorX = 0f;
        float cursorY = 0f;
        float maxLineWidth = 0f;
        float lineHeight = lineHeightOverride ?? 0f;
        int lineCount = 0;
        bool atWordStart = true;

        while (enumerator.MoveNext())
        {
            var c = enumerator.Current;
            if (lineCount == 0) lineCount = 1;

            if (c == '\n')
            {
                maxLineWidth = float.Max(maxLineWidth, cursorX);
                cursorX = 0f;
                atWordStart = true;
                if (lineHeight == 0f)
                    lineHeight = ResolveLineHeight(tier, ref glyphKey, scale, size, lineHeightOverride);
                cursorY += lineHeight;
                cursorY = float.Round(cursorY);
                lineCount++;
                continue;
            }

            if (c == ' ')
                atWordStart = true;

            glyphKey.Character = c;
            if (!tier.Glyphs.TryGetValue(glyphKey, out var glyph))
                continue;

            if (lineHeight == 0f)
                lineHeight = lineHeightOverride ?? glyph.LineHeight * scale;

            float advance = glyph.Advance * scale;

            if (maxWidth > 0f && c != ' ' && atWordStart)
            {
                float wordEnd = MeasureWordEnd(text, enumerator.ByteIndex, tier, glyphKey, cursorX, advance, scale);
                if (wordEnd > maxWidth && cursorX > 0f)
                {
                    maxLineWidth = float.Max(maxLineWidth, cursorX);
                    cursorX = 0f;
                    cursorY += lineHeight;
                    cursorY = float.Round(cursorY);
                    lineCount++;
                }
                atWordStart = false;
            }

            cursorX = float.Round(cursorX + advance);
        }

        if (lineCount == 0) return Vec2f.Zero;
        if (lineHeight == 0f)
            lineHeight = ResolveLineHeight(tier, ref glyphKey, scale, size, lineHeightOverride);

        maxLineWidth = float.Max(maxLineWidth, cursorX);
        float sdfExtra = 2f * tier.SdfPadding * scale;
        return new Vec2f(maxLineWidth, cursorY + lineHeight + sdfExtra);
    }

    static float ResolveLineHeight(SdfTier tier, ref GlyphKey glyphKey, float scale, float size, float? lineHeightOverride)
    {
        if (lineHeightOverride.HasValue && lineHeightOverride.Value > 0f)
            return lineHeightOverride.Value;

        var saved = glyphKey.Character;
        glyphKey.Character = 'A';
        float result = tier.Glyphs.TryGetValue(glyphKey, out var glyph)
            ? glyph.LineHeight * scale
            : size + 2f;
        glyphKey.Character = saved;
        return result;
    }

    static float MeasureWordEnd(NativeUtf8 text, int fromIndex, SdfTier tier, GlyphKey glyphKey, float cursor, float firstAdvance, float scale)
    {
        cursor = float.Round(cursor + firstAdvance);
        var peek = text.GetEnumeratorFrom(fromIndex);
        while (peek.MoveNext())
        {
            var c = peek.Current;
            if (c == ' ' || c == '\n') break;

            glyphKey.Character = c;
            if (tier.Glyphs.TryGetValue(glyphKey, out var glyph))
                cursor = float.Round(cursor + glyph.Advance * scale);
        }
        return cursor;
    }

    public struct CharQuad
    {
        /// <summary>
        /// InlineArray(4) <br/>
        /// TopLeft, TopRight, BottomLeft, BottomRight
        /// </summary>
        public Vertex Vertices;

        /// <summary>
        /// InlineArray(6) <br/>
        /// Two triangles, each with 3 indices <br/>
        /// 0,1,2 - 1,3,2
        /// </summary>
        public Index Indices;

        public uint IndexOffset;

        [InlineArray(4)]
        public struct Vertex
        {
            TextVertex _first;
        }

        [InlineArray(6)]
        public struct Index
        {
            uint _first;
        }
    }

    public readonly ref struct Enumerable
    {
        readonly NativeUtf8 text;
        readonly SdfTier tier;
        readonly Vec2f startPos;
        readonly Vec4f color;
        readonly float size;
        readonly uint indexOffset;
        readonly TextCoordinateMode mode;
        readonly float maxWidth;
        readonly float lineHeightOverride;

        public Enumerable(NativeUtf8 text,
            SdfTier tier,
            Vec2f startPos,
            Vec4f color,
            float size,
            uint indexOffset = 0,
            TextCoordinateMode mode = TextCoordinateMode.YUp,
            float maxWidth = 0f,
            float lineHeightOverride = 0f
        )
        {
            this.text = text;
            this.tier = tier;
            this.startPos = startPos;
            this.color = color;
            this.size = size;
            this.indexOffset = indexOffset;
            this.mode = mode;
            this.maxWidth = maxWidth;
            this.lineHeightOverride = lineHeightOverride;
        }

        public BuilderEnumerator GetEnumerator()
        {
            return new BuilderEnumerator(text, tier, startPos, color, size, indexOffset, mode, maxWidth, lineHeightOverride);
        }
    }

    public ref struct BuilderEnumerator
    {
        NativeUtf8 text;
        NativeUtf8.Enumerator textEnumerator;
        readonly SdfTier tier;
        readonly Vec4f color;
        readonly Vec2f startPos;
        readonly float size;
        readonly TextCoordinateMode mode;
        readonly float maxWidth;
        readonly float lineHeightOverride;

        GlyphKey glyphKey;
        float cursorX;
        float cursorY;
        uint indexOffset;
        readonly float texWidth;
        readonly float texHeight;
        readonly float scale;
        bool atWordStart;

        CharQuad current;
        public ref readonly CharQuad Current => ref Unsafe.AsRef(ref current);

        public BuilderEnumerator(NativeUtf8 text,
            SdfTier tier,
            Vec2f startPos,
            Vec4f color,
            float size,
            uint indexOffset = 0,
            TextCoordinateMode mode = TextCoordinateMode.YUp,
            float maxWidth = 0f,
            float lineHeightOverride = 0f
        )
        {
            this.startPos = startPos;
            this.color = color;
            this.text = text;
            this.textEnumerator = text.GetEnumerator();
            this.tier = tier;
            this.size = size;
            this.indexOffset = indexOffset;
            this.mode = mode;
            this.maxWidth = maxWidth;
            this.lineHeightOverride = lineHeightOverride;

            cursorX = startPos.X;
            cursorY = startPos.Y;
            texWidth = tier.AtlasWidth;
            texHeight = tier.AtlasHeight;
            scale = size / tier.SdfRenderSize;

            glyphKey = new GlyphKey(tier.FontHandle, '\0');
            current = new CharQuad();
            atWordStart = true;
        }

        public bool MoveNext()
        {
            while (textEnumerator.MoveNext())
            {
                var c = textEnumerator.Current;

                if (c == '\n')
                {
                    cursorX = startPos.X;
                    atWordStart = true;
                    cursorY += AdvanceLine();
                    cursorY = float.Round(cursorY);
                    continue;
                }

                if (c == ' ')
                {
                    atWordStart = true;
                }

                glyphKey.Character = c;
                if (!tier.Glyphs.TryGetValue(glyphKey, out var glyph))
                {
                    continue;
                }

                if (maxWidth > 0f && c != ' ' && atWordStart)
                {
                    float wordEnd = MeasureWordEnd(text, textEnumerator.ByteIndex, tier, glyphKey, cursorX, glyph.Advance * scale, scale);
                    if (wordEnd > maxWidth && cursorX > startPos.X)
                    {
                        cursorX = startPos.X;
                        cursorY += AdvanceLine();
                        cursorY = float.Round(cursorY);
                    }
                    atWordStart = false;
                }

                float x = cursorX + glyph.BearingX * scale;
                float w = glyph.Bounds.Width * scale;
                float h = glyph.Bounds.Height * scale;

                var glyphOrigin = glyph.Bounds.TopLeft();
                float u0 = glyphOrigin.X / texWidth;
                float v0 = glyphOrigin.Y / texHeight;
                float u1 = (glyphOrigin.X + glyph.Bounds.Width) / texWidth;
                float v1 = (glyphOrigin.Y + glyph.Bounds.Height) / texHeight;

                if (mode == TextCoordinateMode.YDown)
                {
                    float y = cursorY + (glyph.Ascender - glyph.BearingY) * scale;
                    current.Vertices[0] = new TextVertex { Position = new Vec2f(x, y), UV = new Vec2f(u0, v0), Color = color };
                    current.Vertices[1] = new TextVertex { Position = new Vec2f(x + w, y), UV = new Vec2f(u1, v0), Color = color };
                    current.Vertices[2] = new TextVertex { Position = new Vec2f(x, y + h), UV = new Vec2f(u0, v1), Color = color };
                    current.Vertices[3] = new TextVertex { Position = new Vec2f(x + w, y + h), UV = new Vec2f(u1, v1), Color = color };
                }
                else
                {
                    float y = cursorY + glyph.BearingY * scale;
                    current.Vertices[0] = new TextVertex { Position = new Vec2f(x, y), UV = new Vec2f(u0, v0), Color = color };
                    current.Vertices[1] = new TextVertex { Position = new Vec2f(x + w, y), UV = new Vec2f(u1, v0), Color = color };
                    current.Vertices[2] = new TextVertex { Position = new Vec2f(x, y - h), UV = new Vec2f(u0, v1), Color = color };
                    current.Vertices[3] = new TextVertex { Position = new Vec2f(x + w, y - h), UV = new Vec2f(u1, v1), Color = color };
                }

                current.Indices[0] = indexOffset + 0;
                current.Indices[1] = indexOffset + 1;
                current.Indices[2] = indexOffset + 2;

                current.Indices[3] = indexOffset + 1;
                current.Indices[4] = indexOffset + 3;
                current.Indices[5] = indexOffset + 2;

                current.IndexOffset = indexOffset;

                cursorX = float.Round(cursorX + glyph.Advance * scale);
                indexOffset += 4;

                return true;
            }

            return false;
        }

        float AdvanceLine()
        {
            if (lineHeightOverride > 0f)
                return mode == TextCoordinateMode.YDown ? lineHeightOverride : -lineHeightOverride;

            if (mode == TextCoordinateMode.YDown)
            {
                var saved = glyphKey.Character;
                glyphKey.Character = 'A';
                float result = tier.Glyphs.TryGetValue(glyphKey, out var lhGlyph)
                    ? lhGlyph.LineHeight * scale
                    : size + 2f;
                glyphKey.Character = saved;
                return result;
            }

            return -(size + 2f);
        }
    }
}
