namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.Utils;

public record struct UIRenderResources
{
    public Handle<UIRectMaterial> Material;
}

public static class ExtractUIRectsSystem
{
    public const string Label = "ExtractUIRects";

    public static ISystemBuilder Create() => FnSystem.Create(
        new(Label)
        {
            RunsAfter = [RenderingPlugin.BeginFrameSystem],
        },
        static (
            UIRectBatches batches,
            UIRenderResources resources,
            Query query,
            Query<UILayoutRoot, ComputedNode>.Filter<All<UINode>> qRoots) =>
        {
            batches.Reset();

            var batchKey = new UIRectBatchKey(resources.Material);
            var batch = batches.GetOrCreate(batchKey);
            uint sortIndex = 0;

            foreach (var root in qRoots)
            {
                var rootEntity = root.Entity;
                ref readonly var rootComputed = ref root.Component1;

                EmitNode(batch, ref sortIndex, query, rootEntity, rootComputed, Vec2f.Zero);
            }
        }
    );

    static void EmitNode(UIRectBatch batch, ref uint sortIndex, Query query, Entity entity, in ComputedNode computed, Vec2f parentAbsPos)
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
                // Use average of all border colors for now (single-color shader)
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

            // Text input caret - position relative to the text child entity
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
                        // Text entity position is relative to this parent
                        var textAbsX = absPos.X + textComputed.Position.X;
                        var textAbsY = absPos.Y + textComputed.Position.Y;

                        var caretW = 2f;
                        var caretH = input.CaretHeight;
                        var caretX = textAbsX + input.CaretXOffset;
                        // Vertically center the caret within the text entity
                        var caretY = textAbsY + (textComputed.Size.Y - caretH) * 0.5f;

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

            // Slider fill bar + thumb knob (use sub-keys within this node's
            // sort range so the shared sortIndex stays in sync with ExtractUIText)
            if (entityRef.Has<UISlider>())
            {
                ref readonly var slider = ref entityRef.Get<UISlider>();
                var range = slider.Max - slider.Min;
                var ratio = range > 0 ? Math.Clamp((slider.Value - slider.Min) / range, 0f, 1f) : 0f;

                // Fill bar
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

                // Thumb knob (circle, slightly taller than track)
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
                    Extra = new Vec4f(1f, 0f, 0f, 0f), // Circle shape
                });
            }
        }

        // Walk children via Parent/Child linked list
        var entRef = query.GetEntity(entity);
        if (!entRef.Has<Parent>()) return;

        // Child positions from the flex layout already include the
        // parent's padding+border offset, so we pass absPos directly
        // without adding a content offset.
        var childEntity = entRef.Get<Parent>().FirstChild;
        while (!childEntity.IsNull)
        {
            var childRef = query.GetEntity(childEntity);
            ref var childComputed = ref childRef.Get<ComputedNode>();
            EmitNode(batch, ref sortIndex, query, childEntity, childComputed, absPos);

            if (childRef.Has<Child>())
            {
                childEntity = childRef.Get<Child>().NextSibling;
            }
            else
            {
                break;
            }
        }
    }
}
