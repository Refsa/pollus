namespace Pollus.Engine.UI;

using Pollus.ECS;
using Pollus.UI;

public static class UIWidgetSystems
{
    public const string ButtonVisualLabel = "UIWidgetSystems::ButtonVisual";
    public const string ToggleLabel = "UIWidgetSystems::Toggle";
    public const string CheckBoxLabel = "UIWidgetSystems::CheckBox";
    public const string RadioButtonLabel = "UIWidgetSystems::RadioButton";

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

    public static SystemBuilder CheckBox() => FnSystem.Create(
        new(CheckBoxLabel) { RunsAfter = [UIInteractionSystem.UpdateStateLabel] },
        static (
            EventReader<UIInteractionEvents.UIClickEvent> clickReader,
            Events events,
            Query query) =>
        {
            UpdateCheckBoxes(query, clickReader, events);
        }
    );

    internal static void UpdateCheckBoxes(Query query, EventReader<UIInteractionEvents.UIClickEvent> clickReader, Events events)
    {
        var checkBoxWriter = events.GetWriter<UICheckBoxEvents.UICheckBoxEvent>();

        foreach (var click in clickReader.Read())
        {
            var entity = click.Entity;
            if (!query.Has<UICheckBox>(entity)) continue;
            if (query.Has<UIInteraction>(entity))
            {
                ref readonly var interaction = ref query.Get<UIInteraction>(entity);
                if (interaction.IsDisabled) continue;
            }

            ref var checkBox = ref query.Get<UICheckBox>(entity);
            checkBox.IsChecked = !checkBox.IsChecked;

            if (query.Has<BackgroundColor>(entity))
            {
                ref var bg = ref query.Get<BackgroundColor>(entity);
                bg.Color = checkBox.IsChecked ? checkBox.CheckedColor : checkBox.UncheckedColor;
            }

            checkBoxWriter.Write(new UICheckBoxEvents.UICheckBoxEvent { Entity = entity, IsChecked = checkBox.IsChecked });
        }
    }

    public static SystemBuilder RadioButton() => FnSystem.Create(
        new(RadioButtonLabel) { RunsAfter = [UIInteractionSystem.UpdateStateLabel] },
        static (
            EventReader<UIInteractionEvents.UIClickEvent> clickReader,
            Events events,
            Query query) =>
        {
            UpdateRadioButtons(query, clickReader, events);
        }
    );

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

    static Pollus.Utils.Color GetButtonColor(in UIButton button, in UIInteraction interaction)
    {
        if (interaction.IsDisabled) return button.DisabledColor;
        if (interaction.IsPressed) return button.PressedColor;
        if (interaction.IsHovered) return button.HoverColor;
        return button.NormalColor;
    }
}
