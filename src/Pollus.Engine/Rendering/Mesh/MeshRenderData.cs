namespace Pollus.Engine.Rendering;

using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;

public class MeshRenderData : IRenderData
{
    public required GPUBuffer VertexBuffer { get; init; }
    public required GPUBuffer? IndexBuffer { get; init; }

    public void Dispose()
    {
        VertexBuffer.Dispose();
        IndexBuffer?.Dispose();
    }

    public static MeshRenderData Create(IWGPUContext gpuContext, MeshAsset meshAsset)
    {
        var vertexData = meshAsset.Mesh.GetVertexData([MeshAttributeType.Position, MeshAttributeType.UV0]);

        var vertexBuffer = gpuContext.CreateBuffer(BufferDescriptor.Vertex(
            meshAsset.Name,
            vertexData.SizeInBytes
        ));
        vertexData.WriteTo(vertexBuffer, 0);

        GPUBuffer? indexBuffer = null;
        if (meshAsset.Mesh.GetIndices() is IMeshIndices indices)
        {
            var indexData = indices.Indices;
            indexBuffer = gpuContext.CreateBuffer(BufferDescriptor.Index(
                meshAsset.Name,
                (ulong)indexData.Length
            ));
            indexBuffer.Write<byte>(indexData, 0);
        }

        return new MeshRenderData
        {
            VertexBuffer = vertexBuffer,
            IndexBuffer = indexBuffer
        };
    }
}
