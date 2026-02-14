namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Assets;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;

public static class DrawSystemLabels<TBatches, TBatch>
    where TBatches : IRenderBatches<TBatch>
    where TBatch : class, IRenderBatch
{
    public static readonly string ExtractSystem = $"DrawSystem::Extract<{typeof(TBatches).Name}, {typeof(TBatch).Name}>";
    public static readonly string WriteSystem = $"DrawSystem::Write<{typeof(TBatches).Name}, {typeof(TBatch).Name}>";
    public static readonly string DrawSystem = $"DrawSystem::Draw<{typeof(TBatches).Name}, {typeof(TBatch).Name}>";
}

public abstract class ExtractDrawSystem<TBatches, TBatch, TExtractQuery> : SystemBase<RenderAssets, AssetServer, IWGPUContext, TBatches, TExtractQuery>
    where TBatches : IRenderBatches<TBatch>
    where TBatch : class, IRenderBatch
    where TExtractQuery : IQuery
{
    protected ExtractDrawSystem()
        : base(new SystemDescriptor(DrawSystemLabels<TBatches, TBatch>.ExtractSystem)
        {
            RunsAfter = [RenderingPlugin.BeginFrameSystem],
        })
    {
    }

    protected override void OnTick(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, TBatches batches,
        TExtractQuery query)
    {
        Extract(renderAssets, assetServer, gpuContext, batches, query);
    }

    protected abstract void Extract(RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext, TBatches batches, TExtractQuery query);
}