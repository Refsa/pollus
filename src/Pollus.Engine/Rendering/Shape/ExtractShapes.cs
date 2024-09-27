namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;

public class ExtractShapeDrawSystem : ExtractDrawSystem<ShapeBatches, ShapeBatch, Query<GlobalTransform, ShapeDraw>>
{
    struct Job : IForEach<GlobalTransform, ShapeDraw>
    {
        public required ShapeBatches Batches { get; init; }

        public void Execute(ref GlobalTransform transform, ref ShapeDraw shape)
        {
            var batch = Batches.GetOrCreate(new ShapeBatchKey(shape.ShapeHandle, shape.MaterialHandle));

            batch.Write(transform.Value, shape.Color);
        }
    }

    protected override void Extract(RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext, ShapeBatches batches, Query<GlobalTransform, ShapeDraw> query)
    {
        foreach (var shape in assetServer.GetAssets<Shape>().AssetInfos)
        {
            renderAssets.Prepare(gpuContext, assetServer, shape.Handle);
        }

        query.ForEach(new Job
        {
            Batches = batches,
        });
    }
}