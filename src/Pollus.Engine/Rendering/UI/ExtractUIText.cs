namespace Pollus.Engine.Rendering;

using System.Collections.Generic;
using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;

public static class ExtractUITextSystem
{
    public const string Label = "ExtractUIText";

    public static ISystemBuilder Create() => FnSystem.Create(
        new(Label)
        {
            RunsAfter = [ExtractUIRectsSystem.Label],
        },
        static (
            UIFontBatches batches,
            Query query,
            Query<UILayoutRoot, ComputedNode>.Filter<All<UINode>> qRoots) =>
        {
            batches.Reset();

            uint sortIndex = 0;
            var deferred = new List<(Entity entity, Vec2f parentAbsPos, RectInt? scissor)>();

            foreach (var root in qRoots)
            {
                var rootEntity = root.Entity;
                ref readonly var rootComputed = ref root.Component1;

                EmitNode(batches, ref sortIndex, query, rootEntity, rootComputed, Vec2f.Zero, null, deferred);
            }

            // Render deferred absolute-positioned text on top of normal flow
            foreach (var (deferredEntity, parentAbsPos, deferredScissor) in deferred)
            {
                var entRef = query.GetEntity(deferredEntity);
                if (entRef.Has<ComputedNode>())
                {
                    ref var computed = ref entRef.Get<ComputedNode>();
                    EmitNode(batches, ref sortIndex, query, deferredEntity, computed, parentAbsPos, deferredScissor, null);
                }
            }
        }
    );

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

    static void EmitNode(UIFontBatches batches, ref uint sortIndex, Query query, Entity entity, in ComputedNode computed, Vec2f parentAbsPos, RectInt? scissor, List<(Entity entity, Vec2f parentAbsPos, RectInt? scissor)>? deferred)
    {
        var absPos = parentAbsPos + computed.Position;
        var size = computed.Size;
        var nodeIndex = sortIndex++;

        if (size.X > 0 && size.Y > 0)
        {
            var entityRef = query.GetEntity(entity);

            // Emit text if entity has UIText + TextMesh + UITextFont
            if (entityRef.Has<UIText>() && entityRef.Has<TextMesh>() && entityRef.Has<UITextFont>())
            {
                ref readonly var textMesh = ref entityRef.Get<TextMesh>();
                ref readonly var textFont = ref entityRef.Get<UITextFont>();
                ref readonly var uiText = ref entityRef.Get<UIText>();

                if (textMesh.Mesh != Handle<TextMeshAsset>.Null && textFont.Material != Handle.Null)
                {
                    var batchKey = new UIFontBatchKey(textMesh.Mesh, textFont.Material, scissor);
                    var batch = batches.GetOrCreate(batchKey);
                    var sortKey = (ulong)nodeIndex * 4 + 3;

                    // Content offset: text starts inside padding+border
                    var contentOffset = new Vec2f(
                        computed.PaddingLeft + computed.BorderLeft,
                        computed.PaddingTop + computed.BorderTop);

                    batch.Draw(sortKey, absPos + contentOffset, uiText.Color);
                }
            }
        }

        // Skip children of zero-size nodes
        if (size.X <= 0 && size.Y <= 0) return;

        var entRef = query.GetEntity(entity);
        if (!entRef.Has<Parent>()) return;

        // Compute scissor rect for children if this node has overflow != Visible
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

        // Apply scroll offset for children
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
                    EmitNode(batches, ref sortIndex, query, childEntity, childComputed, childAbsPos, childScissor, deferred);
                }
            }

            if (childRef.Has<Child>())
                childEntity = childRef.Get<Child>().NextSibling;
            else
                break;
        }
    }
}
