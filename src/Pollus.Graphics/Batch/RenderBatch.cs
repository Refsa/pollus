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
    public Handle<GPUBuffer> InstanceBufferHandle { get; set; }
    public Handle[] RequiredResources { get; }

    void Reset();
    GPUBuffer CreateBuffer(IWGPUContext context);
    void EnsureCapacity(GPUBuffer buffer);
    ReadOnlySpan<byte> GetBytes();
    bool CanDraw(IRenderAssets renderAssets);
}

public abstract class RenderBatch<TInstanceData> : IRenderBatch, IDisposable
    where TInstanceData : unmanaged, IShaderType
{
    int count;
    TInstanceData[] scratch;

    public int Key { get; init; }
    public Handle<GPUBuffer> InstanceBufferHandle { get; set; } = Handle<GPUBuffer>.Null;
    public abstract Handle[] RequiredResources { get; }

    public int Count => count;
    public int Capacity => scratch.Length;
    public bool IsEmpty => count == 0;
    public bool IsFull => count == scratch.Length;

    public bool IsDirty { get; set; }
    public bool IsStatic { get; init; }

    protected RenderBatch()
    {
        scratch = new TInstanceData[16];
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

    public void Write(in TInstanceData data)
    {
        if (count == scratch.Length) Resize(count * 2);

        scratch[count++] = data;
        IsDirty = true;
    }

    public void Write(ReadOnlySpan<TInstanceData> data)
    {
        if (count + data.Length > scratch.Length) Resize(count + data.Length);

        data.CopyTo(scratch.AsSpan()[count..]);
        count += data.Length;
        IsDirty = true;
    }

    public ReadOnlySpan<TInstanceData> GetData()
    {
        return scratch.AsSpan(0, count);
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
            Size = Alignment.AlignedSize<TInstanceData>((uint)scratch.Length),
            Usage = BufferUsage.CopyDst | BufferUsage.Vertex,
        });
    }

    public void EnsureCapacity(GPUBuffer buffer)
    {
        var size = Alignment.AlignedSize<TInstanceData>((uint)count);
        if (buffer.Size < size) buffer.Resize<TInstanceData>((uint)count);
    }

    void Resize(int capacity)
    {
        var next = new TInstanceData[capacity];
        scratch.AsSpan().CopyTo(next);
        scratch = next;
    }
}