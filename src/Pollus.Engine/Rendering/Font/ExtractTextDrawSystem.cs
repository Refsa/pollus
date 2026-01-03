namespace Pollus.Engine.Rendering;

using Assets;
using ECS;
using Graphics.WGPU;
using Transform;
using Utils;

public class ExtractTextDrawSystem : ExtractDrawSystem<FontBatches, FontBatch, Query<Transform2D, TextDraw, TextMesh>>
{
    readonly struct Job : IForEach<Transform2D, TextDraw, TextMesh>
    {
        public required FontBatches Batches { get; init; }

        public void Execute(ref Transform2D transform, ref TextDraw textDraw, ref TextMesh textMesh)
        {
            if (textMesh.Mesh == Handle<TextMeshAsset>.Null || textMesh.Material == Handle<FontMaterial>.Null) return;

            var batch = Batches.GetOrCreate(new FontBatchKey(textMesh.Mesh, textMesh.Material));
            var sortKey = RenderingUtils.CreateSortKey2D(transform.Position.Y, batch.Key);
            batch.Draw(sortKey, transform.ToMat4f(), textDraw.Color);
        }
    }

    protected override void Extract(RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext,
        FontBatches batches, Query<Transform2D, TextDraw, TextMesh> query)
    {
        batches.Reset();
        query.ForEach(new Job
        {
            Batches = batches,
        });
    }
}