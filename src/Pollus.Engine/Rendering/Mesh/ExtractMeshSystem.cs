namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics.WGPU;

public class ExtractMeshDrawSystem<TMaterial> : ExtractDrawSystem<MeshRenderBatches, MeshRenderBatch, Query<GlobalTransform, MeshDraw<TMaterial>>>
    where TMaterial : IMaterial
{
    protected override void Extract(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, MeshRenderBatches batches,
        Query<GlobalTransform, MeshDraw<TMaterial>> query)
    {
        batches.Reset();
        query.ForEach(batches, static (in batches, ref transform, ref renderable) =>
        {
            var batch = batches.GetOrCreate(new MeshBatchKey(renderable.Mesh, renderable.Material));
            var sortKey = RenderingUtils.CreateSortKey2D(transform.Value.Col3.Z, batch.Key);
            batch.Draw(sortKey, transform.Value);
        });
    }
}
