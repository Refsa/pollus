namespace Pollus.Graphics;

using Pollus.Collections;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Core.Assets;
using Pollus.Utils;

public interface IRenderBatches<TBatch>
    where TBatch : IRenderBatch
{
    ListEnumerable<TBatch> Batches { get; }
    void Reset(bool all = false);
}

public interface IGlobalRenderBatches
{
    void Prepare(int batchID, int instanceIndex);
    void UpdateBuffers(IRenderAssets renderAssets, IWGPUContext gpuContext);
    Draw GetDrawCall(int batchID, int start, int count, IRenderAssets renderAssets);
    int GetBatchCount(int batchID);
    void Reset(bool all = false);
}

public abstract class RenderBatches<TBatch, TKey> : IRenderBatches<TBatch>, IGlobalRenderBatches, IDisposable
    where TBatch : IRenderBatch
    where TKey : notnull
{
    readonly List<TBatch> batches = [];
    readonly Dictionary<int, int> batchLookup = [];
    bool isDisposed;

    public void UpdateBuffers(IRenderAssets renderAssets, IWGPUContext gpuContext)
    {
        for (int i = 0; i < batches.Count; i++)
        {
            var batch = batches[i];
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

    public ListEnumerable<TBatch> Batches => new(batches);

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);

        foreach (var batch in batches)
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
        foreach (var batch in batches)
        {
            if (!all && batch.IsStatic) continue;
            batch.Reset();
        }
    }

    public void Prepare(int batchID, int instanceIndex)
    {
        batches[batchID].AddSorted(instanceIndex);
    }

    public int GetBatchCount(int batchID)
    {
        return batches[batchID].Count;
    }

    public abstract Draw GetDrawCall(int batchID, int start, int count, IRenderAssets renderAssets);

    protected TBatch GetBatch(int index) => batches[index];
    protected abstract TBatch CreateBatch(in TKey key);
}