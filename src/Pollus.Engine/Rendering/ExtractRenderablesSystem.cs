namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics.WGPU;

struct ExtractRenderablesJob<TMaterial> : IForEach<Transform2, Renderable<TMaterial>>
    where TMaterial : IMaterial
{
    public required RenderBatches Batches { get; init; }
    public required IWGPUContext GpuContext { get; init; }

    public void Execute(ref Transform2 transform, ref Renderable<TMaterial> renderable)
    {
        if (!Batches.TryGetBatch(renderable.Mesh, renderable.Material, out var batch))
        {
            batch = Batches.CreateBatch(GpuContext, 16, renderable.Mesh, renderable.Material);
        }

        if (batch.IsFull)
        {
            batch.Resize(GpuContext, batch.Capacity * 2);
        }

        batch.Write(transform.ToMat4f());
    }
}

class ExtractRenderablesSystem<TMaterial> : ECS.Core.Sys<RenderAssets, AssetServer, IWGPUContext, RenderBatches, Query<Transform2, Renderable<TMaterial>>>
    where TMaterial : IMaterial
{
    public ExtractRenderablesSystem()
        : base(new ECS.Core.SystemDescriptor($"ExtractRenderables<{typeof(TMaterial).Name}>"))
    { }

    protected override void OnTick(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, RenderBatches batches,
        Query<Transform2, Renderable<TMaterial>> query)
    {
        foreach (var material in assetServer.GetAssets<TMaterial>().AssetInfos)
        {
            renderAssets.Prepare(gpuContext, assetServer, material.Handle);
        }

        batches.Reset();
        query.ForEach(new ExtractRenderablesJob<TMaterial>
        {
            Batches = batches,
            GpuContext = gpuContext,
        });
    }
}
