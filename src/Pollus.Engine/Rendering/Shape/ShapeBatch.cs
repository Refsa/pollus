namespace Pollus.Engine.Rendering;

using Pollus.Graphics;
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
        public Vec4f Model0;
        public Vec4f Model1;
        public Vec4f Model2;
        public Vec4f Color;
    }

    public Handle Material { get; }
    public Handle<Shape> Shape { get; }
    public override Handle[] RequiredResources => [Material, Shape];

    public ShapeBatch(in ShapeBatchKey key) : base(key.GetHashCode())
    {
        Material = key.Material;
        Shape = key.Shape;
    }

    public void Draw(ulong sortKey, Mat4f model, Color color)
    {
        var tModel = model.Transpose();
        base.Draw(sortKey, new InstanceData()
        {
            Model0 = tModel.Col0,
            Model1 = tModel.Col1,
            Model2 = tModel.Col2,
            Color = color,
        });
    }
}

public class ShapeBatches : RenderBatches<ShapeBatch, ShapeBatchKey>
{
    public override Draw GetDrawCall(int batchID, int start, int count, IRenderAssets renderAssets)
    {
        var batch = GetBatch(batchID);
        var material = renderAssets.Get<MaterialRenderData>(batch.Material);
        var shape = renderAssets.Get<ShapeRenderData>(batch.Shape);

        return Draw.Create(material.Pipeline)
            .SetVertexInfo((uint)shape.VertexCount, 0)
            .SetInstanceInfo((uint)count, (uint)start)
            .SetVertexBuffer(0, shape.VertexBuffer)
            .SetVertexBuffer(1, batch.InstanceBufferHandle)
            .SetBindGroups(material.BindGroups);
    }

    protected override ShapeBatch CreateBatch(in ShapeBatchKey key)
    {
        return new ShapeBatch(key);
    }
}