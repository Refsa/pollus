namespace Pollus.Engine.Rendering;

using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Mathematics;
using Pollus.Utils;

public readonly record struct UIRectBatchKey(Handle Material, Handle Texture, Handle Sampler, RectInt? ScissorRect = null, RenderStep2D RenderStep = RenderStep2D.UI)
{
    public int SortKey { get; } = RenderingUtils.PackSortKeys(Material.ID, Texture.ID);
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
        public required Vec4f Extra; // x=ShapeType (0=RoundedRect, 1=Circle, 2=Checkmark, 3=DownArrow), y=OutlineWidth, z=OutlineOffset, w=TextMode (>0.5=glyph)
        public required Vec4f OutlineColor;
        public required Vec4f UVRect; // minU, minV, sizeU, sizeV
    }

    public Handle Material { get; init; }
    public Handle Texture { get; init; }
    public Handle Sampler { get; init; }
    public RectInt? ScissorRect { get; init; }
    public override Handle[] RequiredResources { get; }

    public UIRectBatch(in UIRectBatchKey key) : base(key.SortKey)
    {
        Material = key.Material;
        Texture = key.Texture;
        Sampler = key.Sampler;
        ScissorRect = key.ScissorRect;
        RenderStep = (int)key.RenderStep;
        RequiredResources = Texture.IsNull() ? [Material] : [Material, Texture];
    }
}

public class UIRectBatches : RenderBatches<UIRectBatch, UIRectBatchKey>
{
    readonly Dictionary<(Handle, Handle), Handle<GPUBindGroup>> bindGroupCache = new();

    public bool HasBindGroup(Handle texture, Handle sampler) => bindGroupCache.ContainsKey((texture, sampler));

    public void CacheBindGroup(Handle texture, Handle sampler, Handle<GPUBindGroup> bindGroup)
    {
        bindGroupCache[(texture, sampler)] = bindGroup;
    }

    public override Draw GetDrawCall(int batchID, int start, int count, IRenderAssets renderAssets)
    {
        var batch = GetBatch(batchID);
        if (!batch.HasRequiredResources(renderAssets)) return Draw.Empty;
        var material = renderAssets.Get<MaterialRenderData>(batch.Material);
        var draw = Draw.Create(material.Pipeline)
            .SetVertexInfo(6, 0)
            .SetInstanceInfo((uint)count, (uint)start)
            .SetVertexBuffer(0, batch.InstanceBufferHandle)
            .SetBindGroups(material.BindGroups)
            .SetScissorRect(batch.ScissorRect);

        if (!batch.Texture.IsNull() && bindGroupCache.TryGetValue((batch.Texture, batch.Sampler), out var bindGroup))
        {
            draw = draw.SetBindGroup(0, bindGroup);
        }

        return draw;
    }

    protected override UIRectBatch CreateBatch(in UIRectBatchKey key)
    {
        return new UIRectBatch(key);
    }
}
