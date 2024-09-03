namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
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
    protected override MeshRenderBatch CreateBatch(IWGPUContext context, in MeshBatchKey key)
    {
        return new MeshRenderBatch(context, key);
    }
}

public class MeshRenderBatch : RenderBatch<Mat4f>
{
    public Handle<MeshAsset> Mesh { get; init; }
    public Handle Material { get; init; }

    public MeshRenderBatch(IWGPUContext gpuContext, in MeshBatchKey key) : base(gpuContext)
    {
        Key = key.GetHashCode();
        Mesh = key.Mesh;
        Material = key.Material;
    }   
}

public class MeshRenderBatchDraw : IRenderStepDraw
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