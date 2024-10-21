namespace Pollus.Debugging;

using Pollus.Collections;
using Pollus.Engine.Rendering;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

record struct SortKey : IComparable<SortKey>
{
    public required float SortOrder;
    public required uint DrawIndex;
    public required GizmoMode Mode;
    public required GizmoType Type;

    public int CompareTo(SortKey other)
    {
        var result = SortOrder.CompareTo(other.SortOrder);
        if (result == 0) result = Mode.CompareTo(other.Mode);
        if (result == 0) result = DrawIndex.CompareTo(other.DrawIndex);
        return result;
    }
}

public class GizmoBuffer
{
    Handle<GPUBuffer> drawBufferHandle = Handle<GPUBuffer>.Null;
    Handle<GPUBuffer> vertexBufferHandle = Handle<GPUBuffer>.Null;
    Handle<GPURenderPipeline> outlinedPipelineHandle = Handle<GPURenderPipeline>.Null;
    Handle<GPURenderPipeline> filledPipelineHandle = Handle<GPURenderPipeline>.Null;
    Handle<GPUBindGroup> bindGroupHandle = Handle<GPUBindGroup>.Null;

    int drawCount;
    int vertexCount;

    SortKey[] drawOrder = new SortKey[1024];
    IndirectBufferData[] draws = new IndirectBufferData[1024];
    GizmoVertex[] vertices = new GizmoVertex[1024];

    bool isSetup = false;

    public int VertexCount => vertexCount;
    public int DrawCount => drawCount;
    public bool IsSetup => isSetup;

    public void AddVertex(in GizmoVertex vertex)
    {
        if (vertexCount >= vertices.Length) Array.Resize(ref vertices, vertexCount * 2);
        vertices[vertexCount++] = vertex;
    }

    public void AddDraw(in ReadOnlySpan<GizmoVertex> drawVertices, GizmoType type, GizmoMode mode, float sortOrder)
    {
        if (vertexCount + drawVertices.Length > vertices.Length) Array.Resize(ref vertices, vertexCount + drawVertices.Length);
        drawVertices.CopyTo(vertices.AsSpan(vertexCount, drawVertices.Length));
        vertexCount += drawVertices.Length;

        if (drawCount >= draws.Length)
        {
            Array.Resize(ref draws, drawCount * 2);
            Array.Resize(ref drawOrder, drawCount * 2);
        }

        var index = drawCount++;

        ref var drawTarget = ref draws[index];
        drawTarget.FirstInstance = 0;
        drawTarget.InstanceCount = 1;
        drawTarget.FirstVertex = (uint)(vertexCount - drawVertices.Length);
        drawTarget.VertexCount = (uint)drawVertices.Length;

        ref var drawOrderTarget = ref drawOrder[index];
        drawOrderTarget.SortOrder = sortOrder;
        drawOrderTarget.DrawIndex = (uint)index;
        drawOrderTarget.Mode = mode;
        drawOrderTarget.Type = type;
    }

    public void Setup(IWGPUContext gpuContext, RenderAssets renderAssets,
        Handle<GPURenderPipeline> outlinedPipelineHandle,
        Handle<GPURenderPipeline> filledPipelineHandle,
        Handle<GPUBindGroup> bindGroupHandle)
    {
        if (isSetup) return;

        this.outlinedPipelineHandle = outlinedPipelineHandle;
        this.filledPipelineHandle = filledPipelineHandle;
        this.bindGroupHandle = bindGroupHandle;

        drawBufferHandle = renderAssets.Add(gpuContext.CreateBuffer(
            BufferDescriptor.Indirect("gizmo::drawBuffer", (uint)drawCount)
        ));

        vertexBufferHandle = renderAssets.Add(gpuContext.CreateBuffer(
            BufferDescriptor.Vertex<GizmoVertex>("gizmo::vertexBuffer", (uint)vertexCount)
        ));

        isSetup = true;
    }

    public void PrepareFrame(RenderAssets renderAssets)
    {
        var drawBuffer = renderAssets.Get(drawBufferHandle);
        var vertexBuffer = renderAssets.Get(vertexBufferHandle);

        drawOrder.AsSpan(0, drawCount).Sort(static (a, b) =>
        {
            var result = a.SortOrder.CompareTo(b.SortOrder);
            if (result == 0) result = a.DrawIndex.CompareTo(b.DrawIndex);
            return result;
        });

        drawBuffer.Resize<IndirectBufferData>((uint)drawCount);
        drawBuffer.Write<IndirectBufferData>(draws.AsSpan(0, drawCount));

        vertexBuffer.Resize<GizmoVertex>((uint)vertexCount);
        vertexBuffer.Write<GizmoVertex>(vertices.AsSpan(0, vertexCount));
    }

    public void DrawFrame(CommandList commandList)
    {
        var commands = RenderCommands.Builder
            .SetBindGroup(0, bindGroupHandle)
            .SetVertexBuffer(0, vertexBufferHandle, 0, Alignment.AlignedSize<GizmoVertex>((uint)vertexCount));

        GizmoMode? prevMode = null;
        for (uint i = 0; i < drawCount; i++)
        {
            ref var sortKey = ref drawOrder[i];
            var mode = sortKey.Mode;
            if (mode != prevMode)
            {
                commands.SetPipeline(mode == GizmoMode.Filled ? filledPipelineHandle : outlinedPipelineHandle);
                prevMode = mode;
            }
            commands.DrawIndirect(drawBufferHandle, sortKey.DrawIndex * IndirectBufferData.SizeOf);
        }

        commandList.Add(commands);
    }

    public void Clear()
    {
        drawCount = 0;
        vertexCount = 0;
    }
}