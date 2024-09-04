namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;
using Pollus.Utils;

public record struct ShapeBatchKey
{
    readonly int hashCode;
    public readonly Handle<Shape> Shape;
    public readonly Handle Material;

    public ShapeBatchKey(Handle<Shape> shape, Handle material)
    {
        Shape = shape;
        Material = material;
        hashCode = HashCode.Combine(shape, material);
    }

    public override int GetHashCode() => hashCode;
}

public partial class ShapeBatch : RenderBatch<ShapeBatch.InstanceData>
{
    [ShaderType]
    public partial struct InstanceData
    {
        public Vec4f Model_0;
        public Vec4f Model_1;
        public Vec4f Model_2;
        public Vec4f Color;
    }

    public Handle Material { get; }
    public Handle<Shape> Shape { get; }

    public ShapeBatch(IWGPUContext gpuContext, in ShapeBatchKey key) : base(gpuContext)
    {
        Key = key.GetHashCode();
        Material = key.Material;
        Shape = key.Shape;
    }

    public void Write(Mat4f model, Color color)
    {
        var tModel = model.Transpose();
        Write(new InstanceData()
        {
            Model_0 = tModel.Col0,
            Model_1 = tModel.Col1,
            Model_2 = tModel.Col2,
            Color = color,
        });
    }
}

public class ShapeBatches : RenderBatches<ShapeBatch, ShapeBatchKey>
{
    protected override ShapeBatch CreateBatch(IWGPUContext context, in ShapeBatchKey key)
    {
        return new ShapeBatch(context, key);
    }
}

public class ShapeBatchDraw : IRenderStepDraw
{
    public RenderStep2D Stage => RenderStep2D.Main;

    public void Render(GPURenderPassEncoder encoder, Resources resources, RenderAssets renderAssets)
    {
        var batches = resources.Get<ShapeBatches>();

        foreach (var batch in batches.Batches)
        {
            if (batch.IsEmpty) continue;
            batch.WriteBuffer();

            var material = renderAssets.Get<MaterialRenderData>(batch.Material);
            var shape = renderAssets.Get<ShapeRenderData>(batch.Shape);

            encoder.SetPipeline(material.Pipeline);
            for (int i = 0; i < material.BindGroups.Length; i++)
            {
                encoder.SetBindGroup(material.BindGroups[i], (uint)i);
            }

            encoder.SetVertexBuffer(0, shape.VertexBuffer);
            encoder.SetVertexBuffer(1, batch.InstanceBuffer);
            encoder.Draw(shape.VertexCount, (uint)batch.Count, 0, 0);
        }
    }
}