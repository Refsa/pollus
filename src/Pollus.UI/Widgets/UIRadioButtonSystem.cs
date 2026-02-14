namespace Pollus.UI;

using Pollus.ECS;

public class UIRadioButtonSystem : ISystemSet
{
    public static readonly SystemBuilderDescriptor UpdateDescriptor = new()
    {
        Label = new SystemLabel("UIRadioButtonSystem::Update"),
        Stage = CoreStage.PostUpdate,
        RunsAfter = [UIInteractionSystem.UpdateStateLabel],
    };

    public static void AddToSchedule(Schedule schedule)
    {
        schedule.AddSystems(UpdateDescriptor.Stage, FnSystem.Create(UpdateDescriptor,
            (SystemDelegate<EventReader<UIInteractionEvents.UIClickEvent>, Events, Query>)Update));
    }

    public static void Update(
        EventReader<UIInteractionEvents.UIClickEvent> clickReader,
        Events events,
        Query query)
    {
        UpdateRadioButtons(query, clickReader, events);
    }

    internal static void UpdateRadioButtons(Query query, EventReader<UIInteractionEvents.UIClickEvent> clickReader, Events events)
    {
        var radioWriter = events.GetWriter<UIRadioButtonEvents.UIRadioButtonEvent>();

        foreach (var click in clickReader.Read())
        {
            var entity = click.Entity;
            if (!query.Has<UIRadioButton>(entity)) continue;

            ref var radio = ref query.Get<UIRadioButton>(entity);
            if (radio.IsSelected) continue; // Already selected, do nothing

            var groupId = radio.GroupId;

            // Deselect others in the same group
            query.Filtered<All<UIRadioButton>>().ForEach((query, groupId, entity),
                static (in (Query q, int gid, Entity clicked) ctx, in Entity e) =>
                {
                    if (e == ctx.clicked) return;
                    ref var r = ref ctx.q.Get<UIRadioButton>(e);
                    if (r.GroupId != ctx.gid || !r.IsSelected) return;

                    r.IsSelected = false;
                    if (ctx.q.Has<BackgroundColor>(e))
                    {
                        ref var bg = ref ctx.q.Get<BackgroundColor>(e);
                        bg.Color = r.UnselectedColor;
                    }
                });

            // Select this one
            radio.IsSelected = true;
            if (query.Has<BackgroundColor>(entity))
            {
                ref var bg = ref query.Get<BackgroundColor>(entity);
                bg.Color = radio.SelectedColor;
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
