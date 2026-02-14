namespace Pollus.UI;

using Pollus.ECS;

public class UIButtonSystem : ISystemSet
{
    public static readonly SystemBuilderDescriptor UpdateVisualsDescriptor = new()
    {
        Label = new SystemLabel("UIButtonSystem::UpdateVisuals"),
        Stage = CoreStage.PostUpdate,
        RunsAfter = [UIInteractionSystem.UpdateStateLabel],
    };

    public static void AddToSchedule(Schedule schedule)
    {
        schedule.AddSystems(UpdateVisualsDescriptor.Stage, FnSystem.Create(UpdateVisualsDescriptor,
            (SystemDelegate<Query<UIButton, UIInteraction, BackgroundColor>>)UpdateVisuals));
    }

    public static void UpdateVisuals(Query<UIButton, UIInteraction, BackgroundColor> qButtons)
    {
        qButtons.ForEach(static (ref UIButton button, ref UIInteraction interaction, ref BackgroundColor bg) =>
        {
            bg.Color = GetButtonColor(button, interaction);
        });
    }

    internal static void UpdateButtonVisuals(Query query)
    {
        query.Filtered<All<UIButton, UIInteraction, BackgroundColor>>().ForEach(query,
            static (in Query q, in Entity entity) =>
            {
                ref readonly var button = ref q.Get<UIButton>(entity);
                ref readonly var interaction = ref q.Get<UIInteraction>(entity);
                ref var bg = ref q.Get<BackgroundColor>(entity);
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
