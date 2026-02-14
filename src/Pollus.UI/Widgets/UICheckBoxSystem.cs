namespace Pollus.UI;

using Pollus.ECS;

public static class UICheckBoxSystem
{
    public const string CheckBoxLabel = "UICheckBoxSystem::Create";

    public static SystemBuilder Create() => FnSystem.Create(
        new(CheckBoxLabel) { RunsAfter = [UIInteractionSystem.UpdateStateLabel] },
        static (
            EventReader<UIInteractionEvents.UIClickEvent> clickReader,
            Events events,
            Query query) =>
        {
            UpdateCheckBoxes(query, clickReader, events);
        }
    );

    internal static void UpdateCheckBoxes(Query query, EventReader<UIInteractionEvents.UIClickEvent> clickReader, Events events)
    {
        var checkBoxWriter = events.GetWriter<UICheckBoxEvents.UICheckBoxEvent>();

        foreach (var click in clickReader.Read())
        {
            var entity = click.Entity;
            if (!query.Has<UICheckBox>(entity)) continue;
            if (query.Has<UIInteraction>(entity))
            {
                ref readonly var interaction = ref query.Get<UIInteraction>(entity);
                if (interaction.IsDisabled) continue;
            }

            ref var checkBox = ref query.Get<UICheckBox>(entity);
            checkBox.IsChecked = !checkBox.IsChecked;

            if (query.Has<BackgroundColor>(entity))
            {
                ref var bg = ref query.Get<BackgroundColor>(entity);
                bg.Color = checkBox.IsChecked ? checkBox.CheckedColor : checkBox.UncheckedColor;
            }

            checkBoxWriter.Write(new UICheckBoxEvents.UICheckBoxEvent { Entity = entity, IsChecked = checkBox.IsChecked });
        }
    }
}
