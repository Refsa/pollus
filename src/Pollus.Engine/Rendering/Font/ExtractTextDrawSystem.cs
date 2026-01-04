namespace Pollus.Engine.Rendering;

using Assets;
using ECS;
using Graphics.WGPU;
using Transform;
using Utils;

public class ExtractTextDrawSystem : ExtractDrawSystem<FontBatches, FontBatch, Query<Transform2D, TextDraw, TextMesh>>
{
    protected override void Extract(RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext,
        FontBatches batches, Query<Transform2D, TextDraw, TextMesh> query)
    {
        batches.Reset();
        query.ForEach(batches, static (in batches, ref transform, ref textDraw, ref textMesh) =>
        {
            if (textMesh.Mesh == Handle<TextMeshAsset>.Null || textMesh.Material == Handle<FontMaterial>.Null) return;

            var batch = batches.GetOrCreate(new FontBatchKey(textMesh.Mesh, textMesh.Material));
            var sortKey = RenderingUtils.CreateSortKey2D(transform.Position.Y, batch.Key);
            batch.Draw(sortKey, transform.ToMat4f(), textDraw.Color);
        });
    }
}