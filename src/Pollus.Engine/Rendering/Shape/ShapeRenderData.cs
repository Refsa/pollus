namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public class ShapeRenderData : IRenderData
{
    public required uint VertexCount { get; init; }
    public required GPUBuffer VertexBuffer { get; init; }

    public void Dispose()
    {
        VertexBuffer.Dispose();
    }
}

public class ShapeRenderDataLoader : IRenderDataLoader
{
    public int TargetType => AssetLookup.ID<Shape>();

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
            VertexBuffer = vertexBuffer,
        });
    }
}

public class ExtractShapesSystem : ECS.Core.Sys<RenderAssets, AssetServer, IWGPUContext, ShapeBatches, Query<Transform2, ShapeDraw>>
{
    struct Job : IForEach<Transform2, ShapeDraw>
    {
        public required ShapeBatches Batches { get; init; }
        public required IWGPUContext GpuContext { get; init; }

        public void Execute(ref Transform2 transform, ref ShapeDraw shape)
        {
            var batch = Batches.GetOrCreate(GpuContext, new ShapeBatchKey(shape.ShapeHandle, shape.MaterialHandle));

            batch.Write(transform.ToMat4f(), shape.Color);
        }
    }

    public ExtractShapesSystem()
        : base(new ECS.Core.SystemDescriptor("ExtractShapes"))
    { }

    protected override void OnTick(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, ShapeBatches batches,
        Query<Transform2, ShapeDraw> query)
    {
        foreach (var shape in assetServer.GetAssets<Shape>().AssetInfos)
        {
            renderAssets.Prepare(gpuContext, assetServer, shape.Handle);
        }
        foreach (var material in assetServer.GetAssets<ShapeMaterial>().AssetInfos)
        {
            renderAssets.Prepare(gpuContext, assetServer, material.Handle);
        }

        batches.Reset();
        query.ForEach(new Job
        {
            Batches = batches,
            GpuContext = gpuContext,
        });
    }
}