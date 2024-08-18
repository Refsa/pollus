namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;

public class RenderableBatches
{
    Dictionary<int, RenderableBatch> batches = new();

    public IEnumerable<RenderableBatch> Batches => batches.Values.Where(e => e.Count > 0);

    public bool TryGetBatch(Handle<MeshAsset> meshHandle, Handle materialHandle, out RenderableBatch batch)
    {
        var key = HashCode.Combine(meshHandle, materialHandle);
        return batches.TryGetValue(key, out batch!);
    }

    public RenderableBatch CreateBatch(IWGPUContext context, int capacity, Handle<MeshAsset> meshHandle, Handle materialHandle)
    {
        var key = HashCode.Combine(meshHandle, materialHandle);
        if (batches.TryGetValue(key, out var batch)) return batch;

        batch = new RenderableBatch()
        {
            Key = key,
            Mesh = meshHandle,
            Material = materialHandle,
            Transforms = new Mat4f[capacity],
            InstanceBuffer = context.CreateBuffer(new()
            {
                Label = $"InstanceBuffer_{key}",
                Size = (ulong)capacity * (ulong)Mat4f.SizeInBytes,
                Usage = BufferUsage.CopyDst | BufferUsage.Vertex,
            }),
        };

        batches.Add(key, batch);
        return batch;
    }
}

public class RenderableBatch : IDisposable
{
    public required int Key { get; init; }
    public required Handle<MeshAsset> Mesh { get; init; }
    public required Handle Material { get; init; }
    public required Mat4f[] Transforms;
    public required GPUBuffer InstanceBuffer;

    public int Count;
    public bool IsEmpty => Count == 0;
    public bool IsFull => Count == Transforms.Length;
    public int Capacity => Transforms.Length;

    public void Dispose()
    {
        InstanceBuffer.Dispose();
    }

    public void Reset()
    {
        Count = 0;
    }

    public void Write(Mat4f transform)
    {
        Transforms[Count++] = transform;
    }

    public void Write(ReadOnlySpan<Mat4f> transforms)
    {
        transforms.CopyTo(Transforms.AsSpan()[Count..]);
        Count += transforms.Length;
    }

    public Span<Mat4f> GetBlock(int count)
    {
        var block = Transforms.AsSpan()[Count..(Count + count)];
        Count += count;
        return block;
    }

    public void WriteBuffer()
    {
        InstanceBuffer.Write<Mat4f>(Transforms.AsSpan()[..Count], 0);
    }

    public void Resize(IWGPUContext gpuContext, int capacity)
    {
        InstanceBuffer.Dispose();
        InstanceBuffer = gpuContext.CreateBuffer(new()
        {
            Label = $"InstanceBuffer_{Key}",
            Size = (ulong)capacity * (ulong)Mat4f.SizeInBytes,
            Usage = BufferUsage.CopyDst | BufferUsage.Vertex,
        });

        var transforms = new Mat4f[capacity];
        Transforms.AsSpan().CopyTo(transforms);
        Transforms = transforms;
    }
}