namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

[SystemSet]
public partial class UIRadioButtonSystem
{
    [System(nameof(UpdateRadioButtons))]
    static readonly SystemBuilderDescriptor UpdateDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UIInteractionSystem::UpdateState"],
    };

    internal static void UpdateRadioButtons(Query<UIRadioButton> qRadio, View<UIInteraction, BackgroundColor> view, EventReader<UIInteractionEvents.UIClickEvent> clickReader, Events events)
    {
        var radioWriter = events.GetWriter<UIRadioButtonEvents.UIRadioButtonEvent>();

        foreach (var click in clickReader.Read())
        {
            var entity = click.Entity;
            if (!qRadio.Has<UIRadioButton>(entity)) continue;

            if (view.Has<UIInteraction>(entity))
            {
                ref readonly var interaction = ref view.Read<UIInteraction>(entity);
                if (interaction.IsDisabled) continue;
            }

            ref var radio = ref qRadio.GetTracked<UIRadioButton>(entity);
            if (radio.IsSelected) continue; // Already selected, do nothing

            var groupId = radio.GroupId;

            // Deselect others in the same group
            foreach (var row in qRadio)
            {
                if (row.Entity == entity) continue;
                ref var r = ref qRadio.GetTracked<UIRadioButton>(row.Entity);
                if (r.GroupId != groupId || !r.IsSelected) continue;

                r.IsSelected = false;
                if (view.Has<BackgroundColor>(row.Entity))
                {
                    ref var bg = ref view.GetTracked<BackgroundColor>(row.Entity);
                    bg.Color = r.UnselectedColor;
                }
                if (!r.IndicatorEntity.IsNull && view.Has<BackgroundColor>(r.IndicatorEntity))
                {
                    ref var indicatorBg = ref view.GetTracked<BackgroundColor>(r.IndicatorEntity);
                    indicatorBg.Color = Color.TRANSPARENT;
                }
            }

            // Select this one
            radio.IsSelected = true;
            if (view.Has<BackgroundColor>(entity))
            {
                ref var bg = ref view.GetTracked<BackgroundColor>(entity);
                bg.Color = radio.SelectedColor;
            }
            if (!radio.IndicatorEntity.IsNull && view.Has<BackgroundColor>(radio.IndicatorEntity))
            {
                ref var indicatorBg = ref view.GetTracked<BackgroundColor>(radio.IndicatorEntity);
                indicatorBg.Color = radio.IndicatorColor;
            }

            radioWriter.Write(new UIRadioButtonEvents.UIRadioButtonEvent
            {
                Entity = entity,
                GroupId = groupId,
                IsSelected = true,
            });
        }
    }
}
