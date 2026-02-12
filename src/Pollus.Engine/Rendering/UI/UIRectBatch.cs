namespace Pollus.Engine.Rendering;

using Pollus.Graphics;
using Pollus.Mathematics;
using Pollus.Utils;

public readonly record struct UIRectBatchKey(Handle Material, RenderStep2D RenderStep = RenderStep2D.UI)
{
    public int SortKey { get; } = RenderingUtils.PackSortKeys(Material.ID, 0);
}

public partial class UIRectBatch : RenderBatch<UIRectBatch.InstanceData>
{
    [ShaderType]
    public partial struct InstanceData
    {
        public required Vec4f PosSize;
        public required Vec4f BackgroundColor;
        public required Vec4f BorderColor;
        public required Vec4f BorderRadius;
        public required Vec4f BorderWidths;
        public required Vec4f Extra; // x=ShapeType (0=RoundedRect, 1=Circle, 2=Checkmark, 3=DownArrow), yzw=reserved
    }

    public Handle Material { get; init; }
    public override Handle[] RequiredResources { get; }

    public UIRectBatch(in UIRectBatchKey key) : base(key.SortKey)
    {
        Material = key.Material;
        RenderStep = (int)key.RenderStep;
        RequiredResources = [Material];
    }
}

public class UIRectBatches : RenderBatches<UIRectBatch, UIRectBatchKey>
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

    protected override UIRectBatch CreateBatch(in UIRectBatchKey key)
    {
        return new UIRectBatch(key);
    }
}
