namespace Pollus.Debugging;

using Pollus.Engine.Rendering;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;
using Pollus.Utils;

public enum GizmoType : uint
{
    None = 0,
    Line,
    LineString,
    Rect,
    Circle,
    Triangle,
    Grid,
    Text,
    Texture,
}

public enum GizmoMode : uint
{
    Outlined = 0,
    Filled = 1,
    Texture = 2,
    Font = 3,
}

[ShaderType]
public partial struct GizmoVertex
{
    public Vec2f Position;
    public Vec2f UV;
    public Color Color;
}

public class Gizmos
{
    GizmoBuffer drawBuffer = new();

    FontAsset? font;

    public bool HasContent => drawBuffer.DrawCount > 0;

    public void SetFont(FontAsset font)
    {
        this.font = font;
    }

    public void Cleanup(IRenderAssets renderAssets)
    {
        drawBuffer.Cleanup(renderAssets);
    }

    public void PrepareFrame(IWGPUContext gpuContext, RenderAssets renderAssets)
    {
        if (drawBuffer.IsSetup is false)
        {
            drawBuffer.Setup(gpuContext, renderAssets);
        }

        drawBuffer.PrepareFrame(gpuContext, renderAssets);
    }

    public void Dispatch(CommandList commandList)
    {
        drawBuffer.DrawFrame(commandList);
        drawBuffer.Clear();
    }

    public void DrawLine(Vec2f start, Vec2f end, Color color, float thickness = 1.0f, float z = 0f)
    {
        var dir = end - start;
        var normal = new Vec2f(dir.Y, -dir.X).Normalized();
        var offset = normal * (thickness * 0.5f);

        drawBuffer.AddDraw(stackalloc GizmoVertex[]
        {
            new() { Position = end - offset, UV = new Vec2f(0.0f, 0.0f), Color = color },
            new() { Position = end + offset, UV = new Vec2f(0.0f, 1.0f), Color = color },
            new() { Position = start - offset, UV = new Vec2f(1.0f, 1.0f), Color = color },
            new() { Position = start + offset, UV = new Vec2f(1.0f, 0.0f), Color = color },
        }, GizmoType.Line, GizmoMode.Filled, z);
    }

    public void DrawLineString(ReadOnlySpan<Vec2f> points, Color color, float thickness = 1.0f, float z = 0f)
    {
        if (points.Length < 2) return;

        Span<GizmoVertex> vertices = stackalloc GizmoVertex[points.Length * 4];
        for (int i = 0; i < points.Length; i++)
        {
            var p = points[i];
            Vec2f dirPrev = i > 0 ? p - points[i - 1] : points[i + 1] - p;
            Vec2f dirNext = i < points.Length - 1 ? points[i + 1] - p : p - points[i - 1];
            Vec2f normalPrev = new Vec2f(dirPrev.Y, -dirPrev.X).Normalized();
            Vec2f normalNext = new Vec2f(dirNext.Y, -dirNext.X).Normalized();

            Vec2f miter = (normalPrev + normalNext).Normalized();
            float miterLength = thickness * 0.5f / miter.Dot(normalNext);
            Vec2f offset = miter * miterLength;

            vertices[i * 4 + 0] = new() { Position = p - offset, UV = new Vec2f(0.0f, 0.0f), Color = color };
            vertices[i * 4 + 1] = new() { Position = p + offset, UV = new Vec2f(0.0f, 1.0f), Color = color };
            vertices[i * 4 + 2] = new() { Position = p - offset, UV = new Vec2f(1.0f, 1.0f), Color = color };
            vertices[i * 4 + 3] = new() { Position = p + offset, UV = new Vec2f(1.0f, 0.0f), Color = color };
        }

        drawBuffer.AddDraw(vertices, GizmoType.LineString, GizmoMode.Filled, z);
    }

    public void DrawRect(Vec2f center, Vec2f extents, float rotation, Color color, float z = 0f)
    {
        rotation = rotation.Radians();
        drawBuffer.AddDraw(stackalloc GizmoVertex[]
        {
            new() { Position = center + new Vec2f(-extents.X, -extents.Y).Rotate(rotation), UV = new Vec2f(0.0f, 0.0f), Color = color },
            new() { Position = center + new Vec2f(extents.X, -extents.Y).Rotate(rotation), UV = new Vec2f(1.0f, 0.0f), Color = color },
            new() { Position = center + new Vec2f(extents.X, extents.Y).Rotate(rotation), UV = new Vec2f(1.0f, 1.0f), Color = color },
            new() { Position = center + new Vec2f(-extents.X, extents.Y).Rotate(rotation), UV = new Vec2f(0.0f, 1.0f), Color = color },
            new() { Position = center + new Vec2f(-extents.X, -extents.Y).Rotate(rotation), UV = new Vec2f(0.0f, 0.0f), Color = color },
        }, GizmoType.Rect, GizmoMode.Outlined, z);
    }

    public void DrawRectFilled(Vec2f center, Vec2f extents, float rotation, Color color, float z = 0f)
    {
        rotation = rotation.Radians();
        drawBuffer.AddDraw(stackalloc GizmoVertex[]
        {
            new() { Position = center + new Vec2f(-extents.X, -extents.Y).Rotate(rotation), UV = new Vec2f(0.0f, 0.0f), Color = color },
            new() { Position = center + new Vec2f(extents.X, -extents.Y).Rotate(rotation), UV = new Vec2f(1.0f, 0.0f), Color = color },
            new() { Position = center + new Vec2f(-extents.X, extents.Y).Rotate(rotation), UV = new Vec2f(0.0f, 1.0f), Color = color },
            new() { Position = center + new Vec2f(extents.X, extents.Y).Rotate(rotation), UV = new Vec2f(1.0f, 1.0f), Color = color },
        }, GizmoType.Rect, GizmoMode.Filled, z);
    }

    public void DrawCircle(Vec2f center, float radius, Color color, int resolution = 32, float z = 0f)
    {
        Span<GizmoVertex> vertices = stackalloc GizmoVertex[resolution + 1];
        for (int i = 0; i < resolution; i++)
        {
            float angle = MathF.Tau * i / resolution;
            float angleNext = MathF.Tau * (i + 1) / resolution;
            vertices[i] = new() { Position = center + new Vec2f(radius, 0.0f).Rotate(angle), UV = new Vec2f(0.0f, 0.0f), Color = color };
        }

        vertices[resolution] = vertices[0];
        drawBuffer.AddDraw(vertices, GizmoType.Circle, GizmoMode.Outlined, z);
    }

    public void DrawCircleFilled(Vec2f center, float radius, Color color, int resolution = 32, float z = 0f)
    {
        Span<GizmoVertex> vertices = stackalloc GizmoVertex[resolution * 3];
        for (int i = 0; i < resolution; i++)
        {
            float angle = MathF.Tau * i / resolution;
            float angleNext = MathF.Tau * (i + 1) / resolution;
            vertices[i * 3 + 0] = new() { Position = center + new Vec2f(radius, 0.0f).Rotate(angle), UV = new Vec2f(0.0f, 0.0f), Color = color };
            vertices[i * 3 + 1] = new() { Position = center + new Vec2f(radius, 0.0f).Rotate(angleNext), UV = new Vec2f(1.0f, 0.0f), Color = color };
            vertices[i * 3 + 2] = new() { Position = center, UV = new Vec2f(0.5f, 0.5f), Color = color };
        }

        drawBuffer.AddDraw(vertices, GizmoType.Circle, GizmoMode.Filled, z);
    }

    public void DrawTriangle(Vec2f a, Vec2f b, Vec2f c, Color color, float z = 0f)
    {
        drawBuffer.AddDraw(stackalloc GizmoVertex[]
        {
            new() { Position = a, UV = new Vec2f(0.0f, 0.0f), Color = color },
            new() { Position = b, UV = new Vec2f(1.0f, 0.0f), Color = color },
            new() { Position = c, UV = new Vec2f(0.5f, 1.0f), Color = color },
        }, GizmoType.Triangle, GizmoMode.Outlined, z);
    }

