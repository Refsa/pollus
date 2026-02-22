namespace Pollus.UI;

using Pollus.ECS;
using Pollus.UI.Layout;
using Pollus.Utils;
using System.Diagnostics.CodeAnalysis;
using LayoutStyle = Pollus.UI.Layout.Style;

public struct UICheckBoxBuilder : IUINodeBuilder<UICheckBoxBuilder>
{
    internal UINodeBuilderState state;
    [UnscopedRef] public ref UINodeBuilderState State => ref state;

    UICheckBox checkBox;

    public UICheckBoxBuilder(Commands commands)
    {
        state = new UINodeBuilderState(commands);
        checkBox = new();
    }

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

    public Entity Spawn()
    {
        state.interactable = true;
        state.focusable = true;
        state.backgroundColor ??= new Color();

        var indicatorColor = checkBox.IsChecked ? checkBox.CheckmarkColor : Color.TRANSPARENT;
        var indicator = state.commands.Spawn(Entity.With(
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

        var entity = state.commands.Spawn(Entity.With(
            new UINode(),
            checkBox,
            new UIStyle { Value = state.style }
        )).Entity;

        state.Setup(entity);
        state.commands.AddChild(entity, indicator);

        return entity;
    }
}
