namespace Pollus.Engine.Rendering;

using Assets;
using ECS;
using Graphics.WGPU;
using Transform;
using Utils;

public class ExtractTextDrawSystem : ExtractDrawSystem<FontBatches, FontBatch, Query<GlobalTransform, TextDraw, TextMesh, TextFont>>
{
    protected override void Extract(RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext,
        FontBatches batches, Query<GlobalTransform, TextDraw, TextMesh, TextFont> query)
    {
        batches.Reset();
        query.ForEach(batches, static (in data, ref transform, ref textDraw, ref textMesh, ref textFont) =>
        {
            if (textMesh.Mesh == Handle<TextMeshAsset>.Null || textFont.Material == Handle.Null) return;

            var batch = data.GetOrCreate(new FontBatchKey(textMesh.Mesh, textFont.Material, textFont.RenderStep));
            var sortKey = RenderingUtils.CreateSortKey2D(transform.Value.Col3.Z, batch.Key);
            batch.Draw(sortKey, transform.Value, textDraw.Color);
        });
    }
}
