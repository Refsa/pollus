namespace Pollus.UI;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.Input;
using Pollus.UI.Layout;

[SystemSet]
public partial class UIDropdownSystem
{
    [System(nameof(PerformUpdate))]
    static readonly SystemBuilderDescriptor UpdateDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UIInteractionSystem::UpdateState"],
        RunsBefore = ["UILayoutSystem::SyncTree"],
    };

    internal static void PerformUpdate(
        Query<UIInteraction> qInteractions,
        Query<UIDropdown> qDropdown,
        Query<UIDropdownOptionTag, UIStyle> qDropdownOptions,
        Query<UIText, Parent> qText,
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

            if (qInteractions.Has<UIInteraction>(entity))
            {
                ref readonly var interaction = ref qInteractions.Get<UIInteraction>(entity);
                if (interaction.IsDisabled) continue;
            }

            if (qDropdown.Has<UIDropdown>(entity))
            {
                ref var dropdown = ref qDropdown.Get<UIDropdown>(entity);
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

                ref var dropdown = ref qDropdown.Get<UIDropdown>(dropdownEntity);
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

                    UpdateDisplayText(qDropdown, qText, dropdownEntity, entity);
                }

                dirty = true;
                clickedDropdown = entity;
                continue;
            }
        }

        foreach (var keyEvent in keyDownReader.Read())
        {
            if ((Key)keyEvent.Key != Key.Escape) continue;

            var entity = keyEvent.Entity;
            if (!qDropdown.Has<UIDropdown>(entity)) continue;

            ref var dropdown = ref qDropdown.Get<UIDropdown>(entity);
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
            SyncOptionVisibility(qDropdownOptions, qDropdown);
        }
    }

    internal static void UpdateDisplayText(
        Query<UIDropdown> qDropdown,
        Query<UIText, Parent> qText,
        Entity dropdownEntity, Entity optionEntity)
    {
        ref readonly var dropdown = ref qDropdown.Get<UIDropdown>(dropdownEntity);
        if (dropdown.DisplayTextEntity.IsNull) return;
        if (!qText.Has<UIText>(dropdown.DisplayTextEntity)) return;

        if (!qText.Has<Parent>(optionEntity)) return;
        var firstChild = qText.Get<Parent>(optionEntity).FirstChild;
        if (firstChild.IsNull || !qText.Has<UIText>(firstChild)) return;

        ref readonly var srcText = ref qText.Get<UIText>(firstChild);
        ref var displayText = ref qText.Get<UIText>(dropdown.DisplayTextEntity);
        displayText.Text = new NativeUtf8(srcText.Text.ToString().TrimEnd('\0'));
    }

    internal static void SyncOptionVisibility(Query<UIDropdownOptionTag, UIStyle> qDropdownOptions, Query<UIDropdown> qDropdown)
    {
        // Toggle popup panel visibility via PopupRootEntity
        qDropdown.ForEach(qDropdownOptions,
            static (in qOpts, in entity, ref dropdown) =>
            {
                if (dropdown.PopupRootEntity.IsNull) return;
                if (!qOpts.Has<UIStyle>(dropdown.PopupRootEntity)) return;

                ref var panelStyle = ref qOpts.Get<UIStyle>(dropdown.PopupRootEntity);
                var display = dropdown.IsOpen ? Display.Flex : Display.None;
                if (panelStyle.Value.Display != display)
                {
                    ref var val = ref panelStyle.Value;
                    val.Display = display;
                }
            });
    }
}
