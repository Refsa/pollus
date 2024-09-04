namespace Pollus.Graphics;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Graphics.Rendering;
using Pollus.Utils;

public struct Draw
{
    public const int MAX_BIND_GROUPS = 4;
    public const int MAX_VERTEX_BUFFERS = 4;

    public required Handle<GPURenderPipeline> Pipeline;
    public BindGroupArray BindGroups;
    public VertexBufferArray VertexBuffers;
    public Handle<GPUBuffer> IndexBuffer;

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
}