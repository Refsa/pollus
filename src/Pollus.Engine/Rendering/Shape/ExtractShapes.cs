namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics.WGPU;

public class ExtractShapeDrawSystem : ExtractDrawSystem<ShapeBatches, ShapeBatch, Query<GlobalTransform, ShapeDraw>>
{
    protected override void Extract(RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext, ShapeBatches batches, Query<GlobalTransform, ShapeDraw> query)
    {
        batches.Reset();
        query.ForEach(batches, static (in batches, ref transform, ref shape) =>
        {
            var matrix = transform.Value.Translated(shape.Offset.XYZ());
            var batch = batches.GetOrCreate(new ShapeBatchKey(shape.ShapeHandle, shape.MaterialHandle));
            var sortKey = RenderingUtils.CreateSortKey2D(matrix.Col3.Z, batch.Key);
            batch.Draw(sortKey, matrix, shape.Color);
        });
    }
}