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
    [System(nameof(ExtractUIRects))]
    public static readonly SystemBuilderDescriptor ExtractUIRectsDescriptor = new()
    {
        Stage = CoreStage.PreRender,
        RunsAfter = [RenderingPlugin.BeginFrameSystem],
    };

    static void ExtractUIRects(
        UIRectBatches batches,
        UIRenderResources resources,
        Query query,
        Query<UILayoutRoot, ComputedNode>.Filter<All<UINode>> qRoots)
    {
        batches.Reset();

        uint sortIndex = 0;
        var deferred = new List<(Entity entity, Vec2f parentAbsPos, RectInt? scissor)>();

        foreach (var root in qRoots)
        {
            var rootEntity = root.Entity;
            ref readonly var rootComputed = ref root.Component1;

            EmitNode(batches, resources.Material, ref sortIndex, query, rootEntity, rootComputed, Vec2f.Zero, null, deferred);
        }

        // Render deferred absolute-positioned nodes on top of normal flow
        foreach (var (deferredEntity, parentAbsPos, deferredScissor) in deferred)
        {
            var entRef = query.GetEntity(deferredEntity);
            if (entRef.Has<ComputedNode>())
            {
                ref var computed = ref entRef.Get<ComputedNode>();
                EmitNode(batches, resources.Material, ref sortIndex, query, deferredEntity, computed, parentAbsPos, deferredScissor, null);
            }
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

    static RectInt? ComputeChildScissor(RectInt? parentScissor, Vec2f absPos, Vec2f size)
    {
        var nodeRect = new RectInt(
            (int)absPos.X, (int)absPos.Y,
            (int)(absPos.X + size.X), (int)(absPos.Y + size.Y));

        if (parentScissor.HasValue)
            return IntersectScissorRects(parentScissor.Value, nodeRect);
        return nodeRect;
    }

    static void EmitNode(UIRectBatches batches, Handle<UIRectMaterial> material, ref uint sortIndex, Query query, Entity entity, in ComputedNode computed, Vec2f parentAbsPos, RectInt? scissor, List<(Entity entity, Vec2f parentAbsPos, RectInt? scissor)>? deferred)
    {
        var absPos = parentAbsPos + computed.Position;
        var size = computed.Size;
        var nodeIndex = sortIndex++;

        if (size.X > 0 && size.Y > 0)
        {
            var entityRef = query.GetEntity(entity);

            var bgColor = Vec4f.Zero;
            var borderColor = Vec4f.Zero;
            var borderRadius = Vec4f.Zero;
            var borderWidths = new Vec4f(computed.BorderTop, computed.BorderRight, computed.BorderBottom, computed.BorderLeft);

            bool hasBg = false;
            bool hasBorder = false;

            if (entityRef.Has<BackgroundColor>())
            {
                var bg = entityRef.Get<BackgroundColor>();
                bgColor = (Vec4f)bg.Color;
                hasBg = true;
            }

            if (entityRef.Has<BorderColor>())
            {
                var bc = entityRef.Get<BorderColor>();
                borderColor = ((Vec4f)bc.Top + (Vec4f)bc.Right + (Vec4f)bc.Bottom + (Vec4f)bc.Left) * 0.25f;
                hasBorder = true;
            }

            if (entityRef.Has<BorderRadius>())
            {
                var br = entityRef.Get<BorderRadius>();
                borderRadius = new Vec4f(br.TopLeft, br.TopRight, br.BottomRight, br.BottomLeft);
            }

            float shapeType = 0f;
            if (entityRef.Has<UIShape>())
            {
                shapeType = (float)entityRef.Get<UIShape>().Type;
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

            // Text input caret
            if (entityRef.Has<UITextInput>() && entityRef.Has<UIInteraction>())
            {
                ref readonly var input = ref entityRef.Get<UITextInput>();
                ref readonly var interaction = ref entityRef.Get<UIInteraction>();

                if (interaction.IsFocused && input.CaretVisible && input.CaretHeight > 0
                    && !input.TextEntity.IsNull)
                {
                    var textRef = query.GetEntity(input.TextEntity);
                    if (textRef.Has<ComputedNode>())
                    {
                        ref readonly var textComputed = ref textRef.Get<ComputedNode>();
                        var textAbsX = absPos.X + textComputed.Position.X;
                        var textAbsY = absPos.Y + textComputed.Position.Y;

                        var caretW = 2f;
                        var caretH = input.CaretHeight;
                        var caretX = textAbsX + input.CaretXOffset;
                        var caretY = textAbsY + (textComputed.Size.Y - caretH) * 0.5f;

                        var batchKey = new UIRectBatchKey(material, scissor);
                        var batch = batches.GetOrCreate(batchKey);
                        batch.Draw((ulong)nodeIndex * 4 + 1, new UIRectBatch.InstanceData
                        {
                            PosSize = new Vec4f(caretX, caretY, caretW, caretH),
                            BackgroundColor = new Vec4f(1f, 1f, 1f, 1f),
                            BorderColor = Vec4f.Zero,
                            BorderRadius = Vec4f.Zero,
                            BorderWidths = Vec4f.Zero,
                            Extra = Vec4f.Zero,
                        });
                    }
                }
            }

            // Slider fill bar + thumb knob
            if (entityRef.Has<UISlider>())
            {
                ref readonly var slider = ref entityRef.Get<UISlider>();
                var range = slider.Max - slider.Min;
                var ratio = range > 0 ? Math.Clamp((slider.Value - slider.Min) / range, 0f, 1f) : 0f;

                var batchKey = new UIRectBatchKey(material, scissor);
                var batch = batches.GetOrCreate(batchKey);

                var fillW = size.X * ratio;
                if (fillW > 0.5f)
                {
                    batch.Draw((ulong)nodeIndex * 4 + 1, new UIRectBatch.InstanceData
                    {
                        PosSize = new Vec4f(absPos.X, absPos.Y, fillW, size.Y),
                        BackgroundColor = (Vec4f)slider.FillColor,
                        BorderColor = Vec4f.Zero,
                        BorderRadius = borderRadius,
                        BorderWidths = Vec4f.Zero,
                        Extra = Vec4f.Zero,
                    });
                }

                var thumbDiameter = size.Y * 1.4f;
                var thumbX = absPos.X + size.X * ratio - thumbDiameter * 0.5f;
                var thumbY = absPos.Y + (size.Y - thumbDiameter) * 0.5f;
                batch.Draw((ulong)nodeIndex * 4 + 2, new UIRectBatch.InstanceData
                {
                    PosSize = new Vec4f(thumbX, thumbY, thumbDiameter, thumbDiameter),
                    BackgroundColor = (Vec4f)slider.ThumbColor,
                    BorderColor = Vec4f.Zero,
                    BorderRadius = Vec4f.Zero,
                    BorderWidths = Vec4f.Zero,
                    Extra = new Vec4f(1f, 0f, 0f, 0f),
                });
            }

            // Scrollbar indicators
            if (entityRef.Has<UIScrollOffset>() && entityRef.Has<UIStyle>())
            {
                ref readonly var scroll = ref entityRef.Get<UIScrollOffset>();
                ref readonly var scrollStyle = ref entityRef.Get<UIStyle>();

                const float scrollbarThickness = 6f;
                const float scrollbarPadding = 2f;
                var scrollbarColor = new Vec4f(1f, 1f, 1f, 0.3f);

                // Vertical scrollbar (right edge)
                if (scrollStyle.Value.Overflow.Y == Overflow.Scroll && computed.ContentSize.Y > size.Y)
                {
                    float innerH = size.Y - computed.PaddingTop - computed.PaddingBottom
                        - computed.BorderTop - computed.BorderBottom;
                    float contentH = computed.ContentSize.Y;
                    float thumbH = Math.Max(20f, (innerH / contentH) * innerH);
                    float maxScroll = contentH - innerH;
                    float scrollRatio = maxScroll > 0 ? scroll.Offset.Y / maxScroll : 0f;
                    float trackH = innerH - thumbH;
                    float thumbYPos = absPos.Y + computed.BorderTop + computed.PaddingTop + scrollRatio * trackH;
                    float thumbXPos = absPos.X + size.X - scrollbarThickness - scrollbarPadding - computed.BorderRight;

                    var batchKey = new UIRectBatchKey(material, scissor);
                    var batch = batches.GetOrCreate(batchKey);
                    batch.Draw((ulong)nodeIndex * 4 + 3, new UIRectBatch.InstanceData
                    {
                        PosSize = new Vec4f(thumbXPos, thumbYPos, scrollbarThickness, thumbH),
                        BackgroundColor = scrollbarColor,
                        BorderColor = Vec4f.Zero,
                        BorderRadius = new Vec4f(3f, 3f, 3f, 3f),
                        BorderWidths = Vec4f.Zero,
                        Extra = Vec4f.Zero,
                    });
                }

                // Horizontal scrollbar (bottom edge)
                if (scrollStyle.Value.Overflow.X == Overflow.Scroll && computed.ContentSize.X > size.X)
                {
                    float innerW = size.X - computed.PaddingLeft - computed.PaddingRight
                        - computed.BorderLeft - computed.BorderRight;
                    float contentW = computed.ContentSize.X;
                    float thumbW = Math.Max(20f, (innerW / contentW) * innerW);
                    float maxScroll = contentW - innerW;
                    float scrollRatio = maxScroll > 0 ? scroll.Offset.X / maxScroll : 0f;
                    float trackW = innerW - thumbW;
                    float thumbXPos = absPos.X + computed.BorderLeft + computed.PaddingLeft + scrollRatio * trackW;
                    float thumbYPos = absPos.Y + size.Y - scrollbarThickness - scrollbarPadding - computed.BorderBottom;

                    var batchKey = new UIRectBatchKey(material, scissor);
                    var batch = batches.GetOrCreate(batchKey);
                    batch.Draw((ulong)nodeIndex * 4 + 3, new UIRectBatch.InstanceData
                    {
                        PosSize = new Vec4f(thumbXPos, thumbYPos, thumbW, scrollbarThickness),
                        BackgroundColor = scrollbarColor,
                        BorderColor = Vec4f.Zero,
                        BorderRadius = new Vec4f(3f, 3f, 3f, 3f),
                        BorderWidths = Vec4f.Zero,
                        Extra = Vec4f.Zero,
                    });
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
