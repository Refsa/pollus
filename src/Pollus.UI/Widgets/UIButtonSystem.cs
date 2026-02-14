namespace Pollus.UI;

using Pollus.ECS;

public static class UIButtonSystem
{
    public const string ButtonVisualLabel = "UIButtonSystem::ButtonVisual";

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

    static Pollus.Utils.Color GetButtonColor(in UIButton button, in UIInteraction interaction)
    {
        if (interaction.IsDisabled) return button.DisabledColor;
        if (interaction.IsPressed) return button.PressedColor;
        if (interaction.IsHovered) return button.HoverColor;
        return button.NormalColor;
    }
}
