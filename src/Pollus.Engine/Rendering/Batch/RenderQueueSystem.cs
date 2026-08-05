namespace Pollus.Engine.Rendering;

using Collections;
using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;
using Silk.NET.SDL;

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
    struct BatchCursor
    {
        public required IRenderBatches Owner;
        public required IRenderBatch Batch;
        public int NextIndex;
        public int RunOrdinal;
    }

    readonly ArrayList<BatchCursor> cursors = new(64);
    readonly PriorityQueue<int, RenderQueueSortKey> mergeQueue = new(RenderQueueSortKeyComparer.Instance);

    public SubmitRenderQueueSystem() : base(new SystemDescriptor(RenderQueueSystems.SubmitRenderQueueSystem)
    {
        RunsAfter = [RenderQueueSystems.WriteRenderBuffersSystem],
        RunsBefore = [FrameGraph2DPlugin.Render],
    })
    {
    }

    protected override void OnTick(RenderQueueRegistry registry, RenderAssets renderAssets, DrawGroups2D drawGroups)
    {
        cursors.Clear();
        mergeQueue.Clear();

        int runOrdinal = 0;
        foreach (scoped ref readonly var batches in registry.Batches)
        {
            foreach (scoped ref readonly var batch in batches.Batches)
            {
                var entries = batch.SortEntries;
                if (entries.Length == 0)
                {
                    continue;
                }

                var cursorIndex = cursors.Count;

                cursors.Add(new BatchCursor
                {
                    Owner = batches,
                    Batch = batch,
                    NextIndex = 0,
                    RunOrdinal = runOrdinal,
                });

                mergeQueue.Enqueue(
                    cursorIndex,
                    new RenderQueueSortKey(
                        batch.RenderStep,
                        batch.KeyLayout.Compose(entries[0].SortKey),
                        0,
                        runOrdinal
                    )
                );

                runOrdinal++;
            }
        }

        if (cursors.Count == 0) return;
        if (cursors.Count == 1)
        {
            ref var cursor = ref cursors.Get(0);
            var entries = cursor.Batch.SortEntries;

            var draw = cursor.Owner.GetDrawCall(
                cursor.Batch.BatchID,
                0,
                entries.Length,
                renderAssets
            );

            if (!draw.IsEmpty)
            {
                drawGroups
                    .GetDrawList((RenderStep2D)cursor.Batch.RenderStep)
                    .Add(draw);
            }

            return;
        }

        IRenderBatches? currentOwner = null;
        int currentBatchID = -1;
        int currentRenderStep = -1;
        int rangeStart = 0;
        int rangeCount = 0;

        while (mergeQueue.TryDequeue(out var cursorIndex, out _))
        {
            ref var cursor = ref cursors.Get(cursorIndex);
            var entries = cursor.Batch.SortEntries;
            var sortedIndex = cursor.NextIndex;

            var isNewBatch =
                !ReferenceEquals(cursor.Owner, currentOwner) ||
                cursor.Batch.BatchID != currentBatchID;

            var isNewRenderStep =
                cursor.Batch.RenderStep != currentRenderStep;

            var isNonContiguous =
                !isNewBatch &&
                rangeStart + rangeCount != sortedIndex;

            if (isNewBatch || isNewRenderStep || isNonContiguous)
            {
                FlushRange();

                currentOwner = cursor.Owner;
                currentBatchID = cursor.Batch.BatchID;
                currentRenderStep = cursor.Batch.RenderStep;
                rangeStart = sortedIndex;
                rangeCount = 0;
            }

            rangeCount++;
            cursor.NextIndex++;

            if (cursor.NextIndex < entries.Length)
            {
                var nextEntry = entries[cursor.NextIndex];

                mergeQueue.Enqueue(
                    cursorIndex,
                    new RenderQueueSortKey(
                        cursor.Batch.RenderStep,
                        cursor.Batch.KeyLayout.Compose(nextEntry.SortKey),
                        cursor.NextIndex,
                        cursor.RunOrdinal
                    )
                );
            }
        }

        FlushRange();

        void FlushRange()
        {
            if (currentOwner is null || rangeCount == 0)
            {
                return;
            }

            var draw = currentOwner.GetDrawCall(
                currentBatchID,
                rangeStart,
                rangeCount,
                renderAssets
            );

            if (!draw.IsEmpty)
            {
                var drawList = drawGroups.GetDrawList(
                    (RenderStep2D)currentRenderStep
                );

                drawList.Add(draw);
            }
        }
    }
}

