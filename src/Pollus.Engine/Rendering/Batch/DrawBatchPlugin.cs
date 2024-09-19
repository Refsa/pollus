namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public abstract class ExtractDrawSystem<TBatches, TBatch, TExtractQuery> : SystemBase<RenderAssets, AssetServer, IWGPUContext, TBatches, TExtractQuery>
    where TBatches : IRenderBatches<TBatch>
    where TBatch : IRenderBatch
    where TExtractQuery : IQuery
{
    public ExtractDrawSystem()
        : base(new SystemDescriptor(nameof(ExtractSpritesSystem)))
    { }

    protected override void OnTick(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, TBatches batches,
        TExtractQuery query)
    {
        batches.Reset();
        Extract(renderAssets, assetServer, gpuContext, batches, query);
    }

    protected abstract void Extract(RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext, TBatches batches, TExtractQuery query);
}

public class WriteBatchesSystem<TBatches, TBatch> : SystemBase<RenderAssets, AssetServer, IWGPUContext, TBatches>
    where TBatches : IRenderBatches<TBatch>
    where TBatch : IRenderBatch
{
    public WriteBatchesSystem()
        : base(new SystemDescriptor(nameof(WriteBatchesSystem<TBatches, TBatch>)))
    { }

    protected override void OnTick(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, TBatches batches)
    {
        foreach (var batch in batches.Batches)
        {
            if (batch.IsEmpty) continue;

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
        }
    }
}

public class DrawBatchesSystem<TBatches, TBatch> : SystemBase<DrawGroups2D, RenderAssets, TBatches>
    where TBatches : IRenderBatches<TBatch>
    where TBatch : IRenderBatch
{
    public delegate Draw DrawBatchDelegate(RenderAssets renderAssets, TBatch batch);
    public required DrawBatchDelegate DrawExec;
    public required RenderStep2D RenderStep;

    public DrawBatchesSystem()
        : base(new SystemDescriptor(nameof(DrawBatchesSystem<TBatches, TBatch>))
            .After(nameof(WriteBatchesSystem<TBatches, TBatch>))
    )
    { }

    protected override void OnTick(DrawGroups2D renderSteps, RenderAssets renderAssets, TBatches batches)
    {
        var commands = renderSteps.GetDrawList(RenderStep);
        foreach (var batch in batches.Batches)
        {
            if (batch.IsEmpty) continue;
            var draw = DrawExec(renderAssets, batch);
            commands.Add(draw);
        }
    }
}