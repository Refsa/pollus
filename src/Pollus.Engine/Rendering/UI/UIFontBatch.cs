namespace Pollus.Engine.Rendering;

using Pollus.Graphics;
using Pollus.Mathematics;
using Pollus.Utils;

public record struct UIFontBatchKey(Handle<TextMeshAsset> TextMesh, Handle Material, RenderStep2D RenderStep = RenderStep2D.UI)
{
    public int SortKey { get; } = RenderingUtils.PackSortKeys(TextMesh.ID, Material.ID);
}

public partial class UIFontBatch : RenderBatch<UIFontBatch.InstanceData>
{
    [ShaderType]
    public partial struct InstanceData
    {
        public Vec4f Offset;
        public Vec4f Color;
    }

    public Handle Material { get; }
    public Handle<TextMeshAsset> TextMesh { get; }
    public override Handle[] RequiredResources { get; }

    public UIFontBatch(in UIFontBatchKey key) : base(key.SortKey)
    {
        Material = key.Material;
        TextMesh = key.TextMesh;
        RenderStep = (int)key.RenderStep;
        RequiredResources = [Material, TextMesh];
    }

    public void Draw(ulong sortKey, Vec2f offset, Color color)
    {
        base.Draw(sortKey, new InstanceData()
        {
            Offset = new Vec4f(offset.X, offset.Y, 0f, 0f),
            Color = color,
        });
    }
}

public class UIFontBatches : RenderBatches<UIFontBatch, UIFontBatchKey>
{
    public override Draw GetDrawCall(int batchID, int start, int count, IRenderAssets renderAssets)
    {
        var batch = GetBatch(batchID);
        if (!batch.HasRequiredResources(renderAssets)) return Draw.Empty;

        var material = renderAssets.Get<MaterialRenderData>(batch.Material);
        var textMesh = renderAssets.Get<FontMeshRenderData>(batch.TextMesh);
        if (textMesh.VertexCount == 0 || textMesh.IndexCount == 0) return Draw.Empty;

        return Draw.Create(material.Pipeline)
            .SetVertexInfo(textMesh.VertexCount, 0)
            .SetInstanceInfo((uint)count, (uint)start)
            .SetVertexBuffer(0, textMesh.VertexBuffer)
            .SetVertexBuffer(1, batch.InstanceBufferHandle)
            .SetIndexBuffer(textMesh.IndexBuffer, textMesh.IndexFormat, (uint)textMesh.IndexCount, 0)
            .SetBindGroups(material.BindGroups);
    }

    protected override UIFontBatch CreateBatch(in UIFontBatchKey key)
    {
        return new UIFontBatch(key);
    }
}
