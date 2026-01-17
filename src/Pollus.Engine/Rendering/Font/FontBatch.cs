namespace Pollus.Engine.Rendering;

using Graphics;
using Mathematics;
using Utils;

public record struct FontBatchKey(Handle<TextMeshAsset> TextMesh, Handle Material, RenderStep2D RenderStep = RenderStep2D.Main)
{
    public int SortKey { get; } = RenderingUtils.PackSortKeys(TextMesh.ID, Material.ID);
}

public partial class FontBatch : RenderBatch<FontBatch.InstanceData>
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
    public Handle<TextMeshAsset> TextMesh { get; }
    public override Handle[] RequiredResources { get; }

    public FontBatch(in FontBatchKey key) : base(key.SortKey)
    {
        Material = key.Material;
        TextMesh = key.TextMesh;
        RenderStep = (int)key.RenderStep;
        RequiredResources = [Material, TextMesh];
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

public class FontBatches : RenderBatches<FontBatch, FontBatchKey>
{
    public override Draw GetDrawCall(int batchID, int start, int count, IRenderAssets renderAssets)
    {
        var batch = GetBatch(batchID);
        if (!batch.HasRequiredResources(renderAssets)) return Draw.Empty;

        var material = renderAssets.Get<MaterialRenderData>(batch.Material);
        var textMesh = renderAssets.Get<FontMeshRenderData>(batch.TextMesh);

        return Draw.Create(material.Pipeline)
            .SetVertexInfo(textMesh.VertexCount, 0)
            .SetInstanceInfo((uint)count, (uint)start)
            .SetVertexBuffer(0, textMesh.VertexBuffer)
            .SetVertexBuffer(1, batch.InstanceBufferHandle)
            .SetIndexBuffer(textMesh.IndexBuffer, textMesh.IndexFormat, (uint)textMesh.IndexCount, 0)
            .SetBindGroups(material.BindGroups);
    }

    protected override FontBatch CreateBatch(in FontBatchKey key)
    {
        return new FontBatch(key);
    }
}