namespace Pollus.Engine.Rendering;

using Pollus.Graphics;
using Pollus.Mathematics;
using Pollus.Utils;

public record struct SpriteBatchKey(Handle Material);

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

    public SpriteBatch(in SpriteBatchKey key)
    {
        Key = key.GetHashCode();
        Material = key.Material;
    }
}

public class SpriteBatches : RenderBatches<SpriteBatch, SpriteBatchKey>
{
    protected override SpriteBatch CreateBatch(in SpriteBatchKey key)
    {
        return new SpriteBatch(key);
    }
}