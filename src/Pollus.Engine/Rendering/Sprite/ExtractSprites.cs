namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics.WGPU;

struct ExtractSpritesJob : IForEach<Transform2, Sprite>
{
    public required SpriteBatches Batches { get; init; }
    public required IWGPUContext GpuContext { get; init; }

    public void Execute(ref Transform2 transform, ref Sprite sprite)
    {
        var batch = Batches.GetOrCreate(GpuContext, new SpriteBatchKey(sprite.Material));
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

class ExtractSpritesSystem : ECS.Core.Sys<RenderAssets, AssetServer, IWGPUContext, SpriteBatches, Query<Transform2, Sprite>>
{
    public ExtractSpritesSystem()
        : base(new ECS.Core.SystemDescriptor($"ExtractSprites"))
    { }

    protected override void OnTick(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, SpriteBatches batches,
        Query<Transform2, Sprite> query)
    {
        foreach (var spriteMaterial in assetServer.GetAssets<SpriteMaterial>().AssetInfos)
        {
            renderAssets.Prepare(gpuContext, assetServer, spriteMaterial.Handle);
        }

        batches.Reset();
        query.ForEach(new ExtractSpritesJob
        {
            Batches = batches,
            GpuContext = gpuContext,
        });
    }
}