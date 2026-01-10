namespace Pollus.Graphics;

using System.Buffers;
using System.Runtime.InteropServices;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public interface IRenderBatch
{
    int Count { get; }
    bool IsEmpty { get; }
    bool IsFull { get; }
    bool IsStatic { get; }
    bool IsDirty { get; set; }
    int Key { get; }
    int BatchID { get; set; }
    int RenderStep { get; }
    Handle<GPUBuffer> InstanceBufferHandle { get; set; }
    Handle[] RequiredResources { get; }
    Span<SortBuffer.Entry> SortEntries { get; }
    Span<byte> InstanceDataBytes { get; }

    void Reset();
    void Prepare();
    GPUBuffer CreateBuffer(IWGPUContext context);
    void EnsureBufferCapacity(GPUBuffer buffer);
    bool HasRequiredResources(IRenderAssets renderAssets);
}

public interface IRenderBatch<TInstanceData> : IRenderBatch
    where TInstanceData : unmanaged, IShaderType
{
    Span<TInstanceData> Data { get; }
    void Draw(ulong sortKey, in TInstanceData data);
}

public abstract class RenderBatch<TInstanceData> : IRenderBatch<TInstanceData>, IDisposable
    where TInstanceData : unmanaged, IShaderType
{
    int count;
    TInstanceData[] data;
    readonly SortBuffer sortBuffer;

    public int Key { get; init; }
    public int BatchID { get; set; } = -1;
    public Handle<GPUBuffer> InstanceBufferHandle { get; set; } = Handle<GPUBuffer>.Null;
    public abstract Handle[] RequiredResources { get; }
    public Span<TInstanceData> Data => data.AsSpan(0, count);
    public Span<byte> InstanceDataBytes => MemoryMarshal.AsBytes(data.AsSpan(0, sortBuffer.Count));

    public int Count => sortBuffer.Count;
    public bool IsEmpty => count == 0;
    public bool IsFull => count == data.Length;
    public Span<SortBuffer.Entry> SortEntries => sortBuffer.Entries;

    public bool IsDirty { get; set; }
    public bool IsStatic { get; init; }
    public int RenderStep { get; init; }

    protected RenderBatch()
    {
        data = new TInstanceData[16];
        sortBuffer = new SortBuffer(16);
    }

    protected RenderBatch(int key) : this()
    {
        Key = key;
    }

    public void Dispose()
    {
    }

    public void Reset()
    {
        count = 0;
        sortBuffer.Clear();
        IsDirty = true;
    }

    public void Draw(ulong sortKey, in TInstanceData instanceData)
    {
        if (count == data.Length) ResizeScratch(count * 2);

        var instanceIndex = count++;
        data[instanceIndex] = instanceData;
        sortBuffer.Add(sortKey, instanceIndex);
        IsDirty = true;
    }

    public void Prepare()
    {
        if (!IsDirty || sortBuffer.Count == 0) return;

        sortBuffer.Sort();
        var entries = sortBuffer.Entries;
        var n = entries.Length;

        const int stackAllocThreshold = 65536;
        bool[]? rentedArray = null;
        var visited = n <= stackAllocThreshold
            ? stackalloc bool[n]
            : (rentedArray = ArrayPool<bool>.Shared.Rent(n));

        try
        {
            for (int i = 0; i < n; i++)
            {
                if (visited[i] || entries[i].InstanceIndex == i) continue;

                int j = i;
                var temp = data[i];

                while (!visited[j])
                {
                    visited[j] = true;
                    int next = entries[j].InstanceIndex;
                    data[j] = next == i ? temp : data[next];
                    j = next;
                }
            }
        }
        finally
        {
            if (rentedArray != null)
            {
                ArrayPool<bool>.Shared.Return(rentedArray);
            }
        }
    }

    public bool HasRequiredResources(IRenderAssets renderAssets)
    {
        if (IsEmpty) return false;
        foreach (var resource in RequiredResources)
        {
            if (!renderAssets.Has(resource)) return false;
        }

        return true;
    }

    public GPUBuffer CreateBuffer(IWGPUContext context)
    {
        return context.CreateBuffer(new()
        {
            Label = $"InstanceBuffer_{typeof(TInstanceData).Name}_{Key}",
            Size = Alignment.AlignedSize<TInstanceData>((uint)data.Length),
            Usage = BufferUsage.CopyDst | BufferUsage.Vertex,
        });
    }

    public void EnsureBufferCapacity(GPUBuffer buffer)
    {
        var size = Alignment.AlignedSize<TInstanceData>((uint)sortBuffer.Count);
        if (buffer.Size < size) buffer.Resize<TInstanceData>((uint)sortBuffer.Count);
    }

    void ResizeScratch(int capacity)
    {
        var next = new TInstanceData[capacity];
        data.AsSpan(0, count).CopyTo(next);
        data = next;
    }
}