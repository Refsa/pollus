namespace Pollus.Debugging;

using System.Runtime.InteropServices.Marshalling;
using Pollus.Engine.Rendering;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;
using Pollus.Utils;

[ShaderType]
public partial struct GizmoVertex
{
    public Vec3f Position;
    public Vec2f UV;
    public Color Color;
}

public class Gizmos
{
    GizmoRenderData renderData = new();

    GizmoBuffer bufferFilled = new();
    GizmoBuffer bufferOutlined = new();

    public void PrepareFrame(IWGPUContext gpuContext, RenderAssets renderAssets)
    {
        if (bufferFilled.IsSetup is false)
        {
            var pipelineFilledHandle = renderData.SetupPipeline(gpuContext, renderAssets, true);
            bufferFilled.Setup(gpuContext, renderAssets, pipelineFilledHandle, renderData.BindGroupHandle);
        }

        if (bufferOutlined.IsSetup is false)
        {
            var pipelineOutlinedHandle = renderData.SetupPipeline(gpuContext, renderAssets, false);
            bufferOutlined.Setup(gpuContext, renderAssets, pipelineOutlinedHandle, renderData.BindGroupHandle);
        }

        bufferFilled.PrepareFrame(renderAssets);
        bufferOutlined.PrepareFrame(renderAssets);
    }

    public void Dispatch(CommandList commandList)
    {
        bufferOutlined.DrawFrame(commandList);
        bufferOutlined.Clear();

        bufferFilled.DrawFrame(commandList);
        bufferFilled.Clear();
    }

    public void DrawLine(Vec2f start, Vec2f end, Color color, float thickness = 1.0f, float z = 0f)
    {
        var dir = end - start;
        var normal = new Vec2f(dir.Y, -dir.X).Normalized();
        var offset = normal * (thickness * 0.5f);

        bufferFilled.AddDraw(stackalloc GizmoVertex[] {
            new() { Position = new Vec3f(end - offset, z), UV = new Vec2f(0.0f, 0.0f), Color = color },
            new() { Position = new Vec3f(end + offset, z), UV = new Vec2f(0.0f, 1.0f), Color = color },
            new() { Position = new Vec3f(start - offset, z), UV = new Vec2f(1.0f, 1.0f), Color = color },
            new() { Position = new Vec3f(start + offset, z), UV = new Vec2f(1.0f, 0.0f), Color = color },
        });
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

            vertices[i * 4 + 0] = new() { Position = new Vec3f(p - offset, z), UV = new Vec2f(0.0f, 0.0f), Color = color };
            vertices[i * 4 + 1] = new() { Position = new Vec3f(p + offset, z), UV = new Vec2f(0.0f, 1.0f), Color = color };
            vertices[i * 4 + 2] = new() { Position = new Vec3f(p - offset, z), UV = new Vec2f(1.0f, 1.0f), Color = color };
            vertices[i * 4 + 3] = new() { Position = new Vec3f(p + offset, z), UV = new Vec2f(1.0f, 0.0f), Color = color };
        }
        bufferFilled.AddDraw(vertices);
    }

    public void DrawRect(Vec2f center, Vec2f extents, float rotation, Color color, float z = 0f)
    {
        rotation = rotation.Radians();
        bufferOutlined.AddDraw(stackalloc GizmoVertex[] {
            new() { Position = new Vec3f(center + new Vec2f(-extents.X, -extents.Y).Rotate(rotation), z), UV = new Vec2f(0.0f, 0.0f), Color = color },
            new() { Position = new Vec3f(center + new Vec2f(extents.X, -extents.Y).Rotate(rotation), z), UV = new Vec2f(1.0f, 0.0f), Color = color },
            new() { Position = new Vec3f(center + new Vec2f(extents.X, extents.Y).Rotate(rotation), z), UV = new Vec2f(1.0f, 1.0f), Color = color },
            new() { Position = new Vec3f(center + new Vec2f(-extents.X, extents.Y).Rotate(rotation), z), UV = new Vec2f(0.0f, 1.0f), Color = color },
            new() { Position = new Vec3f(center + new Vec2f(-extents.X, -extents.Y).Rotate(rotation), z), UV = new Vec2f(0.0f, 0.0f), Color = color },
        });
    }

    public void DrawRectFilled(Vec2f center, Vec2f extents, float rotation, Color color, float z = 0f)
    {
        rotation = rotation.Radians();
        bufferFilled.AddDraw(stackalloc GizmoVertex[] {
            new() { Position = new Vec3f(center + new Vec2f(-extents.X, -extents.Y).Rotate(rotation), z), UV = new Vec2f(0.0f, 0.0f), Color = color },
            new() { Position = new Vec3f(center + new Vec2f(extents.X, -extents.Y).Rotate(rotation), z), UV = new Vec2f(1.0f, 0.0f), Color = color },
            new() { Position = new Vec3f(center + new Vec2f(-extents.X, extents.Y).Rotate(rotation), z), UV = new Vec2f(0.0f, 1.0f), Color = color },
            new() { Position = new Vec3f(center + new Vec2f(extents.X, extents.Y).Rotate(rotation), z), UV = new Vec2f(1.0f, 1.0f), Color = color },
        });
    }

    public void DrawCircle(Vec2f center, float radius, Color color, int resolution = 32, float z = 0f)
    {
        Span<GizmoVertex> vertices = stackalloc GizmoVertex[resolution + 1];
        for (int i = 0; i < resolution; i++)
        {
            float angle = MathF.Tau * i / resolution;
            float angleNext = MathF.Tau * (i + 1) / resolution;
            vertices[i] = new() { Position = new Vec3f(center + new Vec2f(radius, 0.0f).Rotate(angle), z), UV = new Vec2f(0.0f, 0.0f), Color = color };
        }
        vertices[resolution] = vertices[0];
        bufferOutlined.AddDraw(vertices);
    }

    public void DrawCircleFilled(Vec2f center, float radius, Color color, int resolution = 32, float z = 0f)
    {
        Span<GizmoVertex> vertices = stackalloc GizmoVertex[resolution * 3];
        for (int i = 0; i < resolution; i++)
        {
            float angle = MathF.Tau * i / resolution;
            float angleNext = MathF.Tau * (i + 1) / resolution;
            vertices[i * 3 + 0] = new() { Position = new Vec3f(center + new Vec2f(radius, 0.0f).Rotate(angle), z), UV = new Vec2f(0.0f, 0.0f), Color = color };
            vertices[i * 3 + 1] = new() { Position = new Vec3f(center + new Vec2f(radius, 0.0f).Rotate(angleNext), z), UV = new Vec2f(1.0f, 0.0f), Color = color };
            vertices[i * 3 + 2] = new() { Position = new Vec3f(center, z), UV = new Vec2f(0.5f, 0.5f), Color = color };
        }
        bufferFilled.AddDraw(vertices);
    }

    public void DrawTriangle(Vec2f a, Vec2f b, Vec2f c, Color color, float z = 0f)
    {
        bufferOutlined.AddDraw(stackalloc GizmoVertex[] {
            new() { Position = new Vec3f(a, z), UV = new Vec2f(0.0f, 0.0f), Color = color },
            new() { Position = new Vec3f(b, z), UV = new Vec2f(1.0f, 0.0f), Color = color },
            new() { Position = new Vec3f(c, z), UV = new Vec2f(0.5f, 1.0f), Color = color },
        });
    }

    public void DrawTriangleFilled(Vec2f a, Vec2f b, Vec2f c, Color color, float z = 0f)
    {
        bufferFilled.AddDraw(stackalloc GizmoVertex[] {
            new() { Position = new Vec3f(a, z), UV = new Vec2f(0.0f, 0.0f), Color = color },
            new() { Position = new Vec3f(b, z), UV = new Vec2f(1.0f, 0.0f), Color = color },
            new() { Position = new Vec3f(c, z), UV = new Vec2f(0.5f, 1.0f), Color = color },
        });
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
}
