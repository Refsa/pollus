namespace Pollus.Engine.Rendering;

using System.Collections.Generic;
using Pollus.Assets;
using Pollus.ECS;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;

public record struct UIRenderResources
{
    public Handle<UIRectMaterial> Material;
    public Handle<SamplerAsset> DefaultSampler;
    public Handle<SamplerAsset> LinearSampler;
}

[SystemSet]
public partial class ExtractUIRectsSystem
{
    class LocalData
    {
        public List<(Entity entity, Vec2f parentAbsPos, RectInt? scissor)> Deferred = new();
    }

    [System(nameof(ExtractUIRects))]
    public static readonly SystemBuilderDescriptor ExtractUIRectsDescriptor = new()
    {
        Stage = CoreStage.PreRender,
        RunsAfter = [RenderingPlugin.BeginFrameSystem, "UITextSystemSet::PropagateUIFontAtlas"],
        Locals = [Local.From(new LocalData())],
    };

    static void ExtractUIRects(
        Local<LocalData> localData,
        UIRectBatches batches,
        UIRenderResources resources,
        Assets<TextMeshAsset> meshes,
        Query query,
        Query<UILayoutRoot, ComputedNode>.Filter<All<UINode>> qRoots)
    {
        batches.Reset();

        uint sortIndex = 0;

        foreach (var root in qRoots)
        {
            var rootEntity = root.Entity;
            ref readonly var rootComputed = ref root.Component1;

            EmitNode(batches, resources.Material, meshes, ref sortIndex, query, rootEntity, rootComputed, Vec2f.Zero, null, localData.Value.Deferred);
        }

        foreach (var (deferredEntity, parentAbsPos, deferredScissor) in localData.Value.Deferred)
        {
            var entRef = query.GetEntity(deferredEntity);
            if (entRef.Has<ComputedNode>())
            {
                ref var computed = ref entRef.Get<ComputedNode>();
                EmitNode(batches, resources.Material, meshes, ref sortIndex, query, deferredEntity, computed, parentAbsPos, deferredScissor, null);
            }
        }

        localData.Value.Deferred.Clear();
    }

    [System(nameof(PrepareUIImageBindGroups))]
    public static readonly SystemBuilderDescriptor PrepareUIImageBindGroupsDescriptor = new()
    {
        Stage = CoreStage.PreRender,
        RunsAfter = [nameof(ExtractUIRects)],
    };

    static void PrepareUIImageBindGroups(
        UIRectBatches batches,
        UIRenderResources resources,
        RenderAssets renderAssets,
        IWGPUContext gpuContext,
        AssetServer assetServer)
    {
        foreach (scoped ref readonly var batch in batches.Batches)
        {
            if (batch is not UIRectBatch rectBatch) continue;
            if (rectBatch.Texture.IsNull()) continue;
            if (batches.HasBindGroup(rectBatch.Texture, rectBatch.Sampler)) continue;

            renderAssets.Prepare(gpuContext, assetServer, rectBatch.Texture);
            if (!renderAssets.Has(rectBatch.Texture)) continue;

            var samplerHandle = rectBatch.Sampler.IsNull() ? (Handle)resources.DefaultSampler : rectBatch.Sampler;

            var uniformBinding = new UniformBinding<UIUniform>();
            var textureBinding = new TextureBinding { Image = rectBatch.Texture };
            var samplerBinding = new SamplerBinding { Sampler = samplerHandle };

            IBinding[] bindings = [uniformBinding, textureBinding, samplerBinding];

            using var layout = gpuContext.CreateBindGroupLayout(new()
            {
                Label = "ui-rect-image-bind-group-layout",
                Entries = [.. bindings.Select((b, i) => b.Layout((uint)i))],
            });

            var bindGroup = gpuContext.CreateBindGroup(new()
            {
                Label = "ui-rect-image-bind-group",
                Layout = layout,
                Entries = [.. bindings.Select((b, i) => b.Binding(renderAssets, gpuContext, assetServer, (uint)i))],
            });

            batches.CacheBindGroup(rectBatch.Texture, rectBatch.Sampler, renderAssets.Add(bindGroup));
        }
    }

    static RectInt IntersectScissorRects(RectInt a, RectInt b)
    {
        var left = Math.Max(a.Min.X, b.Min.X);
        var top = Math.Max(a.Min.Y, b.Min.Y);
        var right = Math.Min(a.Max.X, b.Max.X);
        var bottom = Math.Min(a.Max.Y, b.Max.Y);
        return new RectInt(left, top, Math.Max(left, right), Math.Max(top, bottom));
    }

    static RectInt? ComputeChildScissor(RectInt? parentScissor, Vec2f absPos, Vec2f size, in ComputedNode computed)
    {
        var nodeRect = new RectInt(
            (int)(absPos.X + computed.BorderLeft),
            (int)(absPos.Y + computed.BorderTop),
            (int)(absPos.X + size.X - computed.BorderRight),
            (int)(absPos.Y + size.Y - computed.BorderBottom));

        if (parentScissor.HasValue)
            return IntersectScissorRects(parentScissor.Value, nodeRect);
        return nodeRect;
    }

    const int SortSlotsPerNode = 4;
    const int TextSortOffset = 3;

    static void EmitNode(UIRectBatches batches, Handle<UIRectMaterial> material, Assets<TextMeshAsset> meshes, ref uint sortIndex, Query query, Entity entity, in ComputedNode computed, Vec2f parentAbsPos, RectInt? scissor,
        List<(Entity entity, Vec2f parentAbsPos, RectInt? scissor)>? deferred)
    {
        var absPos = parentAbsPos + computed.Position;
        var size = computed.Size;
        var nodeIndex = sortIndex++;
        var entRef = query.GetEntity(entity);

        var effectiveMaterial = material;
        if (entRef.Has<UIMaterial>())
        {
            var overrideMat = entRef.Get<UIMaterial>().Material;
            if (!overrideMat.IsNull())
                effectiveMaterial = overrideMat;
        }

        if (size is { X: > 0, Y: > 0 })
        {
            EmitRectBackground(batches, effectiveMaterial, nodeIndex, entRef, absPos, size, computed, scissor);
            EmitTextGlyphs(batches, effectiveMaterial, meshes, nodeIndex, entRef, absPos, computed, scissor);
        }

        if (size is { X: <= 0, Y: <= 0 }) return;
        if (!entRef.Has<Parent>()) return;

        var childScissor = scissor;
        var childAbsPos = absPos;
        if (entRef.Has<UIStyle>())
        {
            ref readonly var style = ref entRef.Get<UIStyle>();
            if (style.Value.Overflow.X != Overflow.Visible || style.Value.Overflow.Y != Overflow.Visible)
            {
                childScissor = ComputeChildScissor(scissor, absPos, size, computed);
            }
        }

        if (entRef.Has<UIScrollOffset>())
        {
            ref readonly var scroll = ref entRef.Get<UIScrollOffset>();
            childAbsPos = absPos - scroll.Offset;
        }

        var childEntity = entRef.Get<Parent>().FirstChild;
        while (!childEntity.IsNull)
        {
            var childRef = query.GetEntity(childEntity);
            if (childRef.Has<ComputedNode>())
            {
                if (deferred != null && childRef.Has<UIStyle>()
                                     && childRef.Get<UIStyle>().Value.Position == Position.Absolute)
                {
                    deferred.Add((childEntity, childAbsPos, childScissor));
                }
                else
                {
                    ref var childComputed = ref childRef.Get<ComputedNode>();
                    EmitNode(batches, material, meshes, ref sortIndex, query, childEntity, childComputed, childAbsPos, childScissor, deferred);
                }
            }

            if (childRef.Has<Child>())
                childEntity = childRef.Get<Child>().NextSibling;
            else
                break;
        }
    }

