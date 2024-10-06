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
    public Vec2f Position;
    public Vec2f UV;
    public Color Color;
}

public class Gizmos
{
    GizmoRenderData renderData = new();
    Handle<GPURenderPipeline> pipelineFilledHandle = Handle<GPURenderPipeline>.Null;
    Handle<GPURenderPipeline> pipelineOutlinedHandle = Handle<GPURenderPipeline>.Null;

    GizmoBuffer bufferFilled = new();
    GizmoBuffer bufferOutlined = new();

    public void PrepareFrame(IWGPUContext gpuContext, RenderAssets renderAssets)
    {
        if (bufferFilled.IsSetup is false)
        {
            pipelineFilledHandle = renderData.SetupPipeline(gpuContext, renderAssets, true);
            bufferFilled.Setup(gpuContext, renderAssets, pipelineFilledHandle, renderData.BindGroupHandle);
        }

        if (bufferOutlined.IsSetup is false)
        {
            pipelineOutlinedHandle = renderData.SetupPipeline(gpuContext, renderAssets, false);
            bufferOutlined.Setup(gpuContext, renderAssets, pipelineOutlinedHandle, renderData.BindGroupHandle);
        }

        bufferFilled.Prepare(gpuContext, renderAssets);
        bufferOutlined.Prepare(gpuContext, renderAssets);
    }

    public void Dispatch(CommandList commandList)
    {
        bufferFilled.Dispatch(commandList);
        bufferFilled.Clear();

        bufferOutlined.Dispatch(commandList);
        bufferOutlined.Clear();
    }

    public void DrawLine(Vec2f start, Vec2f end, Color color, float thickness = 1.0f)
    {
        var dir = end - start;
        var normal = new Vec2f(dir.Y, -dir.X).Normalized();
        var offset = normal * (thickness * 0.5f);

        bufferFilled.AddDraw(stackalloc GizmoVertex[] {
            new() { Position = end - offset, UV = new Vec2f(0.0f, 0.0f), Color = color },
            new() { Position = end + offset, UV = new Vec2f(0.0f, 1.0f), Color = color },
            new() { Position = start - offset, UV = new Vec2f(1.0f, 1.0f), Color = color },
            new() { Position = start + offset, UV = new Vec2f(1.0f, 0.0f), Color = color },
        });
    }

    public void DrawLineString(ReadOnlySpan<Vec2f> points, Color color, float thickness = 1.0f)
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
        bufferFilled.AddDraw(vertices);
    }

    public void DrawRect(Vec2f center, Vec2f extents, float rotation, Color color, bool filled)
    {
        rotation = rotation.Radians();
        if (filled)
        {
            bufferFilled.AddDraw(stackalloc GizmoVertex[] {
                new() { Position = center + new Vec2f(-extents.X, -extents.Y).Rotate(rotation), UV = new Vec2f(0.0f, 0.0f), Color = color },
                new() { Position = center + new Vec2f(extents.X, -extents.Y).Rotate(rotation), UV = new Vec2f(1.0f, 0.0f), Color = color },
                new() { Position = center + new Vec2f(-extents.X, extents.Y).Rotate(rotation), UV = new Vec2f(0.0f, 1.0f), Color = color },
                new() { Position = center + new Vec2f(extents.X, extents.Y).Rotate(rotation), UV = new Vec2f(1.0f, 1.0f), Color = color },
            });
        }
        else
        {
            bufferOutlined.AddDraw(stackalloc GizmoVertex[] {
                new() { Position = center + new Vec2f(-extents.X, -extents.Y).Rotate(rotation), UV = new Vec2f(0.0f, 0.0f), Color = color },
                new() { Position = center + new Vec2f(extents.X, -extents.Y).Rotate(rotation), UV = new Vec2f(1.0f, 0.0f), Color = color },
                new() { Position = center + new Vec2f(extents.X, extents.Y).Rotate(rotation), UV = new Vec2f(1.0f, 1.0f), Color = color },
                new() { Position = center + new Vec2f(-extents.X, extents.Y).Rotate(rotation), UV = new Vec2f(0.0f, 1.0f), Color = color },
                new() { Position = center + new Vec2f(-extents.X, -extents.Y).Rotate(rotation), UV = new Vec2f(0.0f, 0.0f), Color = color },
            });
        }
    }

    public void DrawCircle(Vec2f center, float radius, Color color, bool filled, int resolution = 32)
    {
        if (filled)
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
            bufferFilled.AddDraw(vertices);
        }
        else
        {
            Span<GizmoVertex> vertices = stackalloc GizmoVertex[resolution + 1];
            for (int i = 0; i < resolution; i++)
            {
                float angle = MathF.Tau * i / resolution;
                float angleNext = MathF.Tau * (i + 1) / resolution;
                vertices[i] = new() { Position = center + new Vec2f(radius, 0.0f).Rotate(angle), UV = new Vec2f(0.0f, 0.0f), Color = color };
            }
            vertices[resolution] = vertices[0];
            bufferOutlined.AddDraw(vertices);
        }
    }
}

class GizmoRenderData
{
    static readonly RenderPipelineDescriptor BasePipelineDescriptor = new()
    {
        Label = "gizmo::basePipeline",
        VertexState = new()
        {
            EntryPoint = "vs_main",
            Layouts = [
                VertexBufferLayout.Vertex(0, [VertexFormat.Float32x2, VertexFormat.Float32x2, VertexFormat.Float32x4])
            ]
        },
        FragmentState = new()
        {
            EntryPoint = "fs_main",
            ColorTargets = [ColorTargetState.Default],
        },
        MultisampleState = MultisampleState.Default,
        PrimitiveState = PrimitiveState.Default,
    };

    bool isRenderResourcesSetup = false;
    Handle<GPUPipelineLayout> pipelineLayoutHandle = Handle<GPUPipelineLayout>.Null;
    public Handle<GPUBindGroup> BindGroupHandle = Handle<GPUBindGroup>.Null;

    public void Setup(IWGPUContext gpuContext, RenderAssets renderAssets)
    {
        if (isRenderResourcesSetup is true) return;

        using var bindGroupLayout = gpuContext.CreateBindGroupLayout(new()
        {
            Label = "gizmo::bindGroupLayout",
            Entries = [
                BindGroupLayoutEntry.Uniform<SceneUniform>(0, ShaderStage.Vertex | ShaderStage.Fragment),
                ]
        });

        pipelineLayoutHandle = renderAssets.Add(gpuContext.CreatePipelineLayout(new()
        {
            Label = "gizmo::pipelineLayout",
            Layouts = [bindGroupLayout],
        }));

        var sceneUniformRenderData = renderAssets.Get<UniformRenderData>(new Handle<Uniform<SceneUniform>>(0));
        var sceneUniformBuffer = renderAssets.Get(sceneUniformRenderData.UniformBuffer);
        BindGroupHandle = renderAssets.Add(gpuContext.CreateBindGroup(new()
        {
            Label = "gizmo::bindGroup",
            Layout = bindGroupLayout,
            Entries = [
                BindGroupEntry.BufferEntry<SceneUniform>(0, sceneUniformBuffer, 0),
                ]
        }));

        isRenderResourcesSetup = true;
    }

    public Handle<GPURenderPipeline> SetupPipeline(IWGPUContext gpuContext, RenderAssets renderAssets, bool filled)
    {
        Setup(gpuContext, renderAssets);

        using var gizmoShader = gpuContext.CreateShaderModule(new()
        {
            Backend = ShaderBackend.WGSL,
            Label = "gizmo::shader",
            Content = GizmoShaders.GIZMO_SHADER,
        });

        var pipelineLayout = renderAssets.Get(pipelineLayoutHandle);

        var pipelineDescriptor = BasePipelineDescriptor with
        {
            VertexState = BasePipelineDescriptor.VertexState with
            {
                ShaderModule = gizmoShader,
            },
            FragmentState = BasePipelineDescriptor.FragmentState with
            {
                ShaderModule = gizmoShader,
                ColorTargets = [ColorTargetState.Default with
                {
                    Format = gpuContext.GetSurfaceFormat(),
                }]
            },
            PrimitiveState = PrimitiveState.Default with
            {
                FrontFace = FrontFace.Ccw,
                CullMode = CullMode.None,
                Topology = PrimitiveTopology.TriangleStrip,
            },
            PipelineLayout = pipelineLayout,
        };

        return renderAssets.Add(gpuContext.CreateRenderPipeline(pipelineDescriptor with
        {
            Label = $"gizmo::{(filled ? "filled" : "outlined")}Pipeline",
            PrimitiveState = filled switch
            {
                true => PrimitiveState.Default with
                {
                    FrontFace = FrontFace.Ccw,
                    CullMode = CullMode.None,
                    Topology = PrimitiveTopology.TriangleStrip,
                },
                false => PrimitiveState.Default with
                {
                    FrontFace = FrontFace.Ccw,
                    CullMode = CullMode.None,
                    Topology = PrimitiveTopology.LineStrip,
                },
            },
        }));
    }
}