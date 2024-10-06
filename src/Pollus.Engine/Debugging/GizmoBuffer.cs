namespace Pollus.Debugging;

using Pollus.Engine.Rendering;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class GizmoBuffer
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