namespace Pollus.Engine.UI;

using Pollus.ECS;
using Pollus.Engine.Input;
using Pollus.Mathematics;
using Pollus.UI;

public static class UIInteractionSystem
{
    public const string HitTestLabel = "UIInteractionSystem::HitTest";
    public const string UpdateStateLabel = "UIInteractionSystem::UpdateState";
    public const string FocusNavigationLabel = "UIInteractionSystem::FocusNavigation";

    public static SystemBuilder HitTest() => FnSystem.Create(
        new(HitTestLabel) { RunsAfter = [UILayoutSystem.WriteBackLabel] },
        static (
            CurrentDevice<Mouse> currentMouse,
            UIHitTestResult hitResult,
            UIFocusState focusState,
            Query query,
            Query<UILayoutRoot, ComputedNode>.Filter<All<UINode>> qRoots) =>
        {
            var mouse = currentMouse.Value;
            var mousePos = mouse != null ? new Vec2f(mouse.Position.X, mouse.Position.Y) : Vec2f.Zero;
            PerformHitTest(query, hitResult, focusState, mousePos);
        }
    );

    internal static void PerformHitTest(Query query, UIHitTestResult hitResult, UIFocusState focusState, Vec2f mousePos)
    {
        hitResult.PreviousHoveredEntity = hitResult.HoveredEntity;
        hitResult.HoveredEntity = Entity.Null;
        hitResult.MousePosition = mousePos;
        focusState.FocusOrder.Clear();

        foreach (var rootEntity in query.Filtered<All<UILayoutRoot>>())
        {
            if (!query.Has<ComputedNode>(rootEntity)) continue;
            ref readonly var computed = ref query.Get<ComputedNode>(rootEntity);
            HitTestNode(query, ref hitResult.HoveredEntity, focusState, rootEntity, computed, Vec2f.Zero, mousePos);
        }
    }

    public static SystemBuilder UpdateState() => FnSystem.Create(
        new(UpdateStateLabel) { RunsAfter = [HitTestLabel] },
        static (
            ButtonInput<MouseButton> mouse,
            UIHitTestResult hitResult,
            UIFocusState focusState,
            Events events,
            Query query) =>
        {
            bool mouseDown = mouse.JustPressed(MouseButton.Left);
            bool mouseUp = mouse.JustReleased(MouseButton.Left);
            PerformUpdateState(query, hitResult, focusState, events, mouseDown, mouseUp);
        }
    );

    internal static void PerformUpdateState(
        Query query, UIHitTestResult hitResult, UIFocusState focusState,
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
            if (query.Has<UIInteraction>(prevHovered))
            {
                ref var prevInteraction = ref query.Get<UIInteraction>(prevHovered);
                prevInteraction.State &= ~InteractionState.Hovered;
                hoverExitWriter.Write(new UIInteractionEvents.UIHoverExitEvent { Entity = prevHovered });
            }
        }

        // Hover enter
        if (!hovered.IsNull && hovered != prevHovered)
        {
            if (query.Has<UIInteraction>(hovered))
            {
                ref var interaction = ref query.Get<UIInteraction>(hovered);
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
            if (query.Has<UIInteraction>(hovered))
            {
                ref var interaction = ref query.Get<UIInteraction>(hovered);
                if (!interaction.IsDisabled)
                {
                    interaction.State |= InteractionState.Pressed;
                    hitResult.PressedEntity = hovered;
                    hitResult.CapturedEntity = hovered;
                    pressWriter.Write(new UIInteractionEvents.UIPressEvent { Entity = hovered });

                    // Set focus
                    SetFocus(query, focusState, events, hovered);
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
            if (!pressed.IsNull)
            {
                if (query.Has<UIInteraction>(pressed))
                {
                    ref var interaction = ref query.Get<UIInteraction>(pressed);
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
        }

        // Click on empty space clears focus
        if (mouseDown && hovered.IsNull && !focusState.FocusedEntity.IsNull)
        {
            ClearFocus(query, focusState, events);
        }

        hitResult.PreviousMousePosition = hitResult.MousePosition;
    }

    internal static void SetFocus(Query query, UIFocusState focusState, Events events, Entity entity)
    {
        var focusWriter = events.GetWriter<UIInteractionEvents.UIFocusEvent>();
        var blurWriter = events.GetWriter<UIInteractionEvents.UIBlurEvent>();

        // Blur previous
        if (!focusState.FocusedEntity.IsNull && focusState.FocusedEntity != entity)
        {
            if (query.Has<UIInteraction>(focusState.FocusedEntity))
            {
                ref var prevInteraction = ref query.Get<UIInteraction>(focusState.FocusedEntity);
                prevInteraction.State &= ~InteractionState.Focused;
            }
            blurWriter.Write(new UIInteractionEvents.UIBlurEvent { Entity = focusState.FocusedEntity });
        }

        // Focus new
        if (query.Has<UIInteraction>(entity))
        {
            ref var interaction = ref query.Get<UIInteraction>(entity);
            if (interaction.Focusable && !interaction.IsDisabled)
            {
                interaction.State |= InteractionState.Focused;
                focusState.FocusedEntity = entity;
                focusWriter.Write(new UIInteractionEvents.UIFocusEvent { Entity = entity });
            }
        }
    }

    internal static void ClearFocus(Query query, UIFocusState focusState, Events events)
    {
        var blurWriter = events.GetWriter<UIInteractionEvents.UIBlurEvent>();

        if (!focusState.FocusedEntity.IsNull)
        {
            if (query.Has<UIInteraction>(focusState.FocusedEntity))
            {
                ref var interaction = ref query.Get<UIInteraction>(focusState.FocusedEntity);
                interaction.State &= ~InteractionState.Focused;
            }
            blurWriter.Write(new UIInteractionEvents.UIBlurEvent { Entity = focusState.FocusedEntity });
            focusState.FocusedEntity = Entity.Null;
        }
    }

    public static SystemBuilder FocusNavigation() => FnSystem.Create(
        new(FocusNavigationLabel) { RunsAfter = [UpdateStateLabel] },
        static (
            ButtonInput<Key> keyboard,
            UIHitTestResult hitResult,
            UIFocusState focusState,
            Events events,
            Query query) =>
        {
            bool tabPressed = keyboard.JustPressed(Key.Tab);
            bool shiftHeld = keyboard.Pressed(Key.LeftShift) || keyboard.Pressed(Key.RightShift);
            bool activatePressed = keyboard.JustPressed(Key.Enter) || keyboard.JustPressed(Key.Space);
            PerformFocusNavigation(query, focusState, events, tabPressed && !shiftHeld, tabPressed && shiftHeld, activatePressed);
        }
    );

    internal static void PerformFocusNavigation(
        Query query, UIFocusState focusState, Events events,
        bool tabPressed, bool shiftTabPressed, bool activatePressed)
    {
        if (tabPressed && focusState.FocusOrder.Count > 0)
        {
            int currentIndex = focusState.FocusedEntity.IsNull
                ? -1
                : focusState.FocusOrder.IndexOf(focusState.FocusedEntity);

            int nextIndex = (currentIndex + 1) % focusState.FocusOrder.Count;
            SetFocus(query, focusState, events, focusState.FocusOrder[nextIndex]);
        }
        else if (shiftTabPressed && focusState.FocusOrder.Count > 0)
        {
            int currentIndex = focusState.FocusedEntity.IsNull
                ? 0
                : focusState.FocusOrder.IndexOf(focusState.FocusedEntity);

            int prevIndex = (currentIndex - 1 + focusState.FocusOrder.Count) % focusState.FocusOrder.Count;
            SetFocus(query, focusState, events, focusState.FocusOrder[prevIndex]);
        }

        if (activatePressed && !focusState.FocusedEntity.IsNull)
        {
            var clickWriter = events.GetWriter<UIInteractionEvents.UIClickEvent>();
            clickWriter.Write(new UIInteractionEvents.UIClickEvent { Entity = focusState.FocusedEntity });
        }
    }

    internal static void HitTestNode(
        Query query, ref Entity hitEntity, UIFocusState focusState,
        Entity entity, in ComputedNode computed, Vec2f parentAbsPos, Vec2f mousePos)
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

        if (!entRef.Has<Parent>()) return;

        var childEntity = entRef.Get<Parent>().FirstChild;
        while (!childEntity.IsNull)
        {
            var childRef = query.GetEntity(childEntity);
            if (childRef.Has<ComputedNode>())
            {
                ref var childComputed = ref childRef.Get<ComputedNode>();
                HitTestNode(query, ref hitEntity, focusState, childEntity, childComputed, absPos, mousePos);
            }

            if (childRef.Has<Child>())
                childEntity = childRef.Get<Child>().NextSibling;
            else
                break;
        }
    }
}
