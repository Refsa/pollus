namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

public class UIButtonBuilder : UINodeBuilder<UIButtonBuilder>
{
    UIButton button = new();

    public UIButtonBuilder(Commands commands) : base(commands) { }

    public UIButtonBuilder Colors(Color normal, Color? hover = null, Color? pressed = null, Color? disabled = null)
    {
        button.NormalColor = normal;
        if (hover.HasValue) button.HoverColor = hover.Value;
        if (pressed.HasValue) button.PressedColor = pressed.Value;
        if (disabled.HasValue) button.DisabledColor = disabled.Value;
        return this;
    }

    public override Entity Spawn()
    {
        var entity = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = focusable },
            button,
            backgroundColor.HasValue
                ? new BackgroundColor { Color = backgroundColor.Value }
                : new BackgroundColor(),
            new UIStyle { Value = style }
        )).Entity;

        if (borderColor.HasValue)
            commands.AddComponent(entity, borderColor.Value);

        if (borderRadius.HasValue)
            commands.AddComponent(entity, borderRadius.Value);

        if (boxShadow.HasValue)
            commands.AddComponent(entity, boxShadow.Value);

        SetupHierarchy(entity);

        return entity;
    }
}
