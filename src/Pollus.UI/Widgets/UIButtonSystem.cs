namespace Pollus.UI;

using Pollus.ECS;

[SystemSet]
public partial class UIButtonSystem
{
    [System(nameof(UpdateVisuals))]
    static readonly SystemBuilderDescriptor UpdateVisualsDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UIInteractionSystem::UpdateState"],
    };

    static void UpdateVisuals(Query<UIButton, UIInteraction, BackgroundColor> qButtons)
    {
        qButtons.ForEach(static (ref button, ref interaction, ref bg) =>
        {
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
