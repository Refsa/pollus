namespace Pollus.Engine.Rendering;

using Pollus.Assets;
using ECS;
using Graphics.WGPU;
using Transform;
using Utils;

public class ExtractTextDrawSystem : ExtractDrawSystem<FontBatches, FontBatch, Query<GlobalTransform, TextDraw, TextMesh, TextFont>>
{
    protected override void Extract(RenderAssets renderAssets, AssetServer assetServer, IWGPUContext gpuContext,
        FontBatches batches, Query<GlobalTransform, TextDraw, TextMesh, TextFont> query)
    {
        var hasStatic = query.Any<Added<StaticCalculated>>();
        batches.Reset(hasStatic);

        if (hasStatic)
        {
            query.Filtered<All<StaticCalculated>>().ForEachChunk(new ExtractChunk(batches, true));
        }

        query.Filtered<None<StaticCalculated>>().ForEachChunk(new ExtractChunk(batches, false));
    }

    readonly struct ExtractChunk(FontBatches batches, bool isStatic) : IChunkForEach<GlobalTransform, TextDraw, TextMesh, TextFont>
    {
        public void Execute(
            scoped in ReadOnlySpan<Entity> entities,
            in Span<GlobalTransform> transforms,
            in Span<TextDraw> textDraws,
            in Span<TextMesh> textMeshes,
            in Span<TextFont> textFonts)
        {
            FontBatch? lastBatch = null;
            int lastMeshId = -1;
            int lastMaterialId = -1;

            for (int i = 0; i < transforms.Length; i++)
            {
                ref var textMesh = ref textMeshes[i];
                ref var textFont = ref textFonts[i];
                if (textMesh.Mesh == Handle<TextMeshAsset>.Null || textFont.Material == Handle.Null) continue;

                var meshId = textMesh.Mesh.ID;
                var materialId = textFont.Material.ID;
                var batch = meshId == lastMeshId && materialId == lastMaterialId
                    ? lastBatch!
                    : batches.GetOrCreate(new FontBatchKey(textMesh.Mesh, textFont.Material, IsStatic: isStatic));

                lastMeshId = meshId;
                lastMaterialId = materialId;
                lastBatch = batch;

                var matrix = transforms[i].Value.Transpose();
                var sortKey = RenderingUtils.CreateDepthSortKey2D(matrix.Col2.W);
                ref var instance = ref batch.Draw(sortKey);
                instance.Model0 = matrix.Col0;
                instance.Model1 = matrix.Col1;
                instance.Model2 = matrix.Col2;
                instance.Color = textDraws[i].Color;
            }
        }
    }
}
