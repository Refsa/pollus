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
        FontAsset font,
        Vec2f startPos,
        Vec4f color,
        float size,
        ArrayList<TextVertex> vertices,
        ArrayList<uint> indices,
        TextCoordinateMode mode = TextCoordinateMode.YUp)
    {
        var bounds = Rect.Zero;
        foreach (scoped ref readonly var quad in BuildMesh(text, font, startPos, color, size, (uint)vertices.Count, mode))
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

    public static Enumerable BuildMesh(NativeUtf8 text, FontAsset font, Vec2f startPos, Vec4f color, float size, uint indexOffset = 0, TextCoordinateMode mode = TextCoordinateMode.YUp)
    {
        return new Enumerable(text, font, startPos, color, size, indexOffset, mode);
    }

    public static Vec2f MeasureText(NativeUtf8 text, FontAsset font, float size)
    {
        var sizePow = ((uint)size).Clamp(8u, 128u).Snap(4u);
        var scale = size / sizePow;
        var glyphKey = new GlyphKey(font.Handle, sizePow, '\0');

        float cursorX = 0f;
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
                lineCount++;
                continue;
            }

            glyphKey.Character = c;
            if (!font.Glyphs.TryGetValue(glyphKey, out var glyph))
            {
                continue;
            }

            if (lineHeight == 0f) lineHeight = glyph.LineHeight * scale;
            cursorX += glyph.Advance * scale;
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
        readonly FontAsset font;
        readonly Vec2f startPos;
        readonly Vec4f color;
        readonly float size;
        readonly uint indexOffset;
        readonly TextCoordinateMode mode;

        public Enumerable(NativeUtf8 text,
            FontAsset font,
            Vec2f startPos,
            Vec4f color,
            float size,
            uint indexOffset = 0,
            TextCoordinateMode mode = TextCoordinateMode.YUp
        )
        {
            this.text = text;
            this.font = font;
            this.startPos = startPos;
            this.color = color;
            this.size = size;
            this.indexOffset = indexOffset;
            this.mode = mode;
        }

        public BuilderEnumerator GetEnumerator()
        {
            return new BuilderEnumerator(text, font, startPos, color, size, indexOffset, mode);
        }
    }

    public ref struct BuilderEnumerator
    {
        NativeUtf8.Enumerator text;
        readonly FontAsset font;
        readonly Vec4f color;
        readonly Vec2f startPos;
        readonly float size;
        readonly TextCoordinateMode mode;

        GlyphKey glyphKey;
        float cursorX;
        float cursorY;
        uint indexOffset;
        readonly float texWidth;
        readonly float texHeight;
        readonly float scale;

        CharQuad current;
        public ref readonly CharQuad Current => ref Unsafe.AsRef(ref current);

        public BuilderEnumerator(NativeUtf8 text,
            FontAsset font,
            Vec2f startPos,
            Vec4f color,
            float size,
            uint indexOffset = 0,
            TextCoordinateMode mode = TextCoordinateMode.YUp
        )
        {
            this.startPos = startPos;
            this.color = color;
            this.text = text.GetEnumerator();
            this.font = font;
            this.size = size;
            this.indexOffset = indexOffset;
            this.mode = mode;

            cursorX = startPos.X;
            cursorY = startPos.Y;
            texWidth = font.AtlasWidth;
            texHeight = font.AtlasHeight;
            var sizePow = ((uint)size).Clamp(8u, 128u).Snap(4u);
            scale = size / sizePow;

            glyphKey = new GlyphKey(font.Handle, sizePow, '\0');
            current = new CharQuad();
        }

        public bool MoveNext()
        {
            while (text.MoveNext())
            {
                var c = text.Current;
                if (c == '\n')
                {
                    cursorX = startPos.X;
                    if (mode == TextCoordinateMode.YDown)
                    {
                        // Find line height from any glyph
                        glyphKey.Character = 'A';
                        if (font.Glyphs.TryGetValue(glyphKey, out var lhGlyph))
                            cursorY += lhGlyph.LineHeight * scale;
                        else
                            cursorY += size + 2f;
                    }
                    else
                    {
                        cursorY -= size + 2f;
                    }
                    continue;
                }

                glyphKey.Character = c;
                if (!font.Glyphs.TryGetValue(glyphKey, out var glyph))
                {
                    continue;
                }

                float x = float.Round(cursorX + (glyph.BearingX * scale));
                float w = float.Round(glyph.Bounds.Width * scale);
                float h = float.Round(glyph.Bounds.Height * scale);

                var glyphOrigin = glyph.Bounds.TopLeft();
                float u0 = glyphOrigin.X / texWidth;
                float v0 = glyphOrigin.Y / texHeight;
                float u1 = (glyphOrigin.X + glyph.Bounds.Width) / texWidth;
                float v1 = (glyphOrigin.Y + glyph.Bounds.Height) / texHeight;

                if (mode == TextCoordinateMode.YDown)
                {
                    float y = float.Round(cursorY + (glyph.Ascender - glyph.BearingY) * scale);
                    current.Vertices[0] = new TextVertex { Position = new Vec2f(x, y), UV = new Vec2f(u0, v0), Color = color };
                    current.Vertices[1] = new TextVertex { Position = new Vec2f(x + w, y), UV = new Vec2f(u1, v0), Color = color };
                    current.Vertices[2] = new TextVertex { Position = new Vec2f(x, y + h), UV = new Vec2f(u0, v1), Color = color };
                    current.Vertices[3] = new TextVertex { Position = new Vec2f(x + w, y + h), UV = new Vec2f(u1, v1), Color = color };
                }
                else
                {
                    float y = float.Round(cursorY + (glyph.BearingY * scale));
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

                cursorX += glyph.Advance * scale;
                indexOffset += 4;

                return true;
            }

            return false;
        }
    }
}
