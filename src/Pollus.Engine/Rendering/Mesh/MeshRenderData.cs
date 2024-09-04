namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class MeshRenderData
{
    public required Handle<GPUBuffer> VertexBuffer { get; init; }
    public required Handle<GPUBuffer> IndexBuffer { get; init; }
    public IndexFormat IndexFormat { get; init; }
    public int IndexCount { get; init; }
}

public class MeshRenderDataLoader : IRenderDataLoader
{
    public int TargetType => TypeLookup.ID<MeshAsset>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var meshAsset = assetServer.GetAssets<MeshAsset>().Get(handle)
            ?? throw new InvalidOperationException("Mesh asset not found");

        var vertexData = meshAsset.Mesh.GetVertexData([MeshAttributeType.Position3, MeshAttributeType.UV0]);

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
            indexBuffer.Write(indexData, 0);
        }

        renderAssets.Add(handle, new MeshRenderData
        {
            VertexBuffer = renderAssets.Add(vertexBuffer),
            IndexBuffer = indexBuffer != null ? renderAssets.Add(indexBuffer) : Handle<GPUBuffer>.Null,
            IndexFormat = meshAsset.Mesh.GetIndices()?.Format ?? IndexFormat.Uint16,
            IndexCount = meshAsset.Mesh.GetIndices()?.Count ?? 0,
        });
    }
}