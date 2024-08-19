namespace Pollus.Engine.Rendering;

using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;

public class SpriteBatch
{
    public int Key => Material.GetHashCode();
    public required Handle Material { get; init; }
    public required GPUBuffer InstanceBuffer;

    VertexData instanceData;

    public int Count;
    public bool IsEmpty => Count == 0;
    public bool IsFull => Count == instanceData.Capacity;
    public int Capacity => (int)instanceData.Capacity;

    public SpriteBatch()
    {
        instanceData = VertexData.From(16, SpriteMaterial.InstanceFormats);
    }

    public void Dispose()
    {
        InstanceBuffer.Dispose();
    }

    public void Reset()
    {
        Count = 0;
    }

    public void Write(Vec4f model_0, Vec4f model_1, Vec4f model_2, Vec4f slice, Vec4f color)
    {
        instanceData.Write(Count, model_0, 0);
        instanceData.Write(Count, model_1, 1);
        instanceData.Write(Count, model_2, 2);
        instanceData.Write(Count, slice, 3);
        instanceData.Write(Count, color, 4);

        Count++;
    }

    public void WriteBuffer()
    {
        InstanceBuffer.Write<byte>(instanceData.Slice(0, Count));
    }

    public void Resize(IWGPUContext gpuContext, int capacity)
    {
        InstanceBuffer.Dispose();
        InstanceBuffer = gpuContext.CreateBuffer(new()
        {
            Label = $"InstanceBuffer_{Key}",
            Size = (ulong)capacity * (ulong)SpriteMaterial.InstanceFormats.Stride(),
            Usage = BufferUsage.CopyDst | BufferUsage.Vertex,
        });

        instanceData.Resize((uint)capacity);
    }
}

public class SpriteBatches
{
    Dictionary<int, SpriteBatch> batches = new();

    public IEnumerable<SpriteBatch> Batches => batches.Values.Where(e => e.Count > 0);

    public bool TryGetBatch(Handle materialHandle, out SpriteBatch batch)
    {
        return batches.TryGetValue(materialHandle.GetHashCode(), out batch!);
    }

    public SpriteBatch CreateBatch(IWGPUContext context, int capacity, Handle materialHandle)
    {
        var key = materialHandle.GetHashCode();
        if (batches.TryGetValue(key, out var batch)) return batch;

        batch = new SpriteBatch()
        {
            Material = materialHandle,
            InstanceBuffer = context.CreateBuffer(new()
            {
                Label = $"InstanceBuffer_{key}",
                Size = (ulong)capacity * (ulong)SpriteMaterial.InstanceFormats.Stride(),
                Usage = BufferUsage.CopyDst | BufferUsage.Vertex,
            }),
        };

        batches.Add(key, batch);
        return batch;
    }
}