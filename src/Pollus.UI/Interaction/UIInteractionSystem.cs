namespace Pollus.UI;

using System.Collections.Generic;
using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI.Layout;

[SystemSet]
public partial class UIInteractionSystem
{
    [System(nameof(HitTest))]
    static readonly SystemBuilderDescriptor HitTestDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
    };

    [System(nameof(UpdateState))]
    static readonly SystemBuilderDescriptor UpdateStateDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UIInteractionSystem::HitTest"],
    };

    [System(nameof(FocusNavigation))]
    static readonly SystemBuilderDescriptor FocusNavigationDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UIInteractionSystem::UpdateState"],
    };

    static void HitTest(
        CurrentDevice<Mouse> currentMouse,
        UIHitTestResult hitResult,
        UIFocusState focusState,
        Query query,
        Query<UILayoutRoot, ComputedNode>.Filter<All<UINode>> qRoots)
    {
        var mouse = currentMouse.Value;
        var mousePos = mouse != null ? new Vec2f(mouse.Position.X, mouse.Position.Y) : Vec2f.Zero;
        PerformHitTest(query, hitResult, focusState, mousePos);
    }

    internal static void PerformHitTest(Query query, UIHitTestResult hitResult, UIFocusState focusState, Vec2f mousePos)
    {
        hitResult.PreviousHoveredEntity = hitResult.HoveredEntity;
        hitResult.HoveredEntity = Entity.Null;
        hitResult.MousePosition = mousePos;
        focusState.FocusOrder.Clear();

        var deferred = hitResult.DeferredBuffer;

        foreach (var rootEntity in query.Filtered<All<UILayoutRoot>>())
        {
            if (!query.Has<ComputedNode>(rootEntity)) continue;
            ref readonly var computed = ref query.Get<ComputedNode>(rootEntity);
            HitTestNode(query, ref hitResult.HoveredEntity, focusState, rootEntity, computed, Vec2f.Zero, mousePos, deferred);
        }

        // Hit-test deferred absolute-positioned nodes last so they win over normal flow
        foreach (var (deferredEntity, parentAbsPos) in deferred)
        {
            if (!query.Has<ComputedNode>(deferredEntity)) continue;
            ref readonly var computed = ref query.Get<ComputedNode>(deferredEntity);
            HitTestNode(query, ref hitResult.HoveredEntity, focusState, deferredEntity, computed, parentAbsPos, mousePos, null);
        }

        deferred.Clear();
    }

    static void UpdateState(
        ButtonInput<MouseButton> mouse,
        UIHitTestResult hitResult,
        UIFocusState focusState,
        Events events,
        View<UIInteraction> view)
    {
        bool mouseDown = mouse.JustPressed(MouseButton.Left);
        bool mouseUp = mouse.JustReleased(MouseButton.Left);
        PerformUpdateState(view, hitResult, focusState, events, mouseDown, mouseUp);
    }

    internal static void PerformUpdateState(
        View<UIInteraction> view, UIHitTestResult hitResult, UIFocusState focusState,
        Events events, bool mouseDown, bool mouseUp = false)
    {
        var hoverEnterWriter = events.GetWriter<UIInteractionEvents.UIHoverEnterEvent>();
        var hoverExitWriter = events.GetWriter<UIInteractionEvents.UIHoverExitEvent>();
        var pressWriter = events.GetWriter<UIInteractionEvents.UIPressEvent>();
        var releaseWriter = events.GetWriter<UIInteractionEvents.UIReleaseEvent>();
        var clickWriter = events.GetWriter<UIInteractionEvents.UIClickEvent>();
        var focusWriter = events.GetWriter<UIInteractionEvents.UIFocusEvent>();
        var blurWriter = events.GetWriter<UIInteractionEvents.UIBlurEvent>();
        var dragWriter = events.GetWriter<UIInteractionEvents.UIDragEvent>();

        var hovered = hitResult.HoveredEntity;
        var prevHovered = hitResult.PreviousHoveredEntity;

        // Hover exit
        if (!prevHovered.IsNull && prevHovered != hovered)
        {
            if (view.Has<UIInteraction>(prevHovered))
            {
                ref var prevInteraction = ref view.GetTracked<UIInteraction>(prevHovered);
                prevInteraction.State &= ~InteractionState.Hovered;
                hoverExitWriter.Write(new UIInteractionEvents.UIHoverExitEvent { Entity = prevHovered });
            }
        }

        // Hover enter
        if (!hovered.IsNull && hovered != prevHovered)
        {
            if (view.Has<UIInteraction>(hovered))
            {
                ref var interaction = ref view.GetTracked<UIInteraction>(hovered);
                if (!interaction.IsDisabled)
                {
                    interaction.State |= InteractionState.Hovered;
                    hoverEnterWriter.Write(new UIInteractionEvents.UIHoverEnterEvent { Entity = hovered });
                }
            }
        }

        // Press
        if (mouseDown && !hovered.IsNull)
        {
            if (view.Has<UIInteraction>(hovered))
            {
                ref var interaction = ref view.GetTracked<UIInteraction>(hovered);
                if (!interaction.IsDisabled)
                {
                    interaction.State |= InteractionState.Pressed;
                    hitResult.PressedEntity = hovered;
                    hitResult.CapturedEntity = hovered;
                    pressWriter.Write(new UIInteractionEvents.UIPressEvent { Entity = hovered });

                    // Set focus
                    focusState.FocusSource = FocusSource.Mouse;
                    SetFocus(view, focusState, events, hovered);
                }
            }
        }

        // Drag (pointer move while captured)
        if (!hitResult.CapturedEntity.IsNull && !mouseDown && !mouseUp)
        {
            var delta = hitResult.MousePosition - hitResult.PreviousMousePosition;
            if (delta.X != 0 || delta.Y != 0)
            {
                dragWriter.Write(new UIInteractionEvents.UIDragEvent
                {
                    Entity = hitResult.CapturedEntity,
                    PositionX = hitResult.MousePosition.X,
                    PositionY = hitResult.MousePosition.Y,
                    DeltaX = delta.X,
                    DeltaY = delta.Y,
                });
            }
        }

        // Release
        if (mouseUp && !hitResult.PressedEntity.IsNull)
        {
            var pressed = hitResult.PressedEntity;
            if (view.Has<UIInteraction>(pressed))
            {
                ref var interaction = ref view.GetTracked<UIInteraction>(pressed);
                interaction.State &= ~InteractionState.Pressed;
            }

            releaseWriter.Write(new UIInteractionEvents.UIReleaseEvent { Entity = pressed });

            // Click if released on same entity
            if (pressed == hovered)
            {
                clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = pressed });
            }

            hitResult.PressedEntity = Entity.Null;
            hitResult.CapturedEntity = Entity.Null;
        }

        // Click on empty space or non-focusable element clears focus
        if (mouseDown && !focusState.FocusedEntity.IsNull && focusState.FocusedEntity != hovered)
        {
            bool hoveredIsFocusable = false;
            if (!hovered.IsNull && view.Has<UIInteraction>(hovered))
            {
                ref readonly var hoveredInteraction = ref view.Read<UIInteraction>(hovered);
                hoveredIsFocusable = hoveredInteraction.Focusable && !hoveredInteraction.IsDisabled;
            }

            if (!hoveredIsFocusable)
            {
                ClearFocus(view, focusState, events);
            }
        }

        hitResult.PreviousMousePosition = hitResult.MousePosition;
    }

    internal static void SetFocus(View<UIInteraction> view, UIFocusState focusState, Events events, Entity entity)
    {
        var focusWriter = events.GetWriter<UIInteractionEvents.UIFocusEvent>();
        var blurWriter = events.GetWriter<UIInteractionEvents.UIBlurEvent>();

        // Blur previous
        if (!focusState.FocusedEntity.IsNull && focusState.FocusedEntity != entity)
        {
            if (view.Has<UIInteraction>(focusState.FocusedEntity))
            {
                ref var prevInteraction = ref view.GetTracked<UIInteraction>(focusState.FocusedEntity);
                prevInteraction.State &= ~InteractionState.Focused;
            }

            blurWriter.Write(new UIInteractionEvents.UIBlurEvent { Entity = focusState.FocusedEntity });
        }

        // Focus new
        if (view.Has<UIInteraction>(entity))
        {
            ref var interaction = ref view.GetTracked<UIInteraction>(entity);
            if (interaction.Focusable && !interaction.IsDisabled)
            {
                interaction.State |= InteractionState.Focused;
                focusState.FocusedEntity = entity;
                focusWriter.Write(new UIInteractionEvents.UIFocusEvent { Entity = entity });
            }
        }
    }

    internal static void ClearFocus(View<UIInteraction> view, UIFocusState focusState, Events events)
    {
        var blurWriter = events.GetWriter<UIInteractionEvents.UIBlurEvent>();

        if (!focusState.FocusedEntity.IsNull)
        {
            if (view.Has<UIInteraction>(focusState.FocusedEntity))
            {
                ref var interaction = ref view.GetTracked<UIInteraction>(focusState.FocusedEntity);
                interaction.State &= ~InteractionState.Focused;
            }

            blurWriter.Write(new UIInteractionEvents.UIBlurEvent { Entity = focusState.FocusedEntity });
            focusState.FocusedEntity = Entity.Null;
        }
    }

    static void FocusNavigation(
        ButtonInput<Key> keyboard,
        UIHitTestResult hitResult,
        UIFocusState focusState,
        Events events,
        View<UIInteraction> view)
    {
        bool tabPressed = keyboard.JustPressed(Key.Tab);
        bool shiftHeld = keyboard.Pressed(Key.LeftShift) || keyboard.Pressed(Key.RightShift);
        bool activatePressed = keyboard.JustPressed(Key.Enter) || keyboard.JustPressed(Key.Space);
        PerformFocusNavigation(view, focusState, events, tabPressed && !shiftHeld, tabPressed && shiftHeld, activatePressed);
    }

    internal static void PerformFocusNavigation(
        View<UIInteraction> view, UIFocusState focusState, Events events,
        bool tabPressed, bool shiftTabPressed, bool activatePressed)
    {
        if (tabPressed && focusState.FocusOrder.Count > 0)
        {
            int currentIndex = focusState.FocusedEntity.IsNull
                ? -1
                : focusState.FocusOrder.IndexOf(focusState.FocusedEntity);

            int nextIndex = (currentIndex + 1) % focusState.FocusOrder.Count;
            focusState.FocusSource = FocusSource.Keyboard;
            SetFocus(view, focusState, events, focusState.FocusOrder[nextIndex]);
        }
        else if (shiftTabPressed && focusState.FocusOrder.Count > 0)
        {
            int currentIndex = focusState.FocusedEntity.IsNull
                ? 0
                : focusState.FocusOrder.IndexOf(focusState.FocusedEntity);

            int prevIndex = (currentIndex - 1 + focusState.FocusOrder.Count) % focusState.FocusOrder.Count;
            focusState.FocusSource = FocusSource.Keyboard;
            SetFocus(view, focusState, events, focusState.FocusOrder[prevIndex]);
        }

        if (activatePressed && !focusState.FocusedEntity.IsNull)
        {
            var clickWriter = events.GetWriter<UIInteractionEvents.UIClickEvent>();
            clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = focusState.FocusedEntity });
        }
    }

    internal static void HitTestNode(
        Query query, ref Entity hitEntity, UIFocusState focusState,
        Entity entity, in ComputedNode computed, Vec2f parentAbsPos, Vec2f mousePos,
        List<(Entity entity, Vec2f parentAbsPos)>? deferred)
    {
        var absPos = parentAbsPos + computed.Position;
        var size = computed.Size;

        var entRef = query.GetEntity(entity);
        if (entRef.Has<UIInteraction>())
        {
            ref readonly var interaction = ref entRef.Get<UIInteraction>();

            if (interaction.Focusable && !interaction.IsDisabled)
            {
                focusState.FocusOrder.Add(entity);
            }

            if (size.X > 0 && size.Y > 0 && !interaction.IsDisabled)
            {
                if (mousePos.X >= absPos.X && mousePos.X < absPos.X + size.X &&
                    mousePos.Y >= absPos.Y && mousePos.Y < absPos.Y + size.Y)
                {
                    hitEntity = entity;
                }
            }
        }

        // Skip children of zero-size nodes (e.g. Display.None containers)
        if (size.X <= 0 && size.Y <= 0) return;

        if (!entRef.Has<Parent>()) return;

        // Apply scroll offset for children hit testing
        var childAbsPos = absPos;
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
                // Defer absolute-positioned children so they win hit tests over normal flow
                if (deferred != null && childRef.Has<UIStyle>()
                                     && childRef.Get<UIStyle>().Value.Position == Position.Absolute)
                {
                    deferred.Add((childEntity, childAbsPos));
                }
                else
                {
                    ref var childComputed = ref childRef.Get<ComputedNode>();
                    HitTestNode(query, ref hitEntity, focusState, childEntity, childComputed, childAbsPos, mousePos, deferred);
                }
            }

            if (childRef.Has<Child>())
                childEntity = childRef.Get<Child>().NextSibling;
            else
                break;
        }
    }
}
