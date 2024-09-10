namespace Pollus.Graphics;

using Pollus.Collections;

public abstract class RenderBatches<TBatch, TKey> : IDisposable
    where TBatch : IRenderBatch
    where TKey : notnull
{
    List<TBatch> batches = new();
    Dictionary<int, int> batchLookup = new();

    public ListEnumerable<TBatch> Batches => new(batches);

    public void Dispose()
    {
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
        batchLookup.Add(keyHash, batches.Count);
        batches.Add(batch);
        return batch;
    }

    public void Reset()
    {
        foreach (var batch in batches)
        {
            batch.Reset();
        }
    }

    protected abstract TBatch CreateBatch(in TKey key);
}