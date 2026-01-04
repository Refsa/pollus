namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics.WGPU;

public class ExtractMeshDrawSystem<TMaterial> : ExtractDrawSystem<MeshRenderBatches, MeshRenderBatch, Query<GlobalTransform, MeshDraw<TMaterial>>>
    where TMaterial : IMaterial
{
    struct ExtractJob : IForEach<GlobalTransform, MeshDraw<TMaterial>>
    {
        public required MeshRenderBatches Batches { get; init; }

        public void Execute(ref GlobalTransform transform, ref MeshDraw<TMaterial> renderable)
        {
            var batch = Batches.GetOrCreate(new MeshBatchKey(renderable.Mesh, renderable.Material));
            var sortKey = RenderingUtils.CreateSortKey2D(transform.Value.Col3.Z, batch.Key);
            batch.Draw(sortKey, transform.Value);
        }
    }

    protected override void Extract(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, MeshRenderBatches batches,
        Query<GlobalTransform, MeshDraw<TMaterial>> query)
    {
        batches.Reset();
        query.ForEach(new ExtractJob
        {
            Batches = batches,
        });
    }
}
