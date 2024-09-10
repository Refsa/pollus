namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;
using Pollus.Utils;

public record struct MeshBatchKey
{
    readonly int hashCode;
    public readonly Handle<MeshAsset> Mesh;
    public readonly Handle Material;

    public MeshBatchKey(Handle<MeshAsset> mesh, Handle material)
    {
        Mesh = mesh;
        Material = material;
        hashCode = HashCode.Combine(mesh, material);
    }

    public override int GetHashCode() => hashCode;
}

public class MeshRenderBatches : RenderBatches<MeshRenderBatch, MeshBatchKey>
{
    protected override MeshRenderBatch CreateBatch( in MeshBatchKey key)
    {
        return new MeshRenderBatch(key);
    }
}

public class MeshRenderBatch : RenderBatch<Mat4f>
{
    public Handle<MeshAsset> Mesh { get; init; }
    public Handle Material { get; init; }

    public MeshRenderBatch(in MeshBatchKey key)
    {
        Key = key.GetHashCode();
        Mesh = key.Mesh;
        Material = key.Material;
    }
}

public class MeshRenderBatchDraw
{
    public RenderStep2D Stage => RenderStep2D.Main;

    public void Render(GPURenderPassEncoder encoder, Resources resources, RenderAssets renderAssets)
    {
        var batches = resources.Get<MeshRenderBatches>();

        foreach (var batch in batches.Batches)
        {
            if (batch.IsEmpty) continue;

            var material = renderAssets.Get<MaterialRenderData>(batch.Material);
            var mesh = renderAssets.Get<MeshRenderData>(batch.Mesh);

            var draw = new Draw()
            {
                Pipeline = material.Pipeline,
                IndexBuffer = mesh.IndexBuffer,
                IndexCount = (uint)mesh.IndexCount,
                InstanceCount = (uint)batch.Count,
            };
            material.BindGroups.CopyTo(draw.BindGroups);
            draw.VertexBuffers[0] = mesh.VertexBuffer;
            draw.VertexBuffers[1] = batch.InstanceBufferHandle;
        }
    }
}