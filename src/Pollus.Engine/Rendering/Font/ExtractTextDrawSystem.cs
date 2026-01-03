namespace Pollus.Engine.Rendering;

using Assets;
using ECS;
using Graphics;
using Graphics.WGPU;
using Transform;
using Utils;

public class ExtractTextDrawSystem : ExtractDrawSystem<FontBatches, FontBatch, Query<Transform2D, TextDraw, TextMesh>>
{
    readonly struct Job : IForEach<Transform2D, TextDraw, TextMesh>
    {
        public required FontBatches Batches { get; init; }
        public required DrawQueue DrawQueue { get; init; }
        public required RendererKey RendererKey { get; init; }

        public void Execute(ref Transform2D transform, ref TextDraw textDraw, ref TextMesh textMesh)
        {
            if (textMesh.Mesh == Handle<TextMeshAsset>.Null || textMesh.Material == Handle<FontMaterial>.Null) return;

            var batch = Batches.GetOrCreate(new FontBatchKey(textMesh.Mesh, textMesh.Material));
            var instanceIndex = batch.Write(transform.ToMat4f(), textDraw.Color);

            var sortKey = RenderingUtils.CreateSortKey2D(transform.Position.Y, batch.Key);
            DrawQueue.Add(sortKey, RendererKey.Key, batch.BatchID, instanceIndex);
        }
    }

    protected override void Extract(RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext,
        FontBatches batches, Query<Transform2D, TextDraw, TextMesh> query, DrawQueue drawQueue)
    {
        batches.Reset();
        query.ForEach(new Job
        {
            Batches = batches,
            DrawQueue = drawQueue,
            RendererKey = RendererKey.From<FontBatches>(),
        });
    }
}