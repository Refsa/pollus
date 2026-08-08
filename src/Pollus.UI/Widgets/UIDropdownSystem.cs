namespace Pollus.UI;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI.Layout;

[SystemSet]
public partial class UIDropdownSystem
{
    internal readonly record struct PopupPlacement(Vec2f Position, Vec2f Size);

    [System(CoreStage.PostUpdate, RunsAfter = ["UIInteractionSystem::UpdateState"], RunsBefore = ["UILayoutSystem::SyncTree"])]
    internal static void PerformUpdate(
        View<UIInteraction, UIText, Parent> view,
        Query<UIDropdown> qDropdown,
        Query<UIDropdownOptionTag, UIStyle> qDropdownOptions,
        Query<UIDropdownPopup, UIScrollOffset> qPopups,
        EventReader<UIInteractionEvents.UIClickEvent> clickReader,
        EventReader<UIInteractionEvents.UIKeyDownEvent> keyDownReader,
        Events events)
    {
        var selectionWriter = events.GetWriter<UIDropdownEvents.UIDropdownSelectionChanged>();
        var dirty = false;
        var clickedDropdown = Entity.Null;
        var clicks = clickReader.Read();

        foreach (var click in clicks)
        {
            var entity = click.Entity;

            if (view.Has<UIInteraction>(entity))
            {
                ref readonly var interaction = ref view.Read<UIInteraction>(entity);
                if (interaction.IsDisabled) continue;
            }

            if (qDropdown.Has<UIDropdown>(entity))
            {
                ref var dropdown = ref qDropdown.GetTracked<UIDropdown>(entity);
                dropdown.IsOpen = !dropdown.IsOpen;
                dirty = true;
                clickedDropdown = entity;
                continue;
            }

            if (qDropdownOptions.Has<UIDropdownOptionTag>(entity))
            {
                ref readonly var option = ref qDropdownOptions.Get<UIDropdownOptionTag>(entity);
                var dropdownEntity = option.DropdownEntity;

                if (!qDropdown.Has<UIDropdown>(dropdownEntity)) continue;

                ref var dropdown = ref qDropdown.GetTracked<UIDropdown>(dropdownEntity);
                var prevIndex = dropdown.SelectedIndex;
                dropdown.SelectedIndex = option.OptionIndex;
                dropdown.IsOpen = false;

                if (prevIndex != option.OptionIndex)
                {
                    selectionWriter.Write(new UIDropdownEvents.UIDropdownSelectionChanged
                    {
                        Entity = dropdownEntity,
                        SelectedIndex = option.OptionIndex,
                        PreviousIndex = prevIndex,
                    });

                    UpdateDisplayText(view, dropdownEntity, entity);
                }

                dirty = true;
                clickedDropdown = dropdownEntity;
                continue;
            }

            if (qPopups.Has<UIDropdownPopup>(entity))
            {
                clickedDropdown = qPopups.Get<UIDropdownPopup>(entity).DropdownEntity;
                continue;
            }

            foreach (var popupRow in qPopups)
            {
                ref readonly var scroll = ref popupRow.Component1;
                if (entity != scroll.VerticalThumbEntity && entity != scroll.HorizontalThumbEntity) continue;
                clickedDropdown = popupRow.Component0.DropdownEntity;
                break;
            }
        }

        foreach (var keyEvent in keyDownReader.Read())
        {
            if ((Key)keyEvent.Key != Key.Escape) continue;

            var entity = keyEvent.Entity;
            if (!qDropdown.Has<UIDropdown>(entity)) continue;

            ref var dropdown = ref qDropdown.GetTracked<UIDropdown>(entity);
            if (dropdown.IsOpen)
            {
                dropdown.IsOpen = false;
                dirty = true;
            }
        }

        if (clicks.Length > 0)
        {
            foreach (var entity in qDropdown)
            {
                if (entity.Entity == clickedDropdown) continue;
                entity.Component0.IsOpen = false;
                dirty = true;
            }
        }

        if (dirty)
        {
            SyncOptionVisibility(qDropdownOptions, qDropdown, qPopups);
        }
    }

    internal static void UpdateDisplayText(
        View<UIInteraction, UIText, Parent> view,
        Entity dropdownEntity, Entity optionEntity)
    {
        ref readonly var dropdown = ref view.Read<UIDropdown>(dropdownEntity);
        if (dropdown.DisplayTextEntity.IsNull) return;
        if (!view.Has<UIText>(dropdown.DisplayTextEntity)) return;

        if (!view.Has<Parent>(optionEntity)) return;
        var firstChild = view.Read<Parent>(optionEntity).FirstChild;
        if (firstChild.IsNull || !view.Has<UIText>(firstChild)) return;

        ref readonly var srcText = ref view.Read<UIText>(firstChild);
        ref var displayText = ref view.GetTracked<UIText>(dropdown.DisplayTextEntity);
        displayText.Text = new NativeUtf8(srcText.Text.ToString().TrimEnd('\0'));
    }

    internal static void SyncOptionVisibility(
        Query<UIDropdownOptionTag, UIStyle> qDropdownOptions,
        Query<UIDropdown> qDropdown,
        Query<UIDropdownPopup, UIScrollOffset> qPopups)
    {
        foreach (var row in qDropdown)
        {
            ref var dropdown = ref row.Component0;
            if (dropdown.PopupRootEntity.IsNull) continue;
            if (!qDropdownOptions.Has<UIStyle>(dropdown.PopupRootEntity)) continue;

            // TODO: GetTracked sets change flag even if nothing has changed, can cause unecessary iterations.
            ref var panelStyle = ref qDropdownOptions.GetTracked<UIStyle>(dropdown.PopupRootEntity);
            var display = dropdown.IsOpen ? Display.Flex : Display.None;
            if (panelStyle.Value.Display == display) continue;

            ref var val = ref panelStyle.Value;
            val.Display = display;

            if (dropdown.IsOpen && qPopups.Has<UIDropdownPopup>(dropdown.PopupRootEntity))
            {
                ref var popup = ref qPopups.GetTracked<UIDropdownPopup>(dropdown.PopupRootEntity);
                popup.NeedsMeasure = true;
                popup.NaturalSize = Vec2f.Zero;
                val.Size = new Size<Length>(Length.Percent(1f), Length.Auto);
                val.Inset = new LayoutRect<Length>(Length.Px(0f), Length.Auto, Length.Px(popup.AnchorOffsetY), Length.Auto);
                val.Overflow = new Point<Overflow>(Overflow.Visible, Overflow.Visible);

                if (qPopups.Has<UIScrollOffset>(dropdown.PopupRootEntity))
                    qPopups.GetTracked<UIScrollOffset>(dropdown.PopupRootEntity).Offset = Vec2f.Zero;
            }
        }
    }

    [System(CoreStage.PostUpdate, RunsAfter = ["UILayoutSystem::WriteBack"])]
    internal static void PlaceOpenPopups(
        Query<UIDropdown> qDropdown,
        View<ComputedNode, Child> viewNodeTree,
        View<UILayoutRoot> viewRoots,
        Query<UIDropdownPopup, UIStyle> qPopups)
    {
        foreach (var row in qDropdown)
        {
            ref readonly var dropdown = ref row.Component0;
            if (!dropdown.IsOpen || dropdown.PopupRootEntity.IsNull) continue;
            if (!viewNodeTree.Has<ComputedNode>(row.Entity) || !viewNodeTree.Has<ComputedNode>(dropdown.PopupRootEntity)) continue;

            var rootEntity = FindLayoutRoot(viewNodeTree, viewRoots, row.Entity);
            if (rootEntity.IsNull) continue;

            ref readonly var root = ref viewRoots.Read<UILayoutRoot>(rootEntity);
            ref readonly var trigger = ref viewNodeTree.Read<ComputedNode>(row.Entity);
            // TODO: Make sure GetTracked isnt over expensive
            ref var popup = ref viewNodeTree.GetTracked<ComputedNode>(dropdown.PopupRootEntity);

            var rootPosition = LayoutHelpers.ComputeAbsolutePosition(viewNodeTree, rootEntity);
            var triggerPosition = LayoutHelpers.ComputeAbsolutePosition(viewNodeTree, row.Entity);
            var naturalPopupSize = popup.UnroundedSize;
            if (qPopups.Has<UIDropdownPopup>(dropdown.PopupRootEntity))
            {
                ref var popupState = ref qPopups.GetTracked<UIDropdownPopup>(dropdown.PopupRootEntity);
                if (popupState.NeedsMeasure)
                {
                    if (naturalPopupSize.X <= 0f) naturalPopupSize.X = popup.Size.X;
                    if (naturalPopupSize.Y <= 0f) naturalPopupSize.Y = popup.Size.Y;
                    if (naturalPopupSize.X <= 0f || naturalPopupSize.Y <= 0f) continue;
                    popupState.NaturalSize = naturalPopupSize;
                    popupState.NeedsMeasure = false;
                }
                else
                {
                    naturalPopupSize = popupState.NaturalSize;
                }
            }
            else
            {
                if (naturalPopupSize.X <= 0f) naturalPopupSize.X = popup.Size.X;
                if (naturalPopupSize.Y <= 0f) naturalPopupSize.Y = popup.Size.Y;
            }

            var placement = CalculatePopupPlacement(
                rootPosition,
                new Vec2f(root.Size.Width, root.Size.Height),
                triggerPosition,
                trigger.Size,
                naturalPopupSize);

            var popupParent = viewNodeTree.Has<Child>(dropdown.PopupRootEntity)
                ? viewNodeTree.Read<Child>(dropdown.PopupRootEntity).Parent
                : row.Entity;
            var parentPosition = LayoutHelpers.ComputeAbsolutePosition(viewNodeTree, popupParent);
            var localPosition = placement.Position - parentPosition;
            popup.Position = localPosition;
            popup.Size = placement.Size;

            if (qPopups.Has<UIStyle>(dropdown.PopupRootEntity))
            {
                ref var popupStyle = ref qPopups.GetTracked<UIStyle>(dropdown.PopupRootEntity);
                ref var style = ref popupStyle.Value;
                var overflowY = placement.Size.Y < naturalPopupSize.Y ? Overflow.Scroll : Overflow.Visible;
                var nextSize = new Size<Length>(Length.Px(placement.Size.X), Length.Px(placement.Size.Y));
                var nextInset = new LayoutRect<Length>(Length.Px(localPosition.X), Length.Auto, Length.Px(localPosition.Y), Length.Auto);
                if (style.Size != nextSize || style.Inset != nextInset || style.Overflow.Y != overflowY)
                {
                    style.Size = nextSize;
                    style.Inset = nextInset;
                    style.Overflow = new Point<Overflow>(Overflow.Visible, overflowY);
                }
            }
        }
    }

    internal static PopupPlacement CalculatePopupPlacement(
        Vec2f viewportPosition,
        Vec2f viewportSize,
        Vec2f triggerPosition,
        Vec2f triggerSize,
        Vec2f popupSize)
    {
        var viewportRight = viewportPosition.X + MathF.Max(0f, viewportSize.X);
        var viewportBottom = viewportPosition.Y + MathF.Max(0f, viewportSize.Y);
        var width = MathF.Min(MathF.Max(0f, popupSize.X), MathF.Max(0f, viewportSize.X));
        var x = Math.Clamp(triggerPosition.X, viewportPosition.X, MathF.Max(viewportPosition.X, viewportRight - width));

        var triggerTop = Math.Clamp(triggerPosition.Y, viewportPosition.Y, viewportBottom);
        var triggerBottom = Math.Clamp(triggerPosition.Y + triggerSize.Y, viewportPosition.Y, viewportBottom);
        var spaceAbove = MathF.Max(0f, triggerTop - viewportPosition.Y);
        var spaceBelow = MathF.Max(0f, viewportBottom - triggerBottom);
        var openAbove = popupSize.Y > spaceBelow && spaceAbove > spaceBelow;
        var availableHeight = openAbove ? spaceAbove : spaceBelow;
        var height = MathF.Min(MathF.Max(0f, popupSize.Y), availableHeight);
        var y = openAbove ? triggerTop - height : triggerBottom;

        return new PopupPlacement(new Vec2f(x, y), new Vec2f(width, height));
    }

    static Entity FindLayoutRoot(View<ComputedNode, Child> viewNodeTree, View<UILayoutRoot> viewRoots, Entity entity)
    {
        var current = entity;
        while (!current.IsNull)
        {
            if (viewRoots.Has<UILayoutRoot>(current)) return current;
            if (!viewNodeTree.Has<Child>(current)) break;
            current = viewNodeTree.Read<Child>(current).Parent;
        }

        return Entity.Null;
    }
}
