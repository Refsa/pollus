namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

public class UICheckBoxBuilder : UINodeBuilder<UICheckBoxBuilder>
{
    UICheckBox checkBox = new();

    public UICheckBoxBuilder(Commands commands) : base(commands) { }

    public UICheckBoxBuilder IsChecked(bool value = true)
    {
        checkBox.IsChecked = value;
        return this;
    }

    public UICheckBoxBuilder CheckedColor(Color color)
    {
        checkBox.CheckedColor = color;
        return this;
    }

    public UICheckBoxBuilder UncheckedColor(Color color)
    {
        checkBox.UncheckedColor = color;
        return this;
    }

    public UICheckBoxBuilder CheckmarkColor(Color color)
    {
        checkBox.CheckmarkColor = color;
        return this;
    }

    public override Entity Spawn()
    {
        var entity = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = focusable },
            checkBox,
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
