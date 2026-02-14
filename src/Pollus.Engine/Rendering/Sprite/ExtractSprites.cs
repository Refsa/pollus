namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;

class ExtractSpritesSystem : ExtractDrawSystem<SpriteBatches, SpriteBatch, Query<GlobalTransform, Sprite>>
{
    static void ExtractQuery(in (SpriteBatches batches, bool isStatic) userData,
        ref GlobalTransform transform, ref Sprite sprite)
    {
        var batch = userData.batches.GetOrCreate(new SpriteBatchKey(sprite.Material, userData.isStatic));
        var matrix = transform.Value.Transpose();
        var extents = sprite.Slice.Size();
        var sortKey = RenderingUtils.CreateSortKey2D(matrix.Col2.W, batch.Key);

        batch.Draw(sortKey, new SpriteBatch.InstanceData
        {
            Model0 = matrix.Col0,
            Model1 = matrix.Col1,
            Model2 = matrix.Col2,
            Slice = new Vec4f(sprite.Slice.Min.X, sprite.Slice.Min.Y, extents.X, extents.Y),
            Color = sprite.Color,
        });
    }

    protected override void Extract(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, SpriteBatches batches,
        Query<GlobalTransform, Sprite> query)
    {
        var hasStatic = query.Any<Added<StaticCalculated>>();
        batches.Reset(hasStatic);

        if (hasStatic)
        {
            query.Filtered<Added<StaticCalculated>>().ForEach((batches, true), ExtractQuery);
        }

        query.Filtered<None<StaticCalculated>>().ForEach((batches, false), ExtractQuery);
    }
}
