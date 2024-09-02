namespace Pollus.Engine.Rendering;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;
using Pollus.Utils;

public class MeshRenderBatches
{
    List<MeshRenderBatch> batches = new();
    Dictionary<int, int> batchLookup = new();

    public ListEnumerable<MeshRenderBatch> Batches => new(batches);

    public bool TryGetBatch(Handle<MeshAsset> meshHandle, Handle materialHandle, out MeshRenderBatch batch)
    {
        var key = HashCode.Combine(meshHandle, materialHandle);
        if (batchLookup.TryGetValue(key, out var batchIdx))
        {
            batch = batches[batchIdx];
            return true;
        }
        batch = null!;
        return false;
    }

    public MeshRenderBatch CreateBatch(IWGPUContext context, int capacity, Handle<MeshAsset> meshHandle, Handle materialHandle)
    {
        var key = HashCode.Combine(meshHandle.GetHashCode(), materialHandle.GetHashCode());
        if (batchLookup.TryGetValue(key, out var batchIdx)) return batches[batchIdx];

        var batch = new MeshRenderBatch()
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

        batchLookup.Add(key, batches.Count);
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

public class MeshRenderBatch : IDisposable
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

public class RenderBatchDraw : IRenderStepDraw
{
    public RenderStep2D Stage => RenderStep2D.Main;

    public void Render(GPURenderPassEncoder encoder, Resources resources, RenderAssets renderAssets)
    {
        var batches = resources.Get<MeshRenderBatches>();

        foreach (var batch in batches.Batches)
        {
            batch.WriteBuffer();

            var material = renderAssets.Get<MaterialRenderData>(batch.Material);
            var mesh = renderAssets.Get<MeshRenderData>(batch.Mesh);

            encoder.SetPipeline(material.Pipeline);
            for (int i = 0; i < material.BindGroups.Length; i++)
            {
                encoder.SetBindGroup(material.BindGroups[i], (uint)i);
            }

            if (mesh.IndexBuffer != null)
            {
                encoder.SetIndexBuffer(mesh.IndexBuffer, mesh.IndexFormat);
            }

            encoder.SetVertexBuffer(0, mesh.VertexBuffer);
            encoder.SetVertexBuffer(1, batch.InstanceBuffer);
            encoder.DrawIndexed((uint)mesh.IndexCount, (uint)batch.Count, 0, 0, 0);
        }
    }
}