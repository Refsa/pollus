namespace Pollus.UI;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.UI.Layout;
using Pollus.Utils;
using System.Diagnostics.CodeAnalysis;
using LayoutStyle = Pollus.UI.Layout.Style;

public struct UICheckBoxGroupBuilder : IUINodeBuilder<UICheckBoxGroupBuilder>
{
    internal UINodeBuilderState state;
    [UnscopedRef] public ref UINodeBuilderState State => ref state;

    List<string?> options;
    HashSet<int> checkedIndices;
    Color? checkedColor;
    Color? uncheckedColor;
    Color? checkmarkColor;
    float fontSize;
    Color textColor;
    Handle font;

    public UICheckBoxGroupBuilder(Commands commands)
    {
        state = new UINodeBuilderState(commands);
        options = [];
        checkedIndices = [];
        fontSize = 16f;
        textColor = Color.WHITE;
        font = Handle.Null;
    }

    public UICheckBoxGroupBuilder Option()
    {
        options.Add(null);
        return this;
    }

    public UICheckBoxGroupBuilder Option(string label)
    {
        options.Add(label);
        return this;
    }

    public UICheckBoxGroupBuilder Checked(int index)
    {
        checkedIndices.Add(index);
        return this;
    }

    public UICheckBoxGroupBuilder CheckedColor(Color color)
    {
        checkedColor = color;
        return this;
    }

    public UICheckBoxGroupBuilder UncheckedColor(Color color)
    {
        uncheckedColor = color;
        return this;
    }

    public UICheckBoxGroupBuilder CheckmarkColor(Color color)
    {
        checkmarkColor = color;
        return this;
    }

    public UICheckBoxGroupBuilder FontSize(float size)
    {
        fontSize = size;
        return this;
    }

    public UICheckBoxGroupBuilder TextColor(Color color)
    {
        textColor = color;
        return this;
    }

    public UICheckBoxGroupBuilder Font(Handle font)
    {
        this.font = font;
        return this;
    }

    public CheckBoxGroupResult Spawn()
    {
        var container = state.commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = state.style }
        )).Entity;

        state.Setup(container);

        var optionEntities = new Entity[options.Count];
        for (int i = 0; i < options.Count; i++)
        {
            var hasLabel = options[i] != null;

            var checkBox = new UICheckBox
            {
                IsChecked = checkedIndices.Contains(i),
            };
            if (checkedColor.HasValue)
                checkBox.CheckedColor = checkedColor.Value;
            if (uncheckedColor.HasValue)
                checkBox.UncheckedColor = uncheckedColor.Value;
            if (checkmarkColor.HasValue)
                checkBox.CheckmarkColor = checkmarkColor.Value;

            var initialColor = checkBox.IsChecked ? checkBox.CheckedColor : checkBox.UncheckedColor;
            var indicatorColor = checkBox.IsChecked ? checkBox.CheckmarkColor : Color.TRANSPARENT;
            var cbStyle = LayoutStyle.Default with
            {
                Size = new Size<Length>(Length.Px(18), Length.Px(18)),
                MinSize = new Size<Length>(Length.Px(18), Length.Px(18)),
            };

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

            if (hasLabel)
            {
                var row = state.commands.Spawn(Entity.With(
                    new UINode(),
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            FlexDirection = FlexDirection.Row,
                            AlignItems = Layout.AlignItems.Center,
                            Gap = new Size<Length>(Length.Px(4), Length.Px(4)),
                        }
                    }
                )).Entity;

                var cbEntity = state.commands.Spawn(Entity.With(
                    new UINode(),
                    checkBox,
                    new UIInteraction { Focusable = true },
                    new Outline(),
                    new BackgroundColor { Color = initialColor },
                    new UIStyle { Value = cbStyle }
                )).Entity;

                state.commands.AddChild(cbEntity, indicator);

                var textEntity = state.commands.Spawn(Entity.With(
                    new UINode(),
                    new ContentSize(),
                    new UIText { Color = textColor, Size = fontSize, Text = new NativeUtf8(options[i]!) },
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Length>(Length.Auto, Length.Auto),
                            FlexShrink = 0f,
                        }
                    }
                )).Entity;

                if (!font.IsNull())
                    state.commands.AddComponent(textEntity, new UITextFont { Font = font });

                state.commands.AddChild(row, cbEntity);
                state.commands.AddChild(row, textEntity);
                state.commands.AddChild(container, row);
                optionEntities[i] = row;
            }
            else
            {
                var cbEntity = state.commands.Spawn(Entity.With(
                    new UINode(),
                    checkBox,
                    new UIInteraction { Focusable = true },
                    new Outline(),
                    new BackgroundColor { Color = initialColor },
                    new UIStyle { Value = cbStyle }
                )).Entity;

                state.commands.AddChild(cbEntity, indicator);
                state.commands.AddChild(container, cbEntity);
                optionEntities[i] = cbEntity;
            }
        }

        return new CheckBoxGroupResult
        {
            Entity = container,
            OptionEntities = optionEntities,
        };
    }
}
