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

    internal static void UpdateRadioButtons(Query<UIRadioButton> qRadio, Query<UIInteraction> qInteraction, Query<BackgroundColor> qBg, EventReader<UIInteractionEvents.UIClickEvent> clickReader, Events events)
    {
        var radioWriter = events.GetWriter<UIRadioButtonEvents.UIRadioButtonEvent>();

        foreach (var click in clickReader.Read())
        {
            var entity = click.Entity;
            if (!qRadio.Has<UIRadioButton>(entity)) continue;

            if (qInteraction.Has<UIInteraction>(entity))
            {
                ref readonly var interaction = ref qInteraction.Get<UIInteraction>(entity);
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
                if (qBg.Has<BackgroundColor>(row.Entity))
                {
                    ref var bg = ref qBg.GetTracked<BackgroundColor>(row.Entity);
                    bg.Color = r.UnselectedColor;
                }
                if (!r.IndicatorEntity.IsNull && qBg.Has<BackgroundColor>(r.IndicatorEntity))
                {
                    ref var indicatorBg = ref qBg.GetTracked<BackgroundColor>(r.IndicatorEntity);
                    indicatorBg.Color = Color.TRANSPARENT;
                }
            }

            // Select this one
            radio.IsSelected = true;
            if (qBg.Has<BackgroundColor>(entity))
            {
                ref var bg = ref qBg.GetTracked<BackgroundColor>(entity);
                bg.Color = radio.SelectedColor;
            }
            if (!radio.IndicatorEntity.IsNull && qBg.Has<BackgroundColor>(radio.IndicatorEntity))
            {
                ref var indicatorBg = ref qBg.GetTracked<BackgroundColor>(radio.IndicatorEntity);
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
