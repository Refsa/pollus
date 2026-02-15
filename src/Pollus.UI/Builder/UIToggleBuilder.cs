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
        var entity = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = focusable },
            toggle,
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
