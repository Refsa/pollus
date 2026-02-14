namespace Pollus.UI;

using Pollus.ECS;

[SystemSet]
public partial class UICheckBoxSystem
{
    [System(nameof(UpdateCheckBoxes))]
    static readonly SystemBuilderDescriptor UpdateDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UIInteractionSystem::UpdateState"],
    };

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
