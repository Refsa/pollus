namespace Pollus.UI;

using Pollus.ECS;

[SystemSet]
public partial class UIToggleSystem
{
    [System(nameof(UpdateToggles))]
    static readonly SystemBuilderDescriptor UpdateDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UIInteractionSystem::UpdateState"],
    };
    internal static void UpdateToggles(Query query, EventReader<UIInteractionEvents.UIClickEvent> clickReader, Events events)
    {
        var toggleWriter = events.GetWriter<UIToggleEvents.UIToggleEvent>();

        foreach (var click in clickReader.Read())
        {
            var entity = click.Entity;
            if (!query.Has<UIToggle>(entity)) continue;
            if (!query.Has<BackgroundColor>(entity)) continue;

            if (query.Has<UIInteraction>(entity))
            {
                ref readonly var interaction = ref query.Get<UIInteraction>(entity);
                if (interaction.IsDisabled) continue;
            }

            ref var toggle = ref query.Get<UIToggle>(entity);
            toggle.IsOn = !toggle.IsOn;

            ref var bg = ref query.Get<BackgroundColor>(entity);
            bg.Color = toggle.IsOn ? toggle.OnColor : toggle.OffColor;

            toggleWriter.Write(new UIToggleEvents.UIToggleEvent { Entity = entity, IsOn = toggle.IsOn });
        }
    }
}
