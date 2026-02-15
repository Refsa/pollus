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
        interactable = true;
        backgroundColor ??= new Color();

        var entity = commands.Spawn(Entity.With(
            new UINode(),
            checkBox,
            new UIStyle { Value = style }
        )).Entity;

        Setup(entity);

        return entity;
    }
}
