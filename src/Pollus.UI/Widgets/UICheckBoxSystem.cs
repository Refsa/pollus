namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

[SystemSet]
public partial class UICheckBoxSystem
{
    [System(nameof(UpdateCheckBoxes))]
    static readonly SystemBuilderDescriptor UpdateDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UIInteractionSystem::UpdateState"],
    };

    internal static void UpdateCheckBoxes(View<UICheckBox, UIInteraction, BackgroundColor> view, EventReader<UIInteractionEvents.UIClickEvent> clickReader, Events events)
    {
        var checkBoxWriter = events.GetWriter<UICheckBoxEvents.UICheckBoxEvent>();

        foreach (var click in clickReader.Read())
        {
            var entity = click.Entity;
            if (!view.Has<UICheckBox>(entity)) continue;
            if (view.Has<UIInteraction>(entity))
            {
                ref readonly var interaction = ref view.Read<UIInteraction>(entity);
                if (interaction.IsDisabled) continue;
            }

            ref var checkBox = ref view.GetTracked<UICheckBox>(entity);
            checkBox.IsChecked = !checkBox.IsChecked;

            if (view.Has<BackgroundColor>(entity))
            {
                ref var bg = ref view.GetTracked<BackgroundColor>(entity);
                bg.Color = checkBox.IsChecked ? checkBox.CheckedColor : checkBox.UncheckedColor;
            }

            if (!checkBox.IndicatorEntity.IsNull && view.Has<BackgroundColor>(checkBox.IndicatorEntity))
            {
                ref var indicatorBg = ref view.GetTracked<BackgroundColor>(checkBox.IndicatorEntity);
                indicatorBg.Color = checkBox.IsChecked ? checkBox.CheckmarkColor : Color.TRANSPARENT;
            }

            checkBoxWriter.Write(new UICheckBoxEvents.UICheckBoxEvent { Entity = entity, IsChecked = checkBox.IsChecked });
        }
    }
}
