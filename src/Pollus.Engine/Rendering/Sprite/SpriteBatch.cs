namespace Pollus.Engine.Rendering;

using Pollus.Graphics;
using Pollus.Mathematics;
using Pollus.Utils;

public readonly record struct SpriteBatchKey(Handle Material, bool IsStatic);

public partial class SpriteBatch : RenderBatch<SpriteBatch.InstanceData>
{
    [ShaderType]
    public partial struct InstanceData
    {
        public required Vec4f Model_0;
        public required Vec4f Model_1;
        public required Vec4f Model_2;
        public required Vec4f Slice;
        public required Vec4f Color;
    }

    public Handle Material { get; init; }
    public override Handle[] RequiredResources => [Material];

    public SpriteBatch(in SpriteBatchKey key) : base(key.GetHashCode())
    {
        Material = key.Material;
        IsStatic = key.IsStatic;
    }
}

public class SpriteBatches : RenderBatches<SpriteBatch, SpriteBatchKey>
{
    protected override SpriteBatch CreateBatch(in SpriteBatchKey key)
    {
        return new SpriteBatch(key);
    }
}