namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

public class UIToggleBuilder : UINodeBuilder<UIToggleBuilder>
{
    UIToggle toggle = new();

    public UIToggleBuilder(Commands commands) : base(commands) { }

    public UIToggleBuilder IsOn(bool value = true)
    {
        toggle.IsOn = value;
        return this;
    }

    public UIToggleBuilder OnColor(Color color)
    {
        toggle.OnColor = color;
        return this;
    }

    public UIToggleBuilder OffColor(Color color)
    {
        toggle.OffColor = color;
        return this;
    }

    public override Entity Spawn()
    {
        interactable = true;
        focusable = true;
        backgroundColor ??= new Color();

        var entity = commands.Spawn(Entity.With(
            new UINode(),
            toggle,
            new UIStyle { Value = style }
        )).Entity;

        Setup(entity);

        return entity;
    }
}
