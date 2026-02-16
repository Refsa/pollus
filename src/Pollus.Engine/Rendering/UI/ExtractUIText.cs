namespace Pollus.Engine.Rendering;

using System.Collections.Generic;
using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;

[SystemSet]
public partial class ExtractUITextSystem
{
    class LocalData
    {
        public List<(Entity entity, Vec2f parentAbsPos, RectInt? scissor)> Deferred = new();
    }

    [System(nameof(ExtractUIText))]
    public static readonly SystemBuilderDescriptor ExtractUITextDescriptor = new()
    {
        Stage = CoreStage.PreRender,
        RunsAfter = [ExtractUIRectsSystem.ExtractUIRectsDescriptor.Label],
        Locals = [Local.From(new LocalData())],
    };

    static void ExtractUIText(
        Local<LocalData> localData,
        UIFontBatches batches,
        Query query,
        Query<UILayoutRoot, ComputedNode>.Filter<All<UINode>> qRoots)
    {
        batches.Reset();

        uint sortIndex = 0;

        foreach (var root in qRoots)
        {
            var rootEntity = root.Entity;
            ref readonly var rootComputed = ref root.Component1;

            EmitNode(batches, ref sortIndex, query, rootEntity, rootComputed, Vec2f.Zero, null, localData.Value.Deferred);
        }

        foreach (var (deferredEntity, parentAbsPos, deferredScissor) in localData.Value.Deferred)
        {
            var entRef = query.GetEntity(deferredEntity);
            if (entRef.Has<ComputedNode>())
            {
                ref var computed = ref entRef.Get<ComputedNode>();
                EmitNode(batches, ref sortIndex, query, deferredEntity, computed, parentAbsPos, deferredScissor, null);
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

    static void EmitNode(UIFontBatches batches, ref uint sortIndex, Query query, Entity entity, in ComputedNode computed, Vec2f parentAbsPos, RectInt? scissor, List<(Entity entity, Vec2f parentAbsPos, RectInt? scissor)>? deferred)
    {
        var absPos = parentAbsPos + computed.Position;
        var size = computed.Size;
        var nodeIndex = sortIndex++;
        var entRef = query.GetEntity(entity);

        if (size.X > 0 && size.Y > 0)
        {
            // Emit text if entity has UIText + TextMesh + UITextFont
            if (entRef.Has<UIText>() && entRef.Has<TextMesh>() && entRef.Has<UITextFont>())
            {
                ref readonly var textMesh = ref entRef.Get<TextMesh>();
                ref readonly var textFont = ref entRef.Get<UITextFont>();
                ref readonly var uiText = ref entRef.Get<UIText>();

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
