namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Transform;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

struct ExtractSpritesJob : IForEach<Transform2, Sprite>
{
    public required SpriteBatches Batches { get; init; }

    public void Execute(ref Transform2 transform, ref Sprite sprite)
    {
        var batch = Batches.GetOrCreate(new SpriteBatchKey(sprite.Material));
        var matrix = transform.ToMat4f().Transpose();
        batch.Write(new SpriteBatch.InstanceData
        {
            Model_0 = matrix.Col0,
            Model_1 = matrix.Col1,
            Model_2 = matrix.Col2,
            Slice = sprite.Slice,
            Color = sprite.Color,
        });
    }
}

class ExtractSpritesSystem : ECS.Core.Sys<RenderAssets, AssetServer, IWGPUContext, SpriteBatches, Query<Transform2, Sprite>>
{
    public ExtractSpritesSystem()
        : base(new ECS.Core.SystemDescriptor(nameof(ExtractSpritesSystem)))
    { }

    protected override void OnTick(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, SpriteBatches batches,
        Query<Transform2, Sprite> query)
    {
        foreach (var spriteMaterial in assetServer.GetAssets<SpriteMaterial>().AssetInfos)
        {
            renderAssets.Prepare(gpuContext, assetServer, spriteMaterial.Handle);
        }

        batches.Reset();
        query.ForEach(new ExtractSpritesJob
        {
            Batches = batches,
        });
    }
}

class WriteSpriteBatchesSystem : ECS.Core.Sys<RenderAssets, AssetServer, IWGPUContext, SpriteBatches>
{
    public WriteSpriteBatchesSystem()
        : base(new ECS.Core.SystemDescriptor(nameof(WriteSpriteBatchesSystem)).After(nameof(ExtractSpritesSystem)))
    { }

    protected override void OnTick(
        RenderAssets renderAssets, AssetServer assetServer,
        IWGPUContext gpuContext, SpriteBatches batches)
    {
        foreach (var batch in batches.Batches)
        {
            if (batch.IsEmpty) continue;

            GPUBuffer? instanceBuffer;
            if (batch.InstanceBufferHandle == Handle<GPUBuffer>.Null)
            {
                instanceBuffer = batch.CreateBuffer(gpuContext);
                batch.InstanceBufferHandle = renderAssets.Add(instanceBuffer);
            }
            else
            {
                instanceBuffer = renderAssets.Get<GPUBuffer>(batch.InstanceBufferHandle);
                batch.EnsureCapacity(instanceBuffer);
            }

            instanceBuffer.Write(batch.GetData());
        }
    }
}

class DrawSpriteBatchesSystem : ECS.Core.Sys<RenderSteps, RenderAssets, SpriteBatches>
{
    public DrawSpriteBatchesSystem() : base(new ECS.Core.SystemDescriptor(nameof(DrawSpriteBatchesSystem)).After(nameof(WriteSpriteBatchesSystem)))
    {
    }

    protected override void OnTick(RenderSteps renderSteps, RenderAssets renderAssets, SpriteBatches batches)
    {
        var commands = renderSteps.GetCommands(RenderStep2D.Main);

        foreach (var batch in batches.Batches)
        {
            if (batch.IsEmpty) continue;

            var material = renderAssets.Get<MaterialRenderData>(batch.Material);
            var draw = new Draw()
            {
                Pipeline = material.Pipeline,
                VertexCount = 6,
                InstanceCount = (uint)batch.Count,
            };
            material.BindGroups.CopyTo(draw.BindGroups);
            draw.VertexBuffers[0] = batch.InstanceBufferHandle;

            commands.Add(draw);
        }
    }
}