namespace Pollus.Graphics;

using Collections;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public interface IRenderBatches
{
    ReadOnlySpan<IRenderBatch> Batches { get; }

    int RendererID { get; set; }
    void WriteBuffers(IRenderAssets renderAssets, IWGPUContext gpuContext);
    Draw GetDrawCall(int batchID, int start, int count, IRenderAssets renderAssets);
    void Reset(bool all = false);
}

public interface IRenderBatches<TBatch> : IRenderBatches
    where TBatch : IRenderBatch
{
}

public abstract class RenderBatches<TBatch, TKey> : IRenderBatches<TBatch>, IDisposable
    where TBatch : class, IRenderBatch
    where TKey : notnull
{
    readonly ArrayList<TBatch> batches = [];
    readonly Dictionary<int, int> batchLookup = [];
    bool isDisposed;

    public int RendererID { get; set; }
    public ReadOnlySpan<IRenderBatch> Batches => batches.AsSpan();

    public void WriteBuffers(IRenderAssets renderAssets, IWGPUContext gpuContext)
    {
        foreach (var batch in Batches)
        {
            if (batch.IsEmpty || !batch.HasRequiredResources(renderAssets)) continue;

            batch.Prepare();
            if (!batch.IsDirty) continue;

            GPUBuffer? instanceBuffer;
            if (batch.InstanceBufferHandle == Handle<GPUBuffer>.Null)
            {
                instanceBuffer = batch.CreateBuffer(gpuContext);
                batch.InstanceBufferHandle = renderAssets.Add(instanceBuffer);
            }
            else
            {
                instanceBuffer = renderAssets.Get(batch.InstanceBufferHandle);
                batch.EnsureBufferCapacity(instanceBuffer);
            }

            instanceBuffer.Write(batch.InstanceDataBytes, 0);
            batch.IsDirty = false;
        }
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);

        foreach (var batch in Batches)
        {
            (batch as IDisposable)?.Dispose();
        }

        batches.Clear();
    }

    public bool TryGet(in TKey key, out TBatch batch)
    {
        if (batchLookup.TryGetValue(key.GetHashCode(), out var batchIdx))
        {
            batch = batches[batchIdx];
            return true;
        }

        batch = default!;
        return false;
    }

    public TBatch GetOrCreate(in TKey key)
    {
        var keyHash = key.GetHashCode();
        if (batchLookup.TryGetValue(keyHash, out var batchIdx)) return batches[batchIdx];

        var batch = CreateBatch(key);
        batch.BatchID = batches.Count;
        batchLookup.Add(keyHash, batches.Count);
        batches.Add(batch);
        return batch;
    }

    public int GetIndex(in TKey key)
    {
        var keyHash = key.GetHashCode();
        if (batchLookup.TryGetValue(keyHash, out var batchIdx)) return batchIdx;

        var batch = CreateBatch(key);
        batchIdx = batches.Count;
        batch.BatchID = batchIdx;
        batchLookup.Add(keyHash, batchIdx);
        batches.Add(batch);
        return batchIdx;
    }

    public void Reset(bool all = false)
    {
        foreach (var batch in Batches)
        {
            if (!all && batch.IsStatic) continue;
            batch.Reset();
        }
    }

    public abstract Draw GetDrawCall(int batchID, int start, int count, IRenderAssets renderAssets);

    protected TBatch GetBatch(int index) => batches[index];
    protected abstract TBatch CreateBatch(in TKey key);
}