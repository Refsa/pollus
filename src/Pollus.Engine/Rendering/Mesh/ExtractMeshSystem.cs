namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics.WGPU;

struct ExtractRenderablesJob<TMaterial> : IForEach<Transform2, MeshDraw<TMaterial>>
    where TMaterial : IMaterial
{
    public required MeshRenderBatches Batches { get; init; }
    public required IWGPUContext GpuContext { get; init; }

    public void Execute(ref Transform2 transform, ref MeshDraw<TMaterial> renderable)
    {
        var batch = Batches.GetOrCreate(GpuContext, new MeshBatchKey(renderable.Mesh, renderable.Material));
        batch.Write(transform.ToMat4f());
    }
}

class ExtractRenderablesSystem<TMaterial> : ECS.Core.Sys<RenderAssets, AssetServer, IWGPUContext, MeshRenderBatches, Query<Transform2, MeshDraw<TMaterial>>>
    where TMaterial : IMaterial
{
    public ExtractRenderablesSystem()
        : base(new ECS.Core.SystemDescriptor($"ExtractRenderables<{typeof(TMaterial).Name}>"))
    { }

    protected override void OnTick(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, MeshRenderBatches batches,
        Query<Transform2, MeshDraw<TMaterial>> query)
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
