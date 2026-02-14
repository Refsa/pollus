namespace Pollus.UI;

using Pollus.ECS;

public class UIToggleSystem : ISystemSet
{
    public static readonly SystemBuilderDescriptor UpdateDescriptor = new()
    {
        Label = new SystemLabel("UIToggleSystem::Update"),
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
        UpdateToggles(query, clickReader, events);
    }

    internal static void UpdateToggles(Query query, EventReader<UIInteractionEvents.UIClickEvent> clickReader, Events events)
    {
        var toggleWriter = events.GetWriter<UIToggleEvents.UIToggleEvent>();

        foreach (var click in clickReader.Read())
        {
            var entity = click.Entity;
            if (!query.Has<UIToggle>(entity)) continue;
            if (!query.Has<BackgroundColor>(entity)) continue;

            ref var toggle = ref query.Get<UIToggle>(entity);
            toggle.IsOn = !toggle.IsOn;

            ref var bg = ref query.Get<BackgroundColor>(entity);
            bg.Color = toggle.IsOn ? toggle.OnColor : toggle.OffColor;

            toggleWriter.Write(new UIToggleEvents.UIToggleEvent { Entity = entity, IsOn = toggle.IsOn });
        }
    }
}
