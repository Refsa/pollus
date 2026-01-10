namespace Pollus.Engine.Rendering;

using Collections;
using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;

public class RenderQueueRegistry
{
    readonly ArrayList<IRenderBatches> batches = [];
    readonly Dictionary<RendererKey, int> lookup = [];

    public void Register(in RendererKey id, IRenderBatches batchCollection)
    {
        var index = batches.Count;
        batches.Add(batchCollection);
        lookup.Add(id, index);
    }

    public ReadOnlySpan<IRenderBatches> Batches => batches.AsSpan();

    public IRenderBatches Get(in RendererKey id) => batches[lookup[id]];
}

public static class RenderQueueSystems
{
    public static readonly string WriteRenderBuffersSystem = "Rendering::WriteRenderBuffers";
    public static readonly string SubmitRenderQueueSystem = "Rendering::SubmitRenderQueue";
}

public class WriteRenderBuffersSystem : SystemBase<RenderQueueRegistry, RenderAssets, IWGPUContext>
{
    public WriteRenderBuffersSystem() : base(new SystemDescriptor(RenderQueueSystems.WriteRenderBuffersSystem)
    {
        RunsBefore = [FrameGraph2DPlugin.Render],
    })
    {
    }

    protected override void OnTick(RenderQueueRegistry registry, RenderAssets renderAssets, IWGPUContext gpuContext)
    {
        foreach (scoped ref readonly var batches in registry.Batches)
        {
            batches.WriteBuffers(renderAssets, gpuContext);
        }
    }
}

public class SubmitRenderQueueSystem : SystemBase<RenderQueueRegistry, RenderAssets, DrawGroups2D>
{
    struct DrawEntry : IComparable<DrawEntry>
    {
        public ulong SortKey;
        public RendererKey RendererKey;
        public int BatchID;
        public int SortedIndex;
        public int RenderStep;

        public int CompareTo(DrawEntry other)
        {
            var renderStepComparison = RenderStep.CompareTo(other.RenderStep);
            if (renderStepComparison != 0) return renderStepComparison;
            var sortKeyComparison = SortKey.CompareTo(other.SortKey);
            if (sortKeyComparison != 0) return sortKeyComparison;
            return SortedIndex.CompareTo(other.SortedIndex);
        }
    }

    readonly ArrayList<DrawEntry> drawOrder = new(1024);

    public SubmitRenderQueueSystem() : base(new SystemDescriptor(RenderQueueSystems.SubmitRenderQueueSystem)
    {
        RunsAfter = [RenderQueueSystems.WriteRenderBuffersSystem],
        RunsBefore = [FrameGraph2DPlugin.Render],
    })
    {
    }

    protected override void OnTick(RenderQueueRegistry registry, RenderAssets renderAssets, DrawGroups2D drawGroups)
    {
        drawOrder.Clear();

        foreach (scoped ref readonly var batches in registry.Batches)
        {
            foreach (scoped ref readonly var batch in batches.Batches)
            {
                var entries = batch.SortEntries;
                drawOrder.EnsureCapacity(drawOrder.Count + entries.Length);
                for (int i = 0; i < entries.Length; i++)
                {
                    drawOrder.Add(new DrawEntry
                    {
                        SortKey = entries[i].SortKey,
                        RendererKey = batches.RendererKey,
                        BatchID = batch.BatchID,
                        SortedIndex = i,
                        RenderStep = batch.RenderStep,
                    });
                }
            }
        }

        if (drawOrder.Count == 0) return;

        drawOrder.AsSpan().Sort();

        RendererKey currentRendererKey = RendererKey.Null;
        int currentBatchID = -1;
        int currentRenderStep = -1;
        int start = 0;
        int count = 0;

        for (int i = 0; i < drawOrder.Count; i++)
        {
            var entry = drawOrder[i];

            var isNewBatch = entry.RendererKey != currentRendererKey || entry.BatchID != currentBatchID;
            var isNewRenderStep = entry.RenderStep != currentRenderStep;
            var isNonContiguous = !isNewBatch && (start + count) != entry.SortedIndex;

            if (isNewBatch || isNewRenderStep || isNonContiguous)
            {
                if (count > 0 && currentRendererKey != RendererKey.Null)
                {
                    var draw = registry.Get(currentRendererKey).GetDrawCall(currentBatchID, start, count, renderAssets);
                    if (!draw.IsEmpty)
                    {
                        var drawList = drawGroups.GetDrawList((RenderStep2D)currentRenderStep);
                        drawList.Add(draw);
                    }
                }

                currentRendererKey = entry.RendererKey;
                currentBatchID = entry.BatchID;
                currentRenderStep = entry.RenderStep;
                start = entry.SortedIndex;
                count = 0;
            }

            count++;
        }

        if (count > 0 && currentRendererKey != RendererKey.Null)
        {
            var draw = registry.Get(currentRendererKey).GetDrawCall(currentBatchID, start, count, renderAssets);
            if (!draw.IsEmpty)
            {
                var drawList = drawGroups.GetDrawList((RenderStep2D)currentRenderStep);
                drawList.Add(draw);
            }
        }
    }
}

