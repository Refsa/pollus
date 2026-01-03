namespace Pollus.Engine.Rendering;

using Pollus.Graphics;
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
    public override Draw GetDrawCall(int batchID, int start, int count, IRenderAssets renderAssets)
    {
        var batch = GetBatch(batchID);
        var material = renderAssets.Get<MaterialRenderData>(batch.Material);
        var mesh = renderAssets.Get<MeshRenderData>(batch.Mesh);

        return Draw.Create(material.Pipeline)
            .SetIndexBuffer(mesh.IndexBuffer, mesh.IndexFormat, (uint)mesh.IndexCount, 0)
            .SetVertexInfo(mesh.VertexCount, mesh.VertexOffset)
            .SetInstanceInfo((uint)count, (uint)start)
            .SetVertexBuffer(0, mesh.VertexBuffer)
            .SetVertexBuffer(1, batch.InstanceBufferHandle)
            .SetBindGroups(material.BindGroups);
    }

    protected override MeshRenderBatch CreateBatch(in MeshBatchKey key)
    {
        return new MeshRenderBatch(key);
    }
}

public class MeshRenderBatch : RenderBatch<Mat4f>
{
    public Handle<MeshAsset> Mesh { get; init; }
    public Handle Material { get; init; }
    public override Handle[] RequiredResources => [Mesh, Material];

    public MeshRenderBatch(in MeshBatchKey key) : base(key.GetHashCode())
    {
        Mesh = key.Mesh;
        Material = key.Material;
    }

    public void Draw(ulong sortKey, Mat4f model)
    {
        base.Draw(sortKey, model.Transpose());
    }
}