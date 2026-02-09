namespace Pollus.Engine.Rendering;

using Pollus.Graphics;
using Pollus.Mathematics;
using Pollus.Utils;

public readonly record struct SpriteBatchKey(Handle Material, bool IsStatic, RenderStep2D RenderStep = RenderStep2D.Main)
{
    public int SortKey { get; } = RenderingUtils.PackSortKeys(Material.ID, IsStatic ? 1 : 0);
}

public partial class SpriteBatch : RenderBatch<SpriteBatch.InstanceData>
{
    [ShaderType]
    public partial struct InstanceData
    {
        public required Vec4f Model0;
        public required Vec4f Model1;
        public required Vec4f Model2;
        public required Vec4f Slice;
        public required Vec4f Color;
    }

    public Handle Material { get; init; }
    public override Handle[] RequiredResources { get; }

    public SpriteBatch(in SpriteBatchKey key) : base(key.SortKey)
    {
        Material = key.Material;
        IsStatic = key.IsStatic;
        RenderStep = (int)key.RenderStep;
        RequiredResources = [Material];
    }
}

public class SpriteBatches : RenderBatches<SpriteBatch, SpriteBatchKey>
{
    public override Draw GetDrawCall(int batchID, int start, int count, IRenderAssets renderAssets)
    {
        var batch = GetBatch(batchID);
        if (!batch.HasRequiredResources(renderAssets)) return Draw.Empty;
        var material = renderAssets.Get<MaterialRenderData>(batch.Material);
        return Draw.Create(material.Pipeline)
            .SetVertexInfo(6, 0)
            .SetInstanceInfo((uint)count, (uint)start)
            .SetVertexBuffer(0, batch.InstanceBufferHandle)
            .SetBindGroups(material.BindGroups);
    }

    protected override SpriteBatch CreateBatch(in SpriteBatchKey key)
    {
        return new SpriteBatch(key);
    }
}