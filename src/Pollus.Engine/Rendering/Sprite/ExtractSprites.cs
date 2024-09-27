namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics.WGPU;

class ExtractSpritesSystem : ExtractDrawSystem<SpriteBatches, SpriteBatch, Query<Transform2D, Sprite>>
{
    struct ExtractJob : IForEach<Transform2D, Sprite>
    {
        public required SpriteBatches Batches { get; init; }

        public void Execute(ref Transform2D transform, ref Sprite sprite)
        {
            var batch = Batches.GetOrCreate(new SpriteBatchKey(sprite.Material));
            var matrix = transform.ToMat4f().Transpose();
            batch.Write(new SpriteBatch.InstanceData
            {
                Model_0 = matrix.Col0,
                Model_1 = matrix.Col1,
                Model_2 = matrix.Col2,
                Slice = sprite.Slice,
                Color = sprite.Color,
            });
        }
    }
    
    protected override void Extract(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, SpriteBatches batches,
        Query<Transform2D, Sprite> query)
    {
        query.ForEach(new ExtractJob
        {
            Batches = batches,
        });
    }
}