namespace Pollus.Graphics;

using System.Runtime.CompilerServices;
using Pollus.Graphics.Rendering;
using Pollus.Utils;

public record struct Draw
{
    public const int MAX_BIND_GROUPS = 4;
    public const int MAX_VERTEX_BUFFERS = 4;

    [InlineArray(MAX_VERTEX_BUFFERS)]
    public struct VertexBufferArray
    {
        Handle<GPUBuffer> _first;
    }

    [InlineArray(MAX_BIND_GROUPS)]
    public struct BindGroupArray
    {
        Handle<GPUBindGroup> _first;
    }

    public required Handle<GPURenderPipeline> Pipeline;
    public BindGroupArray BindGroups;
    public VertexBufferArray VertexBuffers;

    public Handle<GPUBuffer> IndexBuffer;
    public IndexFormat IndexFormat;

    public uint IndexCount;
    public uint IndexOffset;

    public uint VertexCount;
    public uint VertexOffset;

    public uint InstanceCount = 1;
    public uint InstanceOffset;

    public Draw()
    {
        for (int i = 0; i < MAX_VERTEX_BUFFERS; i++) VertexBuffers[i] = Handle<GPUBuffer>.Null;
        for (int i = 0; i < MAX_BIND_GROUPS; i++) BindGroups[i] = Handle<GPUBindGroup>.Null;
        IndexBuffer = Handle<GPUBuffer>.Null;
    }

    public static Draw Create(Handle<GPURenderPipeline> pipeline) => new()
    {
        Pipeline = pipeline,
    };

    public void Clear()
    {
        Pipeline = Handle<GPURenderPipeline>.Null;
        IndexBuffer = Handle<GPUBuffer>.Null;
        IndexCount = 0;
        IndexOffset = 0;
        VertexCount = 0;
        VertexOffset = 0;
        InstanceCount = 1;
        InstanceOffset = 0;
        for (int i = 0; i < MAX_VERTEX_BUFFERS; i++) VertexBuffers[i] = Handle<GPUBuffer>.Null;
        for (int i = 0; i < MAX_BIND_GROUPS; i++) BindGroups[i] = Handle<GPUBindGroup>.Null;
    }

    public Draw SetPipeline(Handle<GPURenderPipeline> pipeline)
    {
        Pipeline = pipeline;
        return this;
    }

    public Draw SetBindGroup(int slot, Handle<GPUBindGroup> bindGroup)
    {
        BindGroups[slot] = bindGroup;
        return this;
    }

    public Draw SetBindGroups(ReadOnlySpan<Handle<GPUBindGroup>> bindGroups)
    {
        bindGroups.CopyTo(BindGroups);
        return this;
    }

    public Draw SetVertexBuffer(int slot, Handle<GPUBuffer> vertexBuffers)
    {
        VertexBuffers[slot] = vertexBuffers;
        return this;
    }

    public Draw SetIndexBuffer(Handle<GPUBuffer> indexBuffer, IndexFormat indexFormat, uint indexCount, uint indexOffset)
    {
        IndexBuffer = indexBuffer;
        IndexCount = indexCount;
        IndexOffset = indexOffset;
        IndexFormat = indexFormat;
        return this;
    }

    public Draw SetVertexInfo(uint vertexCount, uint vertexOffset)
    {
        VertexCount = vertexCount;
        VertexOffset = vertexOffset;
        return this;
    }

    public Draw SetInstanceInfo(uint instanceCount, uint instanceOffset)
    {
        InstanceCount = instanceCount;
        InstanceOffset = instanceOffset;
        return this;
    }
}