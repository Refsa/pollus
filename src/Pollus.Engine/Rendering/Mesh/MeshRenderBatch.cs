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
}