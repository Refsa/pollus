namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class ShapeRenderData
{
    public required uint VertexCount { get; init; }
    public required Handle<GPUBuffer> VertexBuffer { get; init; }
}

public class ShapeRenderDataLoader : IRenderDataLoader
{
    public int TargetType => TypeLookup.ID<Shape>();

    public void Prepare(RenderAssets renderAssets, IWGPUContext gpuContext, AssetServer assetServer, Handle handle)
    {
        var shapeData = assetServer.GetAssets<Shape>().Get(handle)
            ?? throw new InvalidOperationException("Shape data not found");

        var vertexData = shapeData.GetVertexData();

        var vertexBuffer = gpuContext.CreateBuffer(BufferDescriptor.Vertex(
            shapeData.Name,
            vertexData.SizeInBytes
        ));
        vertexData.WriteTo(vertexBuffer, 0);

        renderAssets.Add(handle, new ShapeRenderData
        {
            VertexCount = vertexData.Count,
            VertexBuffer = renderAssets.Add(vertexBuffer),
        });
    }
}