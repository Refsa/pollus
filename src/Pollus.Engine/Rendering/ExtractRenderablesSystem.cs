namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;

struct ExtractRenderablesJob<TMaterial> : IForEach<Transform2, Renderable<TMaterial>>
    where TMaterial : IMaterial
{
    public required RenderableBatches Batches { get; init; }
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

        batch.Write(transform.ToMatrix());
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
        foreach (var meshHandle in assetServer.GetAssets<MeshAsset>().Handles)
        {
            renderAssets.Prepare(gpuContext, assetServer, meshHandle);
        }
        foreach (var materialHandle in assetServer.GetAssets<TMaterial>().Handles)
        {
            renderAssets.Prepare(gpuContext, assetServer, materialHandle);
        }

        query.ForEach(new ExtractRenderablesJob<TMaterial>
        {
            Batches = batches,
            GpuContext = gpuContext,
        });
    }
}
