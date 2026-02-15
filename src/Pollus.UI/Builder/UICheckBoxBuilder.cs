namespace Pollus.UI;

using Pollus.ECS;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;

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

        var indicatorColor = checkBox.IsChecked ? checkBox.CheckmarkColor : Color.TRANSPARENT;
        var indicator = commands.Spawn(Entity.With(
            new UINode(),
            new BackgroundColor { Color = indicatorColor },
            new UIShape { Type = UIShapeType.Checkmark },
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Length>(Length.Percent(1f), Length.Percent(1f)),
                }
            }
        )).Entity;

        checkBox.IndicatorEntity = indicator;

        var entity = commands.Spawn(Entity.With(
            new UINode(),
            checkBox,
            new UIStyle { Value = style }
        )).Entity;

        Setup(entity);
        commands.AddChild(entity, indicator);

        return entity;
    }
}
