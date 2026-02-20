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
    internal static void UpdateToggles(View<UIToggle, UIInteraction, BackgroundColor> view, EventReader<UIInteractionEvents.UIClickEvent> clickReader, Events events)
    {
        var toggleWriter = events.GetWriter<UIToggleEvents.UIToggleEvent>();

        foreach (var click in clickReader.Read())
        {
            var entity = click.Entity;
            if (!view.Has<UIToggle>(entity)) continue;
            if (!view.Has<BackgroundColor>(entity)) continue;

            if (view.Has<UIInteraction>(entity))
            {
                ref readonly var interaction = ref view.Read<UIInteraction>(entity);
                if (interaction.IsDisabled) continue;
            }

            ref var toggle = ref view.GetTracked<UIToggle>(entity);
            toggle.IsOn = !toggle.IsOn;

            ref var bg = ref view.GetTracked<BackgroundColor>(entity);
            bg.Color = toggle.IsOn ? toggle.OnColor : toggle.OffColor;

            toggleWriter.Write(new UIToggleEvents.UIToggleEvent { Entity = entity, IsOn = toggle.IsOn });
        }
    }
}
