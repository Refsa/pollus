namespace Pollus.Engine.Rendering;

using System.Runtime.InteropServices;
using Pollus.Assets;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class FontMeshRenderData
{
    public required Handle<GPUBuffer> VertexBuffer { get; init; }
    public uint VertexCount { get; set; }
    public uint VertexOffset { get; set; }

    public required Handle<GPUBuffer> IndexBuffer { get; init; }
    public IndexFormat IndexFormat { get; init; }
    public int IndexCount { get; set; }
    public uint IndexOffset { get; set; }
}

public class FontMeshRenderDataLoader : IRenderDataLoader
{
    public TypeID TargetType => TypeLookup.ID<TextMeshAsset>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var textMeshAsset = assetServer.GetAssets<TextMeshAsset>().Get(handle)
                            ?? throw new InvalidOperationException("Text mesh asset not found");

        if (renderAssets.TryGet<FontMeshRenderData>(handle, out var fontMeshRenderData))
        {
            var vertexBuffer = renderAssets.Get(fontMeshRenderData.VertexBuffer);
            vertexBuffer.Resize<TextBuilder.TextVertex>((uint)textMeshAsset.Vertices.Count);
            vertexBuffer.Write(textMeshAsset.Vertices.AsSpan());

            var indexBuffer = renderAssets.Get(fontMeshRenderData.IndexBuffer);
            indexBuffer.Resize((uint)textMeshAsset.Indices.Count * sizeof(uint));
            var indexData = MemoryMarshal.Cast<uint, byte>(textMeshAsset.Indices.AsSpan());
            indexBuffer.Write(indexData, 0);

            fontMeshRenderData.VertexCount = (uint)textMeshAsset.Vertices.Count;
            fontMeshRenderData.IndexCount = textMeshAsset.Indices.Count;
        }
        else
        {
            var vertexBuffer = gpuContext.CreateBuffer(BufferDescriptor.Vertex(
                textMeshAsset.Name, Alignment.AlignedSize<TextBuilder.TextVertex>((uint)textMeshAsset.Vertices.Count)
            ));
            vertexBuffer.Write(textMeshAsset.Vertices.AsSpan());

            var indexBuffer = gpuContext.CreateBuffer(BufferDescriptor.Index(textMeshAsset.Name, (ulong)textMeshAsset.Indices.Count * sizeof(uint)));
            var indexData = MemoryMarshal.Cast<uint, byte>(textMeshAsset.Indices.AsSpan());
            indexBuffer.Write(indexData, 0);

            renderAssets.Add(handle, new FontMeshRenderData
            {
                VertexBuffer = renderAssets.Add(vertexBuffer),
                VertexCount = (uint)textMeshAsset.Vertices.Count,
                VertexOffset = 0,

                IndexBuffer = renderAssets.Add(indexBuffer),
                IndexFormat = IndexFormat.Uint32,
                IndexCount = textMeshAsset.Indices.Count,
                IndexOffset = 0,
            });
        }
    }

    public void Unload(RenderAssets renderAssets, Handle handle)
    {
        var fontMesh = renderAssets.Get<FontMeshRenderData>(handle);
        renderAssets.Unload(fontMesh.VertexBuffer);
        renderAssets.Unload(fontMesh.IndexBuffer);
    }
}