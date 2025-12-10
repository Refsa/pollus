using Pollus.Collections;
using Pollus.Graphics;
using Pollus.Mathematics;

namespace Pollus.Engine.Rendering;

public partial class TextBuilder
{
    [ShaderType]
    public partial struct TextVertex
    {
        public Vec3f Position;
        public Vec2f UV;
        public Vec4f Color;
    }

    public static void BuildMesh(
        NativeUtf8 text,
        FontAsset font,
        Vec2f startPos,
        Vec4f color,
        float size,
        ArrayList<TextVertex> vertices,
        ArrayList<uint> indices)
    {
        float cursorX = startPos.X;
        float cursorY = startPos.Y;
        float texWidth = font.AtlasWidth;
        float texHeight = font.AtlasHeight;
        uint sizePow = ((uint)size * 2u).ClosestPowerOfTwo().Clamp(8u, 128u);
        float scale = size / sizePow * 1.5f;

        uint indexOffset = (uint)vertices.Count;

        foreach (char c in text)
        {
            if (c == '\n')
            {
                cursorX = startPos.X;
                // cursorY -= font.LineHeight * glyphs['A'].Scale;
                cursorY -= size;
                continue;
            }

            var glyphKey = new GlyphKey(font.Handle, sizePow, c);
            if (!font.Glyphs.TryGetValue(glyphKey, out var glyph))
            {
                continue;
            }

            float x = cursorX + (glyph.BearingX * scale);
            float y = cursorY + (glyph.BearingY * scale);
            float w = glyph.Bounds.Width * scale;
            float h = glyph.Bounds.Height * scale;

            var glyphOrigin = glyph.Bounds.TopLeft();
            float u0 = glyphOrigin.X / texWidth;
            float v0 = glyphOrigin.Y / texHeight;
            float u1 = (glyphOrigin.X + glyph.Bounds.Width) / texWidth;
            float v1 = (glyphOrigin.Y + glyph.Bounds.Height) / texHeight;

            vertices.Add(new TextVertex { Position = new Vec3f(x, y, 0), UV = new Vec2f(u0, v0), Color = color });
            vertices.Add(new TextVertex { Position = new Vec3f(x + w, y, 0), UV = new Vec2f(u1, v0), Color = color });
            vertices.Add(new TextVertex { Position = new Vec3f(x, y - h, 0), UV = new Vec2f(u0, v1), Color = color });
            vertices.Add(new TextVertex { Position = new Vec3f(x + w, y - h, 0), UV = new Vec2f(u1, v1), Color = color });

            indices.Add(indexOffset + 0);
            indices.Add(indexOffset + 1);
            indices.Add(indexOffset + 2);

            indices.Add(indexOffset + 1);
            indices.Add(indexOffset + 3);
            indices.Add(indexOffset + 2);

            indexOffset += 4;
            cursorX += glyph.Advance * scale;
        }
    }
}