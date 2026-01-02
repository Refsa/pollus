namespace Pollus.Engine.Rendering;

using Debugging;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public static class DrawSystemLabels<TBatches, TBatch>
    where TBatches : IRenderBatches<TBatch>
    where TBatch : class, IRenderBatch
{
    public static readonly string ExtractSystem = $"DrawSystem::Extract<{typeof(TBatches).Name}, {typeof(TBatch).Name}>";
    public static readonly string WriteSystem = $"DrawSystem::Write<{typeof(TBatches).Name}, {typeof(TBatch).Name}>";
    public static readonly string DrawSystem = $"DrawSystem::Draw<{typeof(TBatches).Name}, {typeof(TBatch).Name}>";
    public static readonly string SortSystem = $"DrawSystem::Sort<{typeof(TBatches).Name}, {typeof(TBatch).Name}>";
}

public abstract class ExtractDrawSystem<TBatches, TBatch, TExtractQuery> : SystemBase<RenderAssets, AssetServer, IWGPUContext, TBatches, TExtractQuery>
    where TBatches : IRenderBatches<TBatch>
    where TBatch : class, IRenderBatch
    where TExtractQuery : IQuery
{
    protected ExtractDrawSystem()
        : base(new SystemDescriptor(DrawSystemLabels<TBatches, TBatch>.ExtractSystem)
        {
            RunsAfter = [RenderingPlugin.BeginFrameSystem]
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

public class WriteBatchesSystem<TBatches, TBatch> : SystemBase<RenderAssets, AssetServer, IWGPUContext, TBatches>
    where TBatches : IRenderBatches<TBatch>
    where TBatch : class, IRenderBatch
{
    public WriteBatchesSystem()
        : base(new SystemDescriptor(DrawSystemLabels<TBatches, TBatch>.WriteSystem)
        {
            RunsAfter = [DrawSystemLabels<TBatches, TBatch>.ExtractSystem]
        })
    {
    }

    protected override void OnTick(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, TBatches batches)
    {
        foreach (var batch in batches.Batches)
        {
            if (batch.IsEmpty || !batch.IsDirty) continue;

            GPUBuffer? instanceBuffer;
            if (batch.InstanceBufferHandle == Handle<GPUBuffer>.Null)
            {
                instanceBuffer = batch.CreateBuffer(gpuContext);
                batch.InstanceBufferHandle = renderAssets.Add(instanceBuffer);
            }
            else
            {
                instanceBuffer = renderAssets.Get(batch.InstanceBufferHandle);
                batch.EnsureCapacity(instanceBuffer);
            }

            instanceBuffer.Write(batch.GetBytes(), 0);
            batch.IsDirty = false;
        }
    }
}

public class DrawBatchesSystem<TBatches, TBatch> : SystemBase<DrawGroups2D, RenderAssets, TBatches>
    where TBatches : IRenderBatches<TBatch>
    where TBatch : class, IRenderBatch
{
    public delegate Draw DrawBatchDelegate(RenderAssets renderAssets, TBatch batch);

    public required DrawBatchDelegate DrawExec;
    public required RenderStep2D RenderStep;

    public DrawBatchesSystem()
        : base(new SystemDescriptor(DrawSystemLabels<TBatches, TBatch>.DrawSystem)
        {
            RunsAfter = [DrawSystemLabels<TBatches, TBatch>.WriteSystem]
        })
    {
    }

    protected override void OnTick(DrawGroups2D renderSteps, RenderAssets renderAssets, TBatches batches)
    {
        var commands = renderSteps.GetDrawList(RenderStep);
        foreach (var batch in batches.Batches)
        {
            if (!batch.CanDraw(renderAssets)) continue;
            var draw = DrawExec(renderAssets, batch);
            if (draw.IsEmpty) continue;
            commands.Add(draw);
        }
    }
}

public class SortBatchesSystem<TBatches, TBatch, TInstanceData> : SystemBase<RenderAssets, AssetServer, IWGPUContext, TBatches>
    where TBatches : IRenderBatches<TBatch>
    where TBatch : class, IRenderBatch<TInstanceData>
    where TInstanceData : unmanaged, IShaderType
{
    public required Comparison<TInstanceData> Compare;

    public SortBatchesSystem()
        : base(new SystemDescriptor(DrawSystemLabels<TBatches, TBatch>.SortSystem)
        {
            RunsAfter = [DrawSystemLabels<TBatches, TBatch>.ExtractSystem],
            RunsBefore = [DrawSystemLabels<TBatches, TBatch>.WriteSystem],
        })
    {
    }

    protected override void OnTick(RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext, TBatches batches)
    {
        foreach (var batch in batches.Batches)
        {
            batch.Sort(Compare);
        }
    }
}