namespace Pollus.Engine.Rendering;

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
    readonly Dictionary<int, IGlobalRenderBatches> batches = new();

    public void Register(int id, IGlobalRenderBatches batchCollection)
    {
        batches[id] = batchCollection;
    }

    public IEnumerable<IGlobalRenderBatches> Batches => batches.Values;

    public IGlobalRenderBatches Get(int id) => batches[id];
}

public static class RenderQueueSystems
{
    public static readonly string PrepareRenderQueueSystem = "Rendering::PrepareRenderQueue";
    public static readonly string UpdateRenderBuffersSystem = "Rendering::UpdateRenderBuffers";
    public static readonly string SubmitRenderQueueSystem = "Rendering::SubmitRenderQueue";
}

public class PrepareRenderQueueSystem : SystemBase<DrawQueue, RenderQueueRegistry>
{
    public PrepareRenderQueueSystem() : base(new SystemDescriptor(RenderQueueSystems.PrepareRenderQueueSystem)
    {
        RunsBefore = [FrameGraph2DPlugin.Render],
    })
    {
    }

    protected override void OnTick(DrawQueue drawQueue, RenderQueueRegistry registry)
    {
        if (drawQueue.Count == 0) return;
        drawQueue.Sort();

        var nodes = drawQueue.Nodes;
        int currentRendererID = -1;
        int currentBatchID = -1;

        for (int i = 0; i < nodes.Length; i++)
        {
            ref var node = ref nodes[i];
            if (node.RendererID != currentRendererID || node.BatchID != currentBatchID)
            {
                currentRendererID = node.RendererID;
                currentBatchID = node.BatchID;
            }

            registry.Get(currentRendererID).Prepare(currentBatchID, node.InstanceIndex);
        }
    }
}

public class UpdateRenderBuffersSystem : SystemBase<RenderQueueRegistry, RenderAssets, IWGPUContext>
{
    public UpdateRenderBuffersSystem() : base(new SystemDescriptor(RenderQueueSystems.UpdateRenderBuffersSystem)
    {
        RunsAfter = [RenderQueueSystems.PrepareRenderQueueSystem],
    })
    {
    }

    protected override void OnTick(RenderQueueRegistry registry, RenderAssets renderAssets, IWGPUContext gpuContext)
    {
        foreach (var batches in registry.Batches)
        {
            batches.UpdateBuffers(renderAssets, gpuContext);
        }
    }
}

public class SubmitRenderQueueSystem : SystemBase<DrawQueue, RenderQueueRegistry, RenderAssets, DrawGroups2D>
{
    public required RenderStep2D RenderStep;

    readonly Dictionary<(int, int), int> batchSeenCount = new();

    public SubmitRenderQueueSystem() : base(new SystemDescriptor(RenderQueueSystems.SubmitRenderQueueSystem)
    {
        RunsAfter = [RenderQueueSystems.UpdateRenderBuffersSystem],
        RunsBefore = [FrameGraph2DPlugin.Render],
    })
    {
    }

    protected override void OnTick(DrawQueue drawQueue, RenderQueueRegistry registry, RenderAssets renderAssets, DrawGroups2D drawGroups)
    {
        if (drawQueue.Count == 0) return;

        var nodes = drawQueue.Nodes;
        var drawList = drawGroups.GetDrawList(RenderStep);

        batchSeenCount.Clear();

        int currentRendererID = -1;
        int currentBatchID = -1;
        int start = 0;
        int count = 0;

        for (int i = 0; i < nodes.Length; i++)
        {
            ref var node = ref nodes[i];

            var key = (node.RendererID, node.BatchID);
            if (!batchSeenCount.TryGetValue(key, out var seenSoFar))
            {
                seenSoFar = 0;
            }

            int drawIndex = seenSoFar;
            batchSeenCount[key] = seenSoFar + 1;

            var isNewBatch = node.RendererID != currentRendererID || node.BatchID != currentBatchID;
            var isNonContiguous = (start + count) != drawIndex;

            if (isNewBatch || isNonContiguous)
            {
                if (count > 0 && currentRendererID != -1)
                {
                    var draw = registry.Get(currentRendererID).GetDrawCall(currentBatchID, start, count, renderAssets);
                    if (!draw.IsEmpty) drawList.Add(draw);
                }

                currentRendererID = node.RendererID;
                currentBatchID = node.BatchID;
                start = drawIndex;
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
