namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;

class ExtractSpritesSystem : ExtractDrawSystem<SpriteBatches, SpriteBatch, Query<GlobalTransform, Sprite>>
{
    struct ExtractJob : IForEach<GlobalTransform, Sprite>
    {
        public required SpriteBatches Batches { get; init; }
        public required DrawQueue DrawQueue { get; init; }
        public required bool IsStatic { get; init; }
        public required int RendererID { get; init; }

        public void Execute(ref GlobalTransform transform, ref Sprite sprite)
        {
            var batch = Batches.GetOrCreate(new SpriteBatchKey(sprite.Material, IsStatic));
            var matrix = transform.Value.Transpose();
            var extents = sprite.Slice.Size();
            var instanceIndex = batch.Write(new SpriteBatch.InstanceData
            {
                Model0 = matrix.Col0,
                Model1 = matrix.Col1,
                Model2 = matrix.Col2,
                Slice = new Vec4f(sprite.Slice.Min.X, sprite.Slice.Min.Y, extents.X, extents.Y),
                Color = sprite.Color,
            });

            var sortKey = RenderingUtils.CreateSortKey2D(matrix.Col2.W, batch.Key);
            DrawQueue.Add(sortKey, RendererID, batch.BatchID, instanceIndex);
        }
    }

    protected override void Extract(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, SpriteBatches batches,
        Query<GlobalTransform, Sprite> query, DrawQueue drawQueue)
    {
        var hasStatic = query.Any<Added<StaticCalculated>>();
        batches.Reset(hasStatic);

        var rendererKey = RendererKey.From<SpriteBatches>().Key;

        if (hasStatic)
        {
            query.ForEach<ExtractJob, Added<StaticCalculated>>(new ExtractJob
            {
                Batches = batches,
                IsStatic = true,
                DrawQueue = drawQueue,
                RendererID = rendererKey,
            });
        }

        query.ForEach<ExtractJob, None<Static>>(new ExtractJob
        {
            Batches = batches,
            IsStatic = false,
            DrawQueue = drawQueue,
            RendererID = rendererKey,
        });
    }
}
