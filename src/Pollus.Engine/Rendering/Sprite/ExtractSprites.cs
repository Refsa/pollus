namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;

struct ExtractSpritesJob : IForEach<Transform2, Sprite>
{
    public required SpriteBatches Batches { get; init; }
    public required IWGPUContext GpuContext { get; init; }

    public void Execute(ref Transform2 transform, ref Sprite sprite)
    {
        if (!Batches.TryGetBatch(sprite.Material, out var batch))
        {
            batch = Batches.CreateBatch(GpuContext, 16, sprite.Material);
        }

        if (batch.IsFull)
        {
            batch.Resize(GpuContext, batch.Capacity * 2);
        }

        var matrix = transform.ToMat4f().Transpose();
        batch.Write(matrix.Col0, matrix.Col1, matrix.Col2, sprite.Slice, sprite.Color);
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
        foreach (var materialHandle in assetServer.GetAssets<SpriteMaterial>().Handles)
        {
            renderAssets.Prepare(gpuContext, assetServer, materialHandle);
        }

        query.ForEach(new ExtractSpritesJob
        {
            Batches = batches,
            GpuContext = gpuContext,
        });
    }
}