namespace Pollus.Engine.Rendering;

using Pollus.Graphics;
using Pollus.Mathematics;
using Pollus.Utils;

public record struct MeshBatchKey(Handle<MeshAsset> Mesh, Handle Material, RenderStep2D RenderStep = RenderStep2D.Main)
{
    public int SortKey { get; } = RenderingUtils.PackSortKeys(Mesh.ID, Material.ID);
}

public class MeshRenderBatch : RenderBatch<Mat4f>
{
    public Handle<MeshAsset> Mesh { get; init; }
    public Handle Material { get; init; }
    public override Handle[] RequiredResources { get; }

    public MeshRenderBatch(in MeshBatchKey key) : base(key.SortKey)
    {
        Mesh = key.Mesh;
        Material = key.Material;
        RenderStep = (int)key.RenderStep;
        RequiredResources = [Mesh, Material];
    }
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