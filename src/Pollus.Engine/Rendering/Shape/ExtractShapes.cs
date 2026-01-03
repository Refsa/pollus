namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics.WGPU;
using Pollus.Graphics;

public class ExtractShapeDrawSystem : ExtractDrawSystem<ShapeBatches, ShapeBatch, Query<GlobalTransform, ShapeDraw>>
{
    struct Job : IForEach<GlobalTransform, ShapeDraw>
    {
        public required ShapeBatches Batches { get; init; }
        public required DrawQueue DrawQueue { get; init; }
        public required RendererKey RendererKey { get; init; }

        public void Execute(ref GlobalTransform transform, ref ShapeDraw shape)
        {
            var matrix = transform.Value.Translated(shape.Offset.XYZ());
            var batch = Batches.GetOrCreate(new ShapeBatchKey(shape.ShapeHandle, shape.MaterialHandle));
            var instanceIndex = batch.Write(matrix, shape.Color);

            var sortKey = RenderingUtils.CreateSortKey2D(matrix.Col3.Z, batch.Key);
            DrawQueue.Add(sortKey, RendererKey.Key, batch.BatchID, instanceIndex);
        }
    }

    protected override void Extract(RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext, ShapeBatches batches, Query<GlobalTransform, ShapeDraw> query, DrawQueue drawQueue)
    {
        batches.Reset();
        query.ForEach(new Job
        {
            Batches = batches,
            DrawQueue = drawQueue,
            RendererKey = RendererKey.From<ShapeBatches>(),
        });
    }
}