namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics.WGPU;

class ExtractSpritesSystem : ExtractDrawSystem<SpriteBatches, SpriteBatch, Query<GlobalTransform, Sprite>>
{
    protected override void Extract(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, SpriteBatches batches,
        Query<GlobalTransform, Sprite> query)
    {
        var hasStatic = query.Any<Added<StaticCalculated>>();
        batches.Reset(hasStatic);

        if (hasStatic)
        {
            query.Filtered<All<StaticCalculated>>().ForEachChunk(new ExtractChunk(batches, true));
        }

        query.Filtered<None<StaticCalculated>>().ForEachChunk(new ExtractChunk(batches, false));
    }

    readonly struct ExtractChunk(SpriteBatches batches, bool isStatic) : IChunkForEach<GlobalTransform, Sprite>
    {
        public void Execute(
            scoped in ReadOnlySpan<Entity> entities,
            in Span<GlobalTransform> transforms,
            in Span<Sprite> sprites)
        {
            SpriteBatch? lastBatch = null;
            int lastMaterialId = -1;

            for (int i = 0; i < transforms.Length; i++)
            {
                ref var sprite = ref sprites[i];

                var batch = (sprite.Material.ID == lastMaterialId) ? lastBatch! : batches.GetOrCreate(new SpriteBatchKey(sprite.Material, isStatic));
                lastMaterialId = sprite.Material.ID;
                lastBatch = batch;

                ref readonly var matrix = ref transforms[i].Value;
                var depthKey = RenderingUtils.CreateDepthSortKey2D(matrix.Col3.Z);
                ref var instance = ref batch.Draw(depthKey);

                instance.Model0.X = matrix.Col0.X;
                instance.Model0.Y = matrix.Col1.X;
                instance.Model0.Z = matrix.Col3.Z;
                instance.Model0.W = matrix.Col3.X;

                instance.Model1.X = matrix.Col0.Y;
                instance.Model1.Y = matrix.Col1.Y;
                instance.Model1.Z = matrix.Col3.Y;

                instance.Slice.X = (ushort)sprite.Slice.X;
                instance.Slice.Y = (ushort)sprite.Slice.Y;
                instance.Slice.Z = (ushort)sprite.Slice.Width;
                instance.Slice.W = (ushort)sprite.Slice.Height;

                instance.Color = sprite.PackedColor;
            }
        }
    }
}
