namespace Pollus.Graphics;

using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

public interface IRenderBatch
{
    int Capacity { get; }
    int Count { get; }
    bool IsEmpty { get; }
    bool IsFull { get; }

    void Reset();
}

public abstract class RenderBatch<TInstanceData> : IRenderBatch, IDisposable
    where TInstanceData : unmanaged, IShaderType
{
    int count;
    TInstanceData[] scratch;

    public int Key { get; init; }
    public Handle<GPUBuffer> InstanceBufferHandle { get; set; } = Handle<GPUBuffer>.Null;

    public int Count => count;
    public int Capacity => scratch.Length;
    public bool IsEmpty => count == 0;
    public bool IsFull => count == scratch.Length;

    public RenderBatch()
    {
        scratch = new TInstanceData[16];
    }

    public void Dispose()
    {

    }

    public void Reset()
    {
        count = 0;
    }

    public void Write(in TInstanceData data)
    {
        if (count == scratch.Length) Resize(count * 2);

        scratch[count++] = data;
    }

    public void Write(ReadOnlySpan<TInstanceData> data)
    {
        if (count + data.Length > scratch.Length) Resize(count + data.Length);

        data.CopyTo(scratch.AsSpan()[count..]);
        count += data.Length;
    }

    public ReadOnlySpan<TInstanceData> GetData()
    {
        return scratch.AsSpan(0, count);
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