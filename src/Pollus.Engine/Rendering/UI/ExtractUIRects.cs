namespace Pollus.Engine.Rendering;

using System.Collections.Generic;
using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;

public record struct UIRenderResources
{
    public Handle<UIRectMaterial> Material;
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
        RunsAfter = [RenderingPlugin.BeginFrameSystem],
        Locals = [Local.From(new LocalData())],
    };

    static void ExtractUIRects(
        Local<LocalData> localData,
        UIRectBatches batches,
        UIRenderResources resources,
        Query query,
        Query<UILayoutRoot, ComputedNode>.Filter<All<UINode>> qRoots)
    {
        batches.Reset();

        uint sortIndex = 0;

        foreach (var root in qRoots)
        {
            var rootEntity = root.Entity;
            ref readonly var rootComputed = ref root.Component1;

            EmitNode(batches, resources.Material, ref sortIndex, query, rootEntity, rootComputed, Vec2f.Zero, null, localData.Value.Deferred);
        }

        foreach (var (deferredEntity, parentAbsPos, deferredScissor) in localData.Value.Deferred)
        {
            var entRef = query.GetEntity(deferredEntity);
            if (entRef.Has<ComputedNode>())
            {
                ref var computed = ref entRef.Get<ComputedNode>();
                EmitNode(batches, resources.Material, ref sortIndex, query, deferredEntity, computed, parentAbsPos, deferredScissor, null);
            }
        }

        localData.Value.Deferred.Clear();
    }

    static RectInt IntersectScissorRects(RectInt a, RectInt b)
    {
        var left = Math.Max(a.Min.X, b.Min.X);
        var top = Math.Max(a.Min.Y, b.Min.Y);
        var right = Math.Min(a.Max.X, b.Max.X);
        var bottom = Math.Min(a.Max.Y, b.Max.Y);
        return new RectInt(left, top, Math.Max(left, right), Math.Max(top, bottom));
    }

    static RectInt? ComputeChildScissor(RectInt? parentScissor, Vec2f absPos, Vec2f size)
    {
        var nodeRect = new RectInt(
            (int)absPos.X, (int)absPos.Y,
            (int)(absPos.X + size.X), (int)(absPos.Y + size.Y));

        if (parentScissor.HasValue)
            return IntersectScissorRects(parentScissor.Value, nodeRect);
        return nodeRect;
    }

    static void EmitNode(UIRectBatches batches, Handle<UIRectMaterial> material, ref uint sortIndex, Query query, Entity entity, in ComputedNode computed, Vec2f parentAbsPos, RectInt? scissor,
        List<(Entity entity, Vec2f parentAbsPos, RectInt? scissor)>? deferred)
    {
        var absPos = parentAbsPos + computed.Position;
        var size = computed.Size;
        var nodeIndex = sortIndex++;
        var entRef = query.GetEntity(entity);

        if (size is { X: > 0, Y: > 0 })
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

            if (hasBg || hasBorder)
            {
                var batchKey = new UIRectBatchKey(material, scissor);
                var batch = batches.GetOrCreate(batchKey);
                var sortKey = (ulong)nodeIndex * 4;
                batch.Draw(sortKey, new UIRectBatch.InstanceData
                {
                    PosSize = new Vec4f(absPos.X, absPos.Y, size.X, size.Y),
                    BackgroundColor = bgColor,
                    BorderColor = borderColor,
                    BorderRadius = borderRadius,
                    BorderWidths = borderWidths,
                    Extra = new Vec4f(shapeType, 0f, 0f, 0f),
                });
            }
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
                childScissor = ComputeChildScissor(scissor, absPos, size);
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
                    EmitNode(batches, material, ref sortIndex, query, childEntity, childComputed, childAbsPos, childScissor, deferred);
                }
            }

            if (childRef.Has<Child>())
                childEntity = childRef.Get<Child>().NextSibling;
            else
                break;
        }
    }
}