    static void EmitRectBackground(
        UIRectBatches batches,
        Handle<UIRectMaterial> material,
        uint nodeIndex,
        EntityRef entRef,
        Vec2f absPos,
        Vec2f size,
        in ComputedNode computed,
        RectInt? scissor)
    {
        var bgColor = Vec4f.Zero;
        var borderColor = Vec4f.Zero;
        var borderRadius = Vec4f.Zero;
        var borderWidths = new Vec4f(computed.BorderTop, computed.BorderRight, computed.BorderBottom, computed.BorderLeft);

        bool hasBg = false;
        bool hasBorder = false;

        if (entRef.Has<BackgroundColor>())
        {
            var bg = entRef.Get<BackgroundColor>();
            bgColor = bg.Color;
            hasBg = true;
        }

        if (entRef.Has<BorderColor>())
        {
            var bc = entRef.Get<BorderColor>();
            borderColor = ((Vec4f)bc.Top + (Vec4f)bc.Right + (Vec4f)bc.Bottom + (Vec4f)bc.Left) * 0.25f;
            hasBorder = true;
        }

        if (entRef.Has<BorderRadius>())
        {
            var br = entRef.Get<BorderRadius>();
            borderRadius = new Vec4f(br.TopLeft, br.TopRight, br.BottomRight, br.BottomLeft);
        }

        float shapeType = 0f;
        if (entRef.Has<UIShape>())
        {
            shapeType = (float)entRef.Get<UIShape>().Type;
        }

        float outlineWidth = 0f;
        float outlineOffset = 0f;
        var outlineColor = Vec4f.Zero;
        bool hasOutline = false;
        if (entRef.Has<Outline>())
        {
            var outline = entRef.Get<Outline>();
            outlineWidth = outline.Width;
            outlineOffset = outline.Offset;
            outlineColor = outline.Color;
            hasOutline = outlineWidth > 0f && outline.Color.A > 0f;
        }

        var uvRect = new Vec4f(0, 0, 1, 1);
        var texture = Handle.Null;
        var sampler = Handle.Null;

        if (entRef.Has<UIImage>())
        {
            var image = entRef.Get<UIImage>();
            texture = image.Texture;
            sampler = image.Sampler;
            var slice = image.Slice;
            uvRect = new Vec4f(slice.Min.X, slice.Min.Y, slice.Width, slice.Height);
            if (!hasBg) { bgColor = Color.WHITE; hasBg = true; }
        }

        if (hasBg || hasBorder || hasOutline)
        {
            var batchKey = new UIRectBatchKey(material, texture, sampler, scissor);
            var batch = batches.GetOrCreate(batchKey);
            batch.Draw((ulong)nodeIndex * SortSlotsPerNode, new UIRectBatch.InstanceData
            {
                PosSize = new Vec4f(absPos.X, absPos.Y, size.X, size.Y),
                BackgroundColor = bgColor,
                BorderColor = borderColor,
                BorderRadius = borderRadius,
                BorderWidths = borderWidths,
                Extra = new Vec4f(shapeType, outlineWidth, outlineOffset, 0f),
                OutlineColor = outlineColor,
                UVRect = uvRect,
            });
        }
    }

    static void EmitTextGlyphs(
        UIRectBatches batches,
        Handle<UIRectMaterial> material,
        Assets<TextMeshAsset> meshes,
        uint nodeIndex,
        EntityRef entRef,
        Vec2f absPos,
        in ComputedNode computed,
        RectInt? scissor)
    {
        if (!entRef.Has<UIText>() || !entRef.Has<TextMesh>() || !entRef.Has<UITextFont>()) return;

        ref readonly var textMesh = ref entRef.Get<TextMesh>();
        ref readonly var textFont = ref entRef.Get<UITextFont>();
        ref readonly var uiText = ref entRef.Get<UIText>();

        if (textMesh.Mesh == Handle<TextMeshAsset>.Null || textFont.Atlas.IsNull()) return;

        var tma = meshes.Get(textMesh.Mesh);
        if (tma == null || tma.Vertices.Count == 0) return;

        var contentOffset = new Vec2f(
            computed.PaddingLeft + computed.BorderLeft,
            computed.PaddingTop + computed.BorderTop);
        var textOrigin = absPos + contentOffset;
        var textColor = (Vec4f)uiText.Color;
        var textBatchKey = new UIRectBatchKey(material, textFont.Atlas, textFont.Sampler, scissor);
        var textBatch = batches.GetOrCreate(textBatchKey);

        // 4 vertices per glyph: [0]=TL, [1]=TR, [2]=BL, [3]=BR
        var verts = tma.Vertices.AsSpan();
        for (int g = 0; g + 3 < verts.Length; g += 4)
        {
            ref readonly var tl = ref verts[g];
            ref readonly var br = ref verts[g + 3];
            var glyphSize = new Vec2f(br.Position.X - tl.Position.X, br.Position.Y - tl.Position.Y);
            if (glyphSize.X <= 0 || glyphSize.Y <= 0) continue;

            textBatch.Draw((ulong)nodeIndex * SortSlotsPerNode + TextSortOffset, new UIRectBatch.InstanceData
            {
                PosSize = new Vec4f(textOrigin.X + tl.Position.X, textOrigin.Y + tl.Position.Y,
                                    glyphSize.X, glyphSize.Y),
                BackgroundColor = textColor,
                BorderColor = Vec4f.Zero,
                BorderRadius = Vec4f.Zero,
                BorderWidths = Vec4f.Zero,
                Extra = new Vec4f(0f, 0f, 0f, 1f),
                OutlineColor = Vec4f.Zero,
                UVRect = new Vec4f(tl.UV.X, tl.UV.Y,
                                   br.UV.X - tl.UV.X, br.UV.Y - tl.UV.Y),
            });
        }
    }
}