    public void DrawTriangleFilled(Vec2f a, Vec2f b, Vec2f c, Color color, float z = 0f)
    {
        drawBuffer.AddDraw(stackalloc GizmoVertex[]
        {
            new() { Position = a, UV = new Vec2f(0.0f, 0.0f), Color = color },
            new() { Position = b, UV = new Vec2f(1.0f, 0.0f), Color = color },
            new() { Position = c, UV = new Vec2f(0.5f, 1.0f), Color = color },
        }, GizmoType.Triangle, GizmoMode.Filled, z);
    }

    public void DrawArrow(Vec2f start, Vec2f end, Color color, float headSize = 8f, float z = 0f)
    {
        DrawLine(start, end, color, 1f, z);
        var dir = (end - start).Normalized();
        var normal = new Vec2f(dir.Y, -dir.X).Normalized();
        DrawTriangleFilled(end, end - dir * headSize + normal * headSize * 0.5f, end - dir * headSize - normal * headSize * 0.5f, color, z);
    }

    public void DrawRay(Vec2f origin, Vec2f direction, Color color, float length, float z = 0f)
    {
        DrawArrow(origin, origin + direction * length, color, 8, z);
    }

    public void DrawGrid(Rect bounds, Color color, float cellSize = 64f, float z = 0f)
    {
        var min = bounds.Min;
        var max = bounds.Max;
        for (float x = MathF.Ceiling(min.X / cellSize) * cellSize; x < max.X; x += cellSize)
        {
            DrawLine(new Vec2f(x, min.Y), new Vec2f(x, max.Y), color, 1f, z);
        }

        for (float y = MathF.Ceiling(min.Y / cellSize) * cellSize; y < max.Y; y += cellSize)
        {
            DrawLine(new Vec2f(min.X, y), new Vec2f(max.X, y), color, 1f, z);
        }
    }

    public void DrawText(ReadOnlySpan<char> text, Vec2f position, Color color, float size = 12f, float z = 0f)
    {
        Guard.IsNotNull(font, "Gizmos::DrawText: Font is not set");

        Span<GizmoVertex> vertices = stackalloc GizmoVertex[6];
        var tier = font.GetTierForSize(size);
        foreach (scoped ref readonly var quad in TextBuilder.BuildMesh(text, tier, position, color, size))
        {
            for (int i = 0; i < 6; i++)
            {
                scoped ref readonly var vertex = ref quad.Vertices[(int)(quad.Indices[i] - quad.IndexOffset)];
                vertices[i] = new()
                {
                    Position = vertex.Position,
                    UV = vertex.UV,
                    Color = new Color(vertex.Color.X, vertex.Color.Y, vertex.Color.Z, vertex.Color.W)
                };
            }

            drawBuffer.AddDraw(vertices, GizmoType.Text, GizmoMode.Font, z, font.Atlas);
        }
    }

    public void DrawTexture(Handle<Texture2D> texture, Vec2f position, Vec2f size, Color color, float rotation = 0f, float z = 0f)
    {
        var extents = size / 2f;
        drawBuffer.AddDraw(stackalloc GizmoVertex[]
        {
            new() { Position = position + new Vec2f(-extents.X, -extents.Y).Rotate(rotation), UV = new Vec2f(0.0f, 0.0f), Color = color },
            new() { Position = position + new Vec2f(extents.X, -extents.Y).Rotate(rotation), UV = new Vec2f(1.0f, 0.0f), Color = color },
            new() { Position = position + new Vec2f(extents.X, extents.Y).Rotate(rotation), UV = new Vec2f(1.0f, 1.0f), Color = color },
            new() { Position = position + new Vec2f(-extents.X, extents.Y).Rotate(rotation), UV = new Vec2f(0.0f, 1.0f), Color = color },
            new() { Position = position + new Vec2f(-extents.X, -extents.Y).Rotate(rotation), UV = new Vec2f(0.0f, 0.0f), Color = color },
        }, GizmoType.Texture, GizmoMode.Texture, z, texture);
    }
}
