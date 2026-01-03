namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Graphics.WGPU;

public class ExtractMeshDrawSystem<TMaterial> : ExtractDrawSystem<MeshRenderBatches, MeshRenderBatch, Query<Transform2D, MeshDraw<TMaterial>>>
    where TMaterial : IMaterial
{
    struct ExtractJob : IForEach<Transform2D, MeshDraw<TMaterial>>
    {
        public required MeshRenderBatches Batches { get; init; }
        public required DrawQueue DrawQueue { get; init; }
        public required int RendererID { get; init; }

        public void Execute(ref Transform2D transform, ref MeshDraw<TMaterial> renderable)
        {
            var batch = Batches.GetOrCreate(new MeshBatchKey(renderable.Mesh, renderable.Material));
            var instanceIndex = batch.Write(transform.ToMat4f());

            var sortKey = RenderingUtils.CreateSortKey2D(transform.ZIndex, batch.Key);
            DrawQueue.Add(sortKey, RendererID, batch.BatchID, instanceIndex);
        }
    }

    protected override void Extract(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, MeshRenderBatches batches,
        Query<Transform2D, MeshDraw<TMaterial>> query, DrawQueue drawQueue)
    {
        batches.Reset();
        query.ForEach(new ExtractJob
        {
            Batches = batches,
            DrawQueue = drawQueue,
            RendererID = RendererKey.From<MeshRenderBatches>().Key,
        });
    }
}
