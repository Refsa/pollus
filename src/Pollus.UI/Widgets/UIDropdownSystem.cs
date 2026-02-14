namespace Pollus.UI;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.Input;
using Pollus.UI.Layout;

public static class UIDropdownSystem
{
    public const string Label = "UIDropdownSystem::Update";

    public static SystemBuilder Create() => FnSystem.Create(
        new(Label)
        {
            RunsAfter = [UIInteractionSystem.UpdateStateLabel],
            RunsBefore = [UILayoutSystem.SyncTreeLabel],
        },
        static (
            EventReader<UIInteractionEvents.UIClickEvent> clickReader,
            EventReader<UIInteractionEvents.UIKeyDownEvent> keyDownReader,
            Events events,
            Query query) =>
        {
            PerformUpdate(query, clickReader, keyDownReader, events);
        }
    );

    internal static void PerformUpdate(
        Query query,
        EventReader<UIInteractionEvents.UIClickEvent> clickReader,
        EventReader<UIInteractionEvents.UIKeyDownEvent> keyDownReader,
        Events events)
    {
        var selectionWriter = events.GetWriter<UIDropdownEvents.UIDropdownSelectionChanged>();

        foreach (var click in clickReader.Read())
        {
            var entity = click.Entity;

            // Check if clicked entity is a dropdown
            if (query.Has<UIDropdown>(entity))
            {
                ref var dropdown = ref query.Get<UIDropdown>(entity);
                dropdown.IsOpen = !dropdown.IsOpen;
                continue;
            }

            // Check if clicked entity is a dropdown option
            if (query.Has<UIDropdownOptionTag>(entity))
            {
                ref readonly var option = ref query.Get<UIDropdownOptionTag>(entity);
                var dropdownEntity = option.DropdownEntity;

                if (!query.Has<UIDropdown>(dropdownEntity)) continue;

                ref var dropdown = ref query.Get<UIDropdown>(dropdownEntity);
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

                    UpdateDisplayText(query, dropdownEntity, entity);
                }
            }
        }

        // Handle Escape to close dropdown
        foreach (var keyEvent in keyDownReader.Read())
        {
            if ((Key)keyEvent.Key != Key.Escape) continue;

            var entity = keyEvent.Entity;
            if (!query.Has<UIDropdown>(entity)) continue;

            ref var dropdown = ref query.Get<UIDropdown>(entity);
            if (dropdown.IsOpen)
            {
                dropdown.IsOpen = false;
            }
        }

        // Sync option visibility with dropdown IsOpen state
        SyncOptionVisibility(query);
    }

    internal static void UpdateDisplayText(Query query, Entity dropdownEntity, Entity optionEntity)
    {
        ref readonly var dropdown = ref query.Get<UIDropdown>(dropdownEntity);
        if (dropdown.DisplayTextEntity.IsNull) return;
        if (!query.Has<UIText>(dropdown.DisplayTextEntity)) return;

        // Get text from the option entity's first child (label)
        if (!query.Has<Parent>(optionEntity)) return;
        var firstChild = query.Get<Parent>(optionEntity).FirstChild;
        if (firstChild.IsNull || !query.Has<UIText>(firstChild)) return;

        ref readonly var srcText = ref query.Get<UIText>(firstChild);
        ref var displayText = ref query.Get<UIText>(dropdown.DisplayTextEntity);
        displayText.Text = new NativeUtf8(srcText.Text.ToString().TrimEnd('\0'));
    }

    internal static void SyncOptionVisibility(Query query)
    {
        query.Filtered<All<UIDropdownOptionTag, UIStyle>>().ForEach(query,
            static (in Query q, in Entity entity) =>
            {
                ref readonly var option = ref q.Get<UIDropdownOptionTag>(entity);
                var dropdownEntity = option.DropdownEntity;
                if (!q.Has<UIDropdown>(dropdownEntity)) return;

                ref readonly var dropdown = ref q.Get<UIDropdown>(dropdownEntity);
                ref var style = ref q.Get<UIStyle>(entity);
                var display = dropdown.IsOpen ? Display.Flex : Display.None;
                if (style.Value.Display != display)
                {
                    ref var val = ref style.Value;
                    val.Display = display;
                }
            });
    }
}
