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
        float maxWidth = 0f)
    {
        var bounds = Rect.Zero;
        foreach (scoped ref readonly var quad in BuildMesh(text, tier, startPos, color, size, (uint)vertices.Count, mode, maxWidth))
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

    public static Enumerable BuildMesh(NativeUtf8 text, SdfTier tier, Vec2f startPos, Vec4f color, float size, uint indexOffset = 0, TextCoordinateMode mode = TextCoordinateMode.YUp, float maxWidth = 0f)
    {
        return new Enumerable(text, tier, startPos, color, size, indexOffset, mode, maxWidth);
    }

    public static Vec2f MeasureText(NativeUtf8 text, SdfTier tier, float size, float maxWidth = 0f)
    {
        var scale = size / tier.SdfRenderSize;
        var glyphKey = new GlyphKey(tier.FontHandle, '\0');

        float cursorX = 0f;
        float wordStartX = 0f;
        float maxLineWidth = 0f;
        float lineHeight = 0f;
        int lineCount = 0;

        foreach (var c in text)
        {
            if (lineCount == 0) lineCount = 1;

            if (c == '\n')
            {
                maxLineWidth = float.Max(maxLineWidth, cursorX);
                cursorX = 0f;
                wordStartX = 0f;
                lineCount++;
                continue;
            }

            glyphKey.Character = c;
            if (!tier.Glyphs.TryGetValue(glyphKey, out var glyph))
            {
                continue;
            }

            if (lineHeight == 0f) lineHeight = glyph.LineHeight * scale;

            float advance = glyph.Advance * scale;

            if (c == ' ')
            {
                cursorX = float.Round(cursorX + advance);
                wordStartX = cursorX;
            }
            else
            {
                if (maxWidth > 0f && cursorX + advance > maxWidth && wordStartX > 0f)
                {
                    // Wrap: current word moves to new line
                    maxLineWidth = float.Max(maxLineWidth, wordStartX);
                    cursorX = float.Round((cursorX - wordStartX) + advance);
                    wordStartX = 0f;
                    lineCount++;
                }
                else
                {
                    cursorX = float.Round(cursorX + advance);
                }
            }
        }

        if (lineCount == 0) return Vec2f.Zero;

        maxLineWidth = float.Max(maxLineWidth, cursorX);
        return new Vec2f(maxLineWidth, lineCount * lineHeight);
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

        public Enumerable(NativeUtf8 text,
            SdfTier tier,
            Vec2f startPos,
            Vec4f color,
            float size,
            uint indexOffset = 0,
            TextCoordinateMode mode = TextCoordinateMode.YUp,
            float maxWidth = 0f
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
        }

        public BuilderEnumerator GetEnumerator()
        {
            return new BuilderEnumerator(text, tier, startPos, color, size, indexOffset, mode, maxWidth);
        }
    }

    public unsafe ref struct BuilderEnumerator
    {
        readonly byte* textData;
        readonly int textLength;
        int textIndex;
        readonly SdfTier tier;
        readonly Vec4f color;
        readonly Vec2f startPos;
        readonly float size;
        readonly TextCoordinateMode mode;
        readonly float maxWidth;

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
            float maxWidth = 0f
        )
        {
            this.startPos = startPos;
            this.color = color;
            this.textData = text.Pointer;
            this.textLength = text.AsSpan().Length;
            this.textIndex = 0;
            this.tier = tier;
            this.size = size;
            this.indexOffset = indexOffset;
            this.mode = mode;
            this.maxWidth = maxWidth;

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
            while (DecodeNext(out var c))
            {
                if (c == '\n')
                {
                    cursorX = startPos.X;
                    atWordStart = true;
                    if (mode == TextCoordinateMode.YDown)
                    {
                        glyphKey.Character = 'A';
                        if (tier.Glyphs.TryGetValue(glyphKey, out var lhGlyph))
                            cursorY += lhGlyph.LineHeight * scale;
                        else
                            cursorY += size + 2f;
                    }
                    else
                    {
                        cursorY -= size + 2f;
                    }
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

                // Word wrapping: at word boundary, measure full word and wrap if needed
                if (maxWidth > 0f && c != ' ' && atWordStart)
                {
                    float wordEnd = MeasureWordEnd(cursorX, glyph.Advance * scale);
                    if (wordEnd > maxWidth && cursorX > startPos.X)
                    {
                        cursorX = startPos.X;
                        if (mode == TextCoordinateMode.YDown)
                        {
                            glyphKey.Character = 'A';
                            if (tier.Glyphs.TryGetValue(glyphKey, out var lhGlyph2))
                                cursorY += lhGlyph2.LineHeight * scale;
                            else
                                cursorY += size + 2f;
                            glyphKey.Character = c;
                        }
                        else
                        {
                            cursorY -= size + 2f;
                        }
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

        bool DecodeNext(out char c)
        {
            if (textIndex >= textLength)
            {
                c = '\0';
                return false;
            }

            var b = textData[textIndex];
            if (b == 0)
            {
                c = '\0';
                return false;
            }

            if ((b & 0x80) == 0)
            {
                c = (char)b;
                textIndex++;
                return true;
            }

            if ((b & 0xE0) == 0xC0)
            {
                c = (char)(((b & 0x1F) << 6) | (textData[textIndex + 1] & 0x3F));
                textIndex += 2;
                return true;
            }

            if ((b & 0xF0) == 0xE0)
            {
                c = (char)(((b & 0x0F) << 12) | ((textData[textIndex + 1] & 0x3F) << 6) | (textData[textIndex + 2] & 0x3F));
                textIndex += 3;
                return true;
            }

            if ((b & 0xF8) == 0xF0)
            {
                c = '?';
                textIndex += 4;
                return true;
            }

            c = '\0';
            textIndex++;
            return false;
        }

        /// Predicts the cursor position after rendering the current word,
        /// using the same float.Round accumulation as the actual rendering.
        float MeasureWordEnd(float cursor, float firstAdvance)
        {
            cursor = float.Round(cursor + firstAdvance);
            int i = textIndex;

            while (i < textLength)
            {
                var b = textData[i];
                if (b == 0) break;

                char c;
                if ((b & 0x80) == 0)
                {
                    c = (char)b;
                    i++;
                }
                else if ((b & 0xE0) == 0xC0)
                {
                    c = (char)(((b & 0x1F) << 6) | (textData[i + 1] & 0x3F));
                    i += 2;
                }
                else if ((b & 0xF0) == 0xE0)
                {
                    c = (char)(((b & 0x0F) << 12) | ((textData[i + 1] & 0x3F) << 6) | (textData[i + 2] & 0x3F));
                    i += 3;
                }
                else if ((b & 0xF8) == 0xF0)
                {
                    c = '?';
                    i += 4;
                }
                else
                {
                    break;
                }

                if (c == ' ' || c == '\n') break;

                glyphKey.Character = c;
                if (tier.Glyphs.TryGetValue(glyphKey, out var glyph))
                {
                    cursor = float.Round(cursor + glyph.Advance * scale);
                }
            }

            return cursor;
        }
    }
}
