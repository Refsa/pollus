namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;
using Pollus.Utils;

public record struct SpriteBatchKey(Handle Material);

public class SpriteBatch : RenderBatch<SpriteBatch.InstanceData>
{
    public struct InstanceData
    {
        public required Vec4f Model_0;
        public required Vec4f Model_1;
        public required Vec4f Model_2;
        public required Vec4f Slice;
        public required Vec4f Color;
    }

    public Handle Material { get; init; }

    public SpriteBatch(IWGPUContext gpuContext, in SpriteBatchKey key) : base(gpuContext)
    {
        Key = key.GetHashCode();
        Material = key.Material;
    }
}

public class SpriteBatches : RenderBatches<SpriteBatch, SpriteBatchKey>
{
    protected override SpriteBatch CreateBatch(IWGPUContext context, in SpriteBatchKey key)
    {
        return new SpriteBatch(context, key);
    }
}

public class SpriteBatchDraw : IRenderStepDraw
{
    public RenderStep2D Stage => RenderStep2D.Main;

    public void Render(GPURenderPassEncoder encoder, Resources resources, RenderAssets renderAssets)
    {
        var batches = resources.Get<SpriteBatches>();

        foreach (var batch in batches.Batches)
        {
            if (batch.IsEmpty) continue;

            batch.WriteBuffer();

            var material = renderAssets.Get<MaterialRenderData>(batch.Material);

            encoder.SetPipeline(material.Pipeline);
            for (int i = 0; i < material.BindGroups.Length; i++)
            {
                encoder.SetBindGroup(material.BindGroups[i], (uint)i);
            }

            encoder.SetVertexBuffer(0, batch.InstanceBuffer);
            encoder.Draw(6, (uint)batch.Count, 0, 0);
        }
    }
}