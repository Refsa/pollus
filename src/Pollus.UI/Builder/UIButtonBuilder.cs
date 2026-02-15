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
        interactable = true;
        backgroundColor ??= new Color();

        var entity = commands.Spawn(Entity.With(
            new UINode(),
            button,
            new UIStyle { Value = style }
        )).Entity;

        Setup(entity);

        return entity;
    }
}
