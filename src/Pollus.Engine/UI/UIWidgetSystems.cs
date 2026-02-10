namespace Pollus.Engine.UI;

using Pollus.ECS;
using Pollus.UI;

public static class UIWidgetSystems
{
    public const string ButtonVisualLabel = "UIWidgetSystems::ButtonVisual";
    public const string ToggleLabel = "UIWidgetSystems::Toggle";

    public static SystemBuilder ButtonVisual() => FnSystem.Create(
        new(ButtonVisualLabel) { RunsAfter = [UIInteractionSystem.UpdateStateLabel] },
        static (Query<UIButton, UIInteraction, BackgroundColor> qButtons) =>
        {
            qButtons.ForEach(static (ref UIButton button, ref UIInteraction interaction, ref BackgroundColor bg) =>
            {
                bg.Color = GetButtonColor(button, interaction);
            });
        }
    );

    internal static void UpdateButtonVisuals(Query query)
    {
        query.Filtered<All<UIButton>>().ForEach(query, static (in Query q, in Entity entity) =>
        {
            var entRef = q.GetEntity(entity);
            ref var button = ref entRef.Get<UIButton>();
            ref var interaction = ref entRef.Get<UIInteraction>();
            ref var bg = ref entRef.Get<BackgroundColor>();
            bg.Color = GetButtonColor(button, interaction);
        });
    }

    public static SystemBuilder Toggle() => FnSystem.Create(
        new(ToggleLabel) { RunsAfter = [UIInteractionSystem.UpdateStateLabel] },
        static (
            EventReader<UIInteractionEvents.UIClickEvent> clickReader,
            Events events,
            Query query) =>
        {
            UpdateToggles(query, clickReader, events);
        }
    );

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

    static Pollus.Utils.Color GetButtonColor(in UIButton button, in UIInteraction interaction)
    {
        if (interaction.IsDisabled) return button.DisabledColor;
        if (interaction.IsPressed) return button.PressedColor;
        if (interaction.IsHovered) return button.HoverColor;
        return button.NormalColor;
    }
}
