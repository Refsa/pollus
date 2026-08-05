namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics.WGPU;

public class ExtractShapeDrawSystem : ExtractDrawSystem<ShapeBatches, ShapeBatch, Query<GlobalTransform, ShapeDraw>>
{
    protected override void Extract(RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext, ShapeBatches batches, Query<GlobalTransform, ShapeDraw> query)
    {
        var hasStatic = query.Any<Added<StaticCalculated>>();
        batches.Reset(hasStatic);

        if (hasStatic)
        {
            query.Filtered<All<StaticCalculated>>().ForEachChunk(new ExtractChunk(batches, true));
        }

        query.Filtered<None<StaticCalculated>>().ForEachChunk(new ExtractChunk(batches, false));
    }

    readonly struct ExtractChunk(ShapeBatches batches, bool isStatic) : IChunkForEach<GlobalTransform, ShapeDraw>
    {
        public void Execute(
            scoped in ReadOnlySpan<Entity> entities,
            in Span<GlobalTransform> transforms,
            in Span<ShapeDraw> shapes)
        {
            ShapeBatch? lastBatch = null;
            int lastShapeId = -1;
            int lastMaterialId = -1;

            for (int i = 0; i < transforms.Length; i++)
            {
                ref var shape = ref shapes[i];
                var shapeId = shape.ShapeHandle.ID;
                var materialId = shape.MaterialHandle.ID;
                var batch = shapeId == lastShapeId && materialId == lastMaterialId
                    ? lastBatch!
                    : batches.GetOrCreate(new ShapeBatchKey(shape.ShapeHandle, shape.MaterialHandle, IsStatic: isStatic));

                lastShapeId = shapeId;
                lastMaterialId = materialId;
                lastBatch = batch;

                var matrix = transforms[i].Value.Translated(shape.Offset.XYZ()).Transpose();
                var sortKey = RenderingUtils.CreateDepthSortKey2D(matrix.Col2.W);
                ref var instance = ref batch.Draw(sortKey);
                instance.Model0 = matrix.Col0;
                instance.Model1 = matrix.Col1;
                instance.Model2 = matrix.Col2;
                instance.Color = shape.Color;
            }
        }
    }
}
