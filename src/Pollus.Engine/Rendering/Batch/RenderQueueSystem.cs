namespace Pollus.Engine.Rendering;

using Collections;
using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;
using Utils;

public record struct RendererKey
{
    public int Key { get; init; }

    public static RendererKey From<T>()
    {
        return new RendererKey()
        {
            Key = TypeLookup.ID<T>(),
        };
    }
}

public class RenderQueueRegistry
{
    readonly ArrayList<IRenderBatches> batches = [];
    readonly Dictionary<int, int> lookup = [];

    public void Register(int id, IRenderBatches batchCollection)
    {
        var index = batches.Count;
        batches.Add(batchCollection);
        lookup.Add(id, index);
    }

    public ReadOnlySpan<IRenderBatches> Batches => batches.AsSpan();

    public IRenderBatches Get(int id) => batches[lookup[id]];
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
        public int RendererID;
        public int BatchID;
        public int SortedIndex;

        public int CompareTo(DrawEntry other)
        {
            var sortKeyComparison = SortKey.CompareTo(other.SortKey);
            if (sortKeyComparison != 0) return sortKeyComparison;
            return SortedIndex.CompareTo(other.SortedIndex);
        }
    }

    public required RenderStep2D RenderStep;

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
                        RendererID = batches.RendererID,
                        BatchID = batch.BatchID,
                        SortedIndex = i,
                    });
                }
            }
        }

        if (drawOrder.Count == 0) return;

        drawOrder.AsSpan().Sort();

        var drawList = drawGroups.GetDrawList(RenderStep);

        int currentRendererID = -1;
        int currentBatchID = -1;
        int start = 0;
        int count = 0;

        for (int i = 0; i < drawOrder.Count; i++)
        {
            var entry = drawOrder[i];

            var isNewBatch = entry.RendererID != currentRendererID || entry.BatchID != currentBatchID;
            var isNonContiguous = !isNewBatch && (start + count) != entry.SortedIndex;

            if (isNewBatch || isNonContiguous)
            {
                if (count > 0 && currentRendererID != -1)
                {
                    var draw = registry.Get(currentRendererID).GetDrawCall(currentBatchID, start, count, renderAssets);
                    if (!draw.IsEmpty) drawList.Add(draw);
                }

                currentRendererID = entry.RendererID;
                currentBatchID = entry.BatchID;
                start = entry.SortedIndex;
                count = 0;
            }

            count++;
        }

        if (count > 0 && currentRendererID != -1)
        {
            var draw = registry.Get(currentRendererID).GetDrawCall(currentBatchID, start, count, renderAssets);
            if (!draw.IsEmpty) drawList.Add(draw);
        }
    }
}

