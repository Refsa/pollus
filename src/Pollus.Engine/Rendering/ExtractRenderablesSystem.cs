namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;

struct ExtractRenderablesJob<TMaterial> : IChunkForEach<Transform2, Renderable<TMaterial>>
    where TMaterial : IMaterial
{
    public required RenderableBatch Batch { get; init; }

    public void Execute(in Span<Transform2> transforms, in Span<Renderable<TMaterial>> renderables)
    {
        var count = transforms.Length;
        Span<Mat4f> chunk = Batch.GetBlock(count);
        for (int i = 0; i < count; i++)
        {
            chunk[i] = transforms[i].ToMatrix();
        }
    }
}

class ExtractRenderablesSystem<TMaterial> : ECS.Core.Sys<RenderAssets, AssetServer, IWGPUContext, RenderableBatches, Query<Transform2, Renderable<TMaterial>>>
    where TMaterial : IMaterial
{
    public ExtractRenderablesSystem()
        : base(new ECS.Core.SystemDescriptor($"ExtractRenderables<{typeof(TMaterial).Name}>"))
    { }

    protected override void OnTick(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, RenderableBatches batches,
        Query<Transform2, Renderable<TMaterial>> query)
    {
        var first = query.Single();
        var count = query.EntityCount();

        if (!batches.TryGetBatch(first.Component1.Mesh, first.Component1.Material, out var batch))
        {
            batch = batches.CreateBatch(gpuContext, count, first.Component1.Mesh, first.Component1.Material);
        }

        renderAssets.Prepare(gpuContext, assetServer, first.Component1.Mesh);
        renderAssets.Prepare(gpuContext, assetServer, first.Component1.Material);

        if (batch.Capacity < count)
        {
            batch.Resize(gpuContext, count);
        }

        query.ForEach(new ExtractRenderablesJob<TMaterial>
        {
            Batch = batch
        });
    }
}
