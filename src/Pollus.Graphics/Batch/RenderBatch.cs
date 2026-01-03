namespace Pollus.Graphics;

using System.Runtime.InteropServices;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public interface IRenderBatch
{
    int Capacity { get; }
    int Count { get; }
    bool IsEmpty { get; }
    bool IsFull { get; }
    bool IsStatic { get; }
    bool IsDirty { get; set; }
    int Key { get; }
    int BatchID { get; set; }
    public Handle<GPUBuffer> InstanceBufferHandle { get; set; }
    public Handle[] RequiredResources { get; }

    void Reset();
    GPUBuffer CreateBuffer(IWGPUContext context);
    void EnsureCapacity(GPUBuffer buffer);
    ReadOnlySpan<byte> GetBytes();
    bool CanDraw(IRenderAssets renderAssets);
    void AddSorted(int index);
}

public interface IRenderBatch<TInstanceData> : IRenderBatch
    where TInstanceData : unmanaged, IShaderType
{
    Span<TInstanceData> Data { get; }
    void Sort(Comparison<TInstanceData> comparer);
}

public abstract class RenderBatch<TInstanceData> : IRenderBatch<TInstanceData>, IDisposable
    where TInstanceData : unmanaged, IShaderType
{
    int count;
    TInstanceData[] scratch;
    TInstanceData[] drawScratch;
    int drawCount;

    public int Key { get; init; }
    public int BatchID { get; set; } = -1;
    public Handle<GPUBuffer> InstanceBufferHandle { get; set; } = Handle<GPUBuffer>.Null;
    public abstract Handle[] RequiredResources { get; }
    public Span<TInstanceData> Data => scratch.AsSpan(0, count);

    public int Count => drawCount;
    public int Capacity => scratch.Length;
    public bool IsEmpty => count == 0;
    public bool IsFull => count == scratch.Length;

    public bool IsDirty { get; set; }
    public bool IsStatic { get; init; }

    protected RenderBatch()
    {
        scratch = new TInstanceData[16];
        drawScratch = new TInstanceData[16];
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
        drawCount = 0;
        IsDirty = true;
    }

    public bool CanDraw(IRenderAssets renderAssets)
    {
        if (IsEmpty) return false;
        foreach (var resource in RequiredResources)
        {
            if (!renderAssets.Has(resource)) return false;
        }

        return true;
    }

    public int Write(in TInstanceData data)
    {
        if (count == scratch.Length) ResizeExtract(count * 2);

        scratch[count++] = data;
        IsDirty = true;
        return count - 1;
    }

    public void AddSorted(int index)
    {
        if (drawCount == drawScratch.Length) ResizeDraw(drawCount * 2);

        drawScratch[drawCount++] = scratch[index];
    }

    public void Write(ReadOnlySpan<TInstanceData> data)
    {
        if (count + data.Length > scratch.Length) ResizeExtract(count + data.Length);

        data.CopyTo(scratch.AsSpan()[count..]);
        count += data.Length;
        IsDirty = true;
    }

    public ReadOnlySpan<TInstanceData> GetData()
    {
        return drawScratch.AsSpan(0, drawCount);
    }

    public ReadOnlySpan<byte> GetBytes()
    {
        return MemoryMarshal.AsBytes(GetData());
    }

    public GPUBuffer CreateBuffer(IWGPUContext context)
    {
        return context.CreateBuffer(new()
        {
            Label = $"InstanceBuffer_{typeof(TInstanceData).Name}_{Key}",
            Size = Alignment.AlignedSize<TInstanceData>((uint)Math.Max(drawScratch.Length, scratch.Length)),
            Usage = BufferUsage.CopyDst | BufferUsage.Vertex,
        });
    }

    public void EnsureCapacity(GPUBuffer buffer)
    {
        var size = Alignment.AlignedSize<TInstanceData>((uint)drawCount);
        if (buffer.Size < size) buffer.Resize<TInstanceData>((uint)drawCount);
    }

    public void Sort(Comparison<TInstanceData> comparer)
    {
        Data.Sort(comparer);
    }

    void ResizeExtract(int capacity)
    {
        var next = new TInstanceData[capacity];
        scratch.AsSpan().CopyTo(next);
        scratch = next;
    }

    void ResizeDraw(int capacity)
    {
        var next = new TInstanceData[capacity];
        drawScratch.AsSpan().CopyTo(next);
        drawScratch = next;
    }
}