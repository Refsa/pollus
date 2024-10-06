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

    class GizmoBuffer
    {
        Handle<GPUBuffer> drawBufferHandle = Handle<GPUBuffer>.Null;
        Handle<GPUBuffer> vertexBufferHandle = Handle<GPUBuffer>.Null;
        Handle<GPURenderPipeline> pipelineHandle = Handle<GPURenderPipeline>.Null;
        Handle<GPUBindGroup> bindGroupHandle = Handle<GPUBindGroup>.Null;

        int drawCount;
        IndirectBufferData[] draws = new IndirectBufferData[1024];

        int vertexCount;
        GizmoVertex[] vertices = new GizmoVertex[1024];

        bool isSetup = false;

        public int VertexCount => vertexCount;
        public bool IsSetup => isSetup;

        public void AddVertex(in GizmoVertex vertex)
        {
            if (vertexCount >= vertices.Length) Array.Resize(ref vertices, vertexCount * 2);
            vertices[vertexCount++] = vertex;
        }

        public void AddDraw(in ReadOnlySpan<GizmoVertex> drawVertices)
        {
            if (vertexCount + drawVertices.Length > this.vertices.Length) Array.Resize(ref this.vertices, vertexCount + drawVertices.Length);
            drawVertices.CopyTo(this.vertices.AsSpan(vertexCount, drawVertices.Length));
            vertexCount += drawVertices.Length;
            AddDraw(new IndirectBufferData()
            {
                InstanceCount = 1,
                FirstInstance = 0,
                VertexCount = (uint)drawVertices.Length,
                FirstVertex = (uint)(vertexCount - drawVertices.Length),
            });
        }

        public void AddDraw(in IndirectBufferData draw)
        {
            draws[drawCount++] = draw;
        }

        public void Setup(IWGPUContext gpuContext, RenderAssets renderAssets, Handle<GPURenderPipeline> pipelineHandle, Handle<GPUBindGroup> bindGroupHandle)
        {
            if (isSetup) return;

            this.pipelineHandle = pipelineHandle;
            this.bindGroupHandle = bindGroupHandle;

            drawBufferHandle = renderAssets.Add(gpuContext.CreateBuffer(
                BufferDescriptor.Indirect("gizmo::drawBuffer", (uint)drawCount)
            ));

            vertexBufferHandle = renderAssets.Add(gpuContext.CreateBuffer(
                BufferDescriptor.Vertex<GizmoVertex>("gizmo::vertexBuffer", (uint)vertexCount)
            ));

            isSetup = true;
        }

        public void Prepare(IWGPUContext gpuContext, RenderAssets renderAssets)
        {
            var drawBuffer = renderAssets.Get(drawBufferHandle);
            var vertexBuffer = renderAssets.Get(vertexBufferHandle);

            drawBuffer.Resize<IndirectBufferData>((uint)drawCount);
            drawBuffer.Write<IndirectBufferData>(draws.AsSpan(0, drawCount));

            vertexBuffer.Resize<GizmoVertex>((uint)vertexCount);
            vertexBuffer.Write<GizmoVertex>(vertices.AsSpan(0, vertexCount));
        }

        public void Dispatch(CommandList commandList)
        {
            var commands = RenderCommands.Builder
                .SetPipeline(pipelineHandle)
                .SetBindGroup(0, bindGroupHandle)
                .SetVertexBuffer(0, vertexBufferHandle, 0, (uint)vertexCount);

            for (uint i = 0; i < drawCount; i++)
            {
                commands = commands.DrawIndirect(drawBufferHandle, i * IndirectBufferData.SizeOf);
            }

            commandList.Add(commands);
        }

        public void Clear()
        {
            drawCount = 0;
            vertexCount = 0;
        }
    }

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

    GizmoBuffer bufferFilled = new();
    GizmoBuffer bufferOutlined = new();

    public void Prepare(IWGPUContext gpuContext, RenderAssets renderAssets)
    {
        if (bufferFilled.IsSetup is false || bufferOutlined.IsSetup is false)
        {
            using var gizmoShader = gpuContext.CreateShaderModule(new()
            {
                Backend = ShaderBackend.WGSL,
                Label = "gizmo::shader",
                Content = GizmoShaders.GIZMO_SHADER,
            });

            using var bindGroupLayout = gpuContext.CreateBindGroupLayout(new()
            {
                Label = "gizmo::bindGroupLayout",
                Entries = [
                    BindGroupLayoutEntry.Uniform<SceneUniform>(0, ShaderStage.Vertex | ShaderStage.Fragment),
                ]
            });

            using var pipelineLayout = gpuContext.CreatePipelineLayout(new()
            {
                Label = "gizmo::pipelineLayout",
                Layouts = [bindGroupLayout],
            });

            var sceneUniformRenderData = renderAssets.Get<UniformRenderData>(new Handle<Uniform<SceneUniform>>(0));
            var sceneUniformBuffer = renderAssets.Get(sceneUniformRenderData.UniformBuffer);
            var bindGroup = renderAssets.Add(gpuContext.CreateBindGroup(new()
            {
                Label = "gizmo::bindGroup",
                Layout = bindGroupLayout,
                Entries = [
                    BindGroupEntry.BufferEntry<SceneUniform>(0, sceneUniformBuffer, 0),
                ]
            }));

            var pipelineDescriptor = BasePipelineDescriptor with
            {
                VertexState = BasePipelineDescriptor.VertexState with
                {
                    ShaderModule = gizmoShader,
                },
                FragmentState = BasePipelineDescriptor.FragmentState with
                {
                    ShaderModule = gizmoShader,
                    ColorTargets = [
                        ColorTargetState.Default with
                        {
                            Format = gpuContext.GetSurfaceFormat(),
                        }
                    ]
                },
                PrimitiveState = PrimitiveState.Default with
                {
                    FrontFace = FrontFace.Ccw,
                    CullMode = CullMode.None,
                    Topology = PrimitiveTopology.TriangleStrip,
                },
                PipelineLayout = pipelineLayout,
            };

            var filledPipeline = renderAssets.Add(gpuContext.CreateRenderPipeline(pipelineDescriptor));
            var outlinedPipeline = renderAssets.Add(gpuContext.CreateRenderPipeline(pipelineDescriptor with
            {
                Label = "gizmo::outlinedPipeline",
                PrimitiveState = PrimitiveState.Default with
                {
                    FrontFace = FrontFace.Ccw,
                    CullMode = CullMode.None,
                    Topology = PrimitiveTopology.LineStrip,
                },
            }));

            bufferFilled.Setup(gpuContext, renderAssets, filledPipeline, bindGroup);
            bufferOutlined.Setup(gpuContext, renderAssets, outlinedPipeline, bindGroup);
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
}
