namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;

class ExtractSpritesSystem : ExtractDrawSystem<SpriteBatches, SpriteBatch, Query<Transform2D, Sprite>>
{
    struct ExtractJob : IForEach<Transform2D, Sprite>
    {
        public required SpriteBatches Batches { get; init; }
        public required bool IsStatic { get; init; }

        public void Execute(ref Transform2D transform, ref Sprite sprite)
        {
            var batch = Batches.GetOrCreate(new SpriteBatchKey(sprite.Material, IsStatic));
            var matrix = transform.ToMat4f_Row();
            var extents = sprite.Slice.Size();
            batch.Write(new SpriteBatch.InstanceData
            {
                Model_0 = matrix.Col0,
                Model_1 = matrix.Col1,
                Model_2 = matrix.Col2,
                Slice = new Vec4f(sprite.Slice.Min.X, sprite.Slice.Min.Y, extents.X, extents.Y),
                Color = sprite.Color,
            });
        }
    }

    protected override void Extract(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, SpriteBatches batches,
        Query<Transform2D, Sprite> query)
    {
        var hasStatic = query.Any<Added<StaticCalculated>>();
        batches.Reset(hasStatic);
        if (hasStatic)
        {
            query.ForEach<ExtractJob, Added<StaticCalculated>>(new ExtractJob
            {
                Batches = batches,
                IsStatic = true,
            });
        }

        query.ForEach<ExtractJob, None<Static>>(new ExtractJob
        {
            Batches = batches,
            IsStatic = false,
        });
    }
}