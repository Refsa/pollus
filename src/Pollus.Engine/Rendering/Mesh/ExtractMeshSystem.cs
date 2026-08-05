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
        var hasStatic = query.Any<Added<StaticCalculated>>();
        batches.Reset(hasStatic);

        if (hasStatic)
        {
            query.Filtered<All<StaticCalculated>>().ForEachChunk(new ExtractChunk(batches, true));
        }

        query.Filtered<None<StaticCalculated>>().ForEachChunk(new ExtractChunk(batches, false));
    }

    readonly struct ExtractChunk(MeshRenderBatches batches, bool isStatic) : IChunkForEach<GlobalTransform, MeshDraw<TMaterial>>
    {
        public void Execute(
            scoped in ReadOnlySpan<Entity> entities,
            in Span<GlobalTransform> transforms,
            in Span<MeshDraw<TMaterial>> renderables)
        {
            MeshRenderBatch? lastBatch = null;
            int lastMeshId = -1;
            int lastMaterialId = -1;

            for (int i = 0; i < transforms.Length; i++)
            {
                ref var renderable = ref renderables[i];
                var meshId = renderable.Mesh.ID;
                var materialId = renderable.Material.ID;
                var batch = meshId == lastMeshId && materialId == lastMaterialId
                    ? lastBatch!
                    : batches.GetOrCreate(new MeshBatchKey(renderable.Mesh, renderable.Material, IsStatic: isStatic));

                lastMeshId = meshId;
                lastMaterialId = materialId;
                lastBatch = batch;

                ref readonly var matrix = ref transforms[i].Value;
                var sortKey = RenderingUtils.CreateDepthSortKey2D(matrix.Col3.Z);
                batch.Draw(sortKey) = matrix;
            }
        }
    }
}
