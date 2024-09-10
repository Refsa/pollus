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

    public ShapeBatch(in ShapeBatchKey key)
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
    protected override ShapeBatch CreateBatch(in ShapeBatchKey key)
    {
        return new ShapeBatch(key);
    }
}

public class ShapeBatchDraw
{
    public RenderStep2D Stage => RenderStep2D.Main;

    public void Render(GPURenderPassEncoder encoder, Resources resources, RenderAssets renderAssets)
    {
        var batches = resources.Get<ShapeBatches>();

        foreach (var batch in batches.Batches)
        {
            if (batch.IsEmpty) continue;

            var material = renderAssets.Get<MaterialRenderData>(batch.Material);
            var shape = renderAssets.Get<ShapeRenderData>(batch.Shape);

            var draw = new Draw()
            {
                Pipeline = material.Pipeline,
                VertexCount = shape.VertexCount,
                InstanceCount = (uint)batch.Count,
            };
            material.BindGroups.CopyTo(draw.BindGroups);
            draw.VertexBuffers[0] = shape.VertexBuffer;
            draw.VertexBuffers[1] = batch.InstanceBufferHandle;
        }
    }
}