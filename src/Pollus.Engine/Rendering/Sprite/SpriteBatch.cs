namespace Pollus.Engine.Rendering;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;

public class SpriteBatch : IDisposable
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
        InstanceBuffer.Write(instanceData.Slice(0, Count), 0);
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

public class SpriteBatches : IDisposable
{
    List<SpriteBatch> batches = new();
    Dictionary<int, int> batcheLookup = new();

    public ListEnumerable<SpriteBatch> Batches => new(batches);

    public void Dispose()
    {
        foreach (var batch in batches)
        {
            batch.Dispose();
        }
        batches.Clear();
    }

    public bool TryGetBatch(Handle materialHandle, out SpriteBatch batch)
    {
        if (batcheLookup.TryGetValue(materialHandle.GetHashCode(), out var batchIdx))
        {
            batch = batches[batchIdx];
            return true;
        }
        batch = null!;
        return false;
    }

    public SpriteBatch CreateBatch(IWGPUContext context, int capacity, Handle materialHandle)
    {
        var key = materialHandle.GetHashCode();
        if (batcheLookup.TryGetValue(key, out var batchIdx)) return batches[batchIdx];

        var batch = new SpriteBatch()
        {
            Material = materialHandle,
            InstanceBuffer = context.CreateBuffer(new()
            {
                Label = $"InstanceBuffer_{key}",
                Size = (ulong)capacity * (ulong)SpriteMaterial.InstanceFormats.Stride(),
                Usage = BufferUsage.CopyDst | BufferUsage.Vertex,
            }),
        };

        batcheLookup.Add(key, batches.Count);
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
}

public class SpriteBatchDraw : IRenderStepDraw
{
    public RenderStep2D Stage => RenderStep2D.Main;

    public void Render(GPURenderPassEncoder encoder, Resources resources, RenderAssets renderAssets)
    {
        var batches = resources.Get<SpriteBatches>();

        foreach (var batch in batches.Batches)
        {
            if (batch.IsEmpty) continue;

            batch.WriteBuffer();

            var material = renderAssets.Get<MaterialRenderData>(batch.Material);

            encoder.SetPipeline(material.Pipeline);
            for (int i = 0; i < material.BindGroups.Length; i++)
            {
                encoder.SetBindGroup(material.BindGroups[i], (uint)i);
            }

            encoder.SetVertexBuffer(0, batch.InstanceBuffer);
            encoder.Draw(6, (uint)batch.Count, 0, 0);
        }
    }
}