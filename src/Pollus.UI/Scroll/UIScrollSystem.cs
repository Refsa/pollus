namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI.Layout;
using Pollus.Utils;

[SystemSet]
public partial class UIScrollSystem
{
    const float ScrollSpeed = 30f;
    const float ScrollbarThickness = 6f;
    const float ScrollbarPadding = 2f;

    [System(nameof(Update))]
    static readonly SystemBuilderDescriptor UpdateDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UIInteractionSystem::UpdateState"],
    };

    [System(nameof(UpdateVisuals))]
    static readonly SystemBuilderDescriptor UpdateVisualsDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UILayoutSystem::WriteBack", "UIScrollSystem::Update"],
    };

    static void Update(
        CurrentDevice<Mouse> currentMouse,
        CurrentDevice<Keyboard> currentKeyboard,
        UIHitTestResult hitResult,
        EventReader<UIInteractionEvents.UIDragEvent> dragReader,
        Query query,
        Query<UIScrollOffset, ComputedNode> qScroll)
    {
        // Handle drag events on scroll thumbs
        foreach (var drag in dragReader.Read())
        {
            HandleThumbDrag(in drag, query, qScroll);
        }

        var mouse = currentMouse.Value;
        if (mouse == null) return;

        var scrollX = mouse.GetAxis(MouseAxis.ScrollX);
        var scrollY = mouse.GetAxis(MouseAxis.ScrollY);
        if (scrollX == 0f && scrollY == 0f) return;

        // Shift+scroll: vertical scroll acts as horizontal
        var keyboard = currentKeyboard.Value;
        if (scrollY != 0f && keyboard != null
            && (keyboard.Pressed(Key.LeftShift) || keyboard.Pressed(Key.RightShift)))
        {
            scrollX = scrollY;
            scrollY = 0f;
        }

        // Find the nearest ancestor (or self) with UIScrollOffset starting from hovered entity
        var target = hitResult.HoveredEntity;
        if (target.IsNull)
        {
            // Fallback: check if mouse is within any scroll container's bounds
            var mousePos = hitResult.MousePosition;
            foreach (var row in qScroll)
            {
                ref readonly var computed = ref row.Component1;
                var absPos = LayoutHelpers.ComputeAbsolutePosition(query, row.Entity);
                if (mousePos.X >= absPos.X && mousePos.X < absPos.X + computed.Size.X &&
                    mousePos.Y >= absPos.Y && mousePos.Y < absPos.Y + computed.Size.Y)
                {
                    target = row.Entity;
                    break;
                }
            }

            if (target.IsNull) return;
        }

        var scrollEntity = FindScrollAncestor(query, target);
        if (scrollEntity.IsNull) return;

        ref var scrollOffset = ref query.GetTracked<UIScrollOffset>(scrollEntity);
        ref readonly var scrollComputed = ref query.Get<ComputedNode>(scrollEntity);

        if (scrollY != 0f)
        {
            var innerHeight = scrollComputed.Size.Y - scrollComputed.PaddingTop - scrollComputed.PaddingBottom
                              - scrollComputed.BorderTop - scrollComputed.BorderBottom;
            var maxScrollY = MathF.Max(0, scrollComputed.ContentSize.Y - innerHeight);
            scrollOffset.Offset.Y = Math.Clamp(scrollOffset.Offset.Y - scrollY * ScrollSpeed, 0f, maxScrollY);
        }

        if (scrollX != 0f)
        {
            var innerWidth = scrollComputed.Size.X - scrollComputed.PaddingLeft - scrollComputed.PaddingRight
                             - scrollComputed.BorderLeft - scrollComputed.BorderRight;
            var maxScrollX = MathF.Max(0, scrollComputed.ContentSize.X - innerWidth);
            scrollOffset.Offset.X = Math.Clamp(scrollOffset.Offset.X - scrollX * ScrollSpeed, 0f, maxScrollX);
        }
    }

    internal static void UpdateVisuals(
        Commands commands,
        Query query,
        Query<UIScrollOffset, ComputedNode, UIStyle> qScroll)
    {
        var scrollbarColor = new Color(1f, 1f, 1f, 0.3f);

        foreach (var row in qScroll)
        {
            var entity = row.Entity;
            ref var scroll = ref row.Component0;
            ref readonly var computed = ref row.Component1;
            ref readonly var style = ref row.Component2;

            var size = computed.Size;
            bool justSpawned = false;

            bool needsVertical = style.Value.Overflow.Y == Overflow.Scroll && computed.ContentSize.Y > size.Y;
            bool needsHorizontal = style.Value.Overflow.X == Overflow.Scroll && computed.ContentSize.X > size.X;

            // Spawn vertical thumb entity if needed
            if (needsVertical && scroll.VerticalThumbEntity.IsNull)
            {
                var thumb = commands.Spawn(Entity.With(
                    new ComputedNode(),
                    new BackgroundColor { Color = scrollbarColor },
                    new BorderRadius { TopLeft = 3f, TopRight = 3f, BottomRight = 3f, BottomLeft = 3f },
                    new UIInteraction()
                )).Entity;
                commands.AddChild(entity, thumb);
                scroll.VerticalThumbEntity = thumb;
                justSpawned = true;
            }

            // Spawn horizontal thumb entity if needed
            if (needsHorizontal && scroll.HorizontalThumbEntity.IsNull)
            {
                var thumb = commands.Spawn(Entity.With(
                    new ComputedNode(),
                    new BackgroundColor { Color = scrollbarColor },
                    new BorderRadius { TopLeft = 3f, TopRight = 3f, BottomRight = 3f, BottomLeft = 3f },
                    new UIInteraction()
                )).Entity;
                commands.AddChild(entity, thumb);
                scroll.HorizontalThumbEntity = thumb;
                justSpawned = true;
            }

            if (justSpawned) continue;

            // Update vertical scrollbar
            if (needsVertical && !scroll.VerticalThumbEntity.IsNull
                              && query.Has<ComputedNode>(scroll.VerticalThumbEntity))
            {
                ref var thumbComputed = ref query.GetTracked<ComputedNode>(scroll.VerticalThumbEntity);

                float innerH = size.Y - computed.PaddingTop - computed.PaddingBottom
                               - computed.BorderTop - computed.BorderBottom;
                float contentH = computed.ContentSize.Y;
                float thumbH = contentH > 0 ? MathF.Max(20f, (innerH / contentH) * innerH) : 20f;
                float maxScroll = contentH - innerH;
                float scrollRatio = maxScroll > 0 ? scroll.Offset.Y / maxScroll : 0f;
                float trackH = innerH - thumbH;

                // Position relative to parent, counteracting scroll offset
                float thumbX = size.X - ScrollbarThickness - ScrollbarPadding - computed.BorderRight;
                float thumbY = computed.BorderTop + computed.PaddingTop + scrollRatio * trackH;

                thumbComputed.Position = new Vec2f(thumbX + scroll.Offset.X, thumbY + scroll.Offset.Y);
                thumbComputed.Size = new Vec2f(ScrollbarThickness, thumbH);
            }
            else if (!scroll.VerticalThumbEntity.IsNull
                     && query.Has<ComputedNode>(scroll.VerticalThumbEntity))
            {
                // Not needed anymore — hide it
                query.GetTracked<ComputedNode>(scroll.VerticalThumbEntity).Size = Vec2f.Zero;
            }

            // Update horizontal scrollbar
            if (needsHorizontal && !scroll.HorizontalThumbEntity.IsNull
                                && query.Has<ComputedNode>(scroll.HorizontalThumbEntity))
            {
                ref var thumbComputed = ref query.GetTracked<ComputedNode>(scroll.HorizontalThumbEntity);

                float innerW = size.X - computed.PaddingLeft - computed.PaddingRight
                               - computed.BorderLeft - computed.BorderRight;
                float contentW = computed.ContentSize.X;
                float thumbW = contentW > 0 ? MathF.Max(20f, (innerW / contentW) * innerW) : 20f;
                float maxScroll = contentW - innerW;
                float scrollRatio = maxScroll > 0 ? scroll.Offset.X / maxScroll : 0f;
                float trackW = innerW - thumbW;

                float thumbX = computed.BorderLeft + computed.PaddingLeft + scrollRatio * trackW;
                float thumbY = size.Y - ScrollbarThickness - ScrollbarPadding - computed.BorderBottom;

                thumbComputed.Position = new Vec2f(thumbX + scroll.Offset.X, thumbY + scroll.Offset.Y);
                thumbComputed.Size = new Vec2f(thumbW, ScrollbarThickness);
            }
            else if (!scroll.HorizontalThumbEntity.IsNull
                     && query.Has<ComputedNode>(scroll.HorizontalThumbEntity))
            {
                query.GetTracked<ComputedNode>(scroll.HorizontalThumbEntity).Size = Vec2f.Zero;
            }
        }
    }

    static void HandleThumbDrag(
        in UIInteractionEvents.UIDragEvent drag,
        Query query,
        Query<UIScrollOffset, ComputedNode> qScroll)
    {
        foreach (var row in qScroll)
        {
            ref readonly var scroll = ref row.Component0;
            ref readonly var computed = ref row.Component1;

            bool isVertical = !scroll.VerticalThumbEntity.IsNull && scroll.VerticalThumbEntity == drag.Entity;
            bool isHorizontal = !scroll.HorizontalThumbEntity.IsNull && scroll.HorizontalThumbEntity == drag.Entity;

            if (!isVertical && !isHorizontal) continue;

            ref var scrollTracked = ref query.GetTracked<UIScrollOffset>(row.Entity);
            var size = computed.Size;

            if (isVertical)
            {
                float innerH = size.Y - computed.PaddingTop - computed.PaddingBottom
                               - computed.BorderTop - computed.BorderBottom;
                float contentH = computed.ContentSize.Y;
                float thumbH = contentH > 0 ? MathF.Max(20f, (innerH / contentH) * innerH) : 20f;
                float trackH = innerH - thumbH;
                float maxScroll = MathF.Max(0, contentH - innerH);

                if (trackH > 0)
                {
                    float scrollDelta = drag.DeltaY * (maxScroll / trackH);
                    scrollTracked.Offset.Y = Math.Clamp(scrollTracked.Offset.Y + scrollDelta, 0f, maxScroll);
                }
            }

            if (isHorizontal)
            {
                float innerW = size.X - computed.PaddingLeft - computed.PaddingRight
                               - computed.BorderLeft - computed.BorderRight;
                float contentW = computed.ContentSize.X;
                float thumbW = contentW > 0 ? MathF.Max(20f, (innerW / contentW) * innerW) : 20f;
                float trackW = innerW - thumbW;
                float maxScroll = MathF.Max(0, contentW - innerW);

                if (trackW > 0)
                {
                    float scrollDelta = drag.DeltaX * (maxScroll / trackW);
                    scrollTracked.Offset.X = Math.Clamp(scrollTracked.Offset.X + scrollDelta, 0f, maxScroll);
                }
            }

            break;
        }
    }

    static Entity FindScrollAncestor(Query query, Entity entity)
    {
        var current = entity;
        while (!current.IsNull)
        {
            if (query.Has<UIScrollOffset>(current))
                return current;

            if (query.Has<Child>(current))
                current = query.Get<Child>(current).Parent;
            else
                break;
        }

        return Entity.Null;
    }
}
