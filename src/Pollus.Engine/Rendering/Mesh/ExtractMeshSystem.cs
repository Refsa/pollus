namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics.WGPU;

public class ExtractMeshDrawSystem<TMaterial> : ExtractDrawSystem<MeshRenderBatches, MeshRenderBatch, Query<Transform2D, MeshDraw<TMaterial>>>
    where TMaterial : IMaterial
{
    struct ExtractJob : IForEach<Transform2D, MeshDraw<TMaterial>>
    {
        public required MeshRenderBatches Batches { get; init; }

        public void Execute(ref Transform2D transform, ref MeshDraw<TMaterial> renderable)
        {
            var batch = Batches.GetOrCreate(new MeshBatchKey(renderable.Mesh, renderable.Material));
            batch.Write(transform.ToMat4f());
        }
    }

    protected override void Extract(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, MeshRenderBatches batches,
        Query<Transform2D, MeshDraw<TMaterial>> query)
    {
        query.ForEach(new ExtractJob
        {
            Batches = batches,
        });
    }
}
