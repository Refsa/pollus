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
    public override Handle[] RequiredResources => [Material, Shape];

    public ShapeBatch(in ShapeBatchKey key) : base(key.GetHashCode())
    {
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