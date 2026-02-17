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

    internal static void UpdateRadioButtons(Query query, EventReader<UIInteractionEvents.UIClickEvent> clickReader, Events events)
    {
        var radioWriter = events.GetWriter<UIRadioButtonEvents.UIRadioButtonEvent>();

        foreach (var click in clickReader.Read())
        {
            var entity = click.Entity;
            if (!query.Has<UIRadioButton>(entity)) continue;

            if (query.Has<UIInteraction>(entity))
            {
                ref readonly var interaction = ref query.Get<UIInteraction>(entity);
                if (interaction.IsDisabled) continue;
            }

            ref var radio = ref query.GetTracked<UIRadioButton>(entity);
            if (radio.IsSelected) continue; // Already selected, do nothing

            var groupId = radio.GroupId;

            // Deselect others in the same group
            query.Filtered<All<UIRadioButton>>().ForEach((query, groupId, entity),
                static (in (Query q, int gid, Entity clicked) ctx, in Entity e) =>
                {
                    if (e == ctx.clicked) return;
                    ref var r = ref ctx.q.GetTracked<UIRadioButton>(e);
                    if (r.GroupId != ctx.gid || !r.IsSelected) return;

                    r.IsSelected = false;
                    if (ctx.q.Has<BackgroundColor>(e))
                    {
                        ref var bg = ref ctx.q.GetTracked<BackgroundColor>(e);
                        bg.Color = r.UnselectedColor;
                    }
                    if (!r.IndicatorEntity.IsNull && ctx.q.Has<BackgroundColor>(r.IndicatorEntity))
                    {
                        ref var indicatorBg = ref ctx.q.GetTracked<BackgroundColor>(r.IndicatorEntity);
                        indicatorBg.Color = Color.TRANSPARENT;
                    }
                });

            // Select this one
            radio.IsSelected = true;
            if (query.Has<BackgroundColor>(entity))
            {
                ref var bg = ref query.GetTracked<BackgroundColor>(entity);
                bg.Color = radio.SelectedColor;
            }
            if (!radio.IndicatorEntity.IsNull && query.Has<BackgroundColor>(radio.IndicatorEntity))
            {
                ref var indicatorBg = ref query.GetTracked<BackgroundColor>(radio.IndicatorEntity);
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
