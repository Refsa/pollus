namespace Pollus.UI;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;

public class UIRadioGroupBuilder : UINodeBuilder<UIRadioGroupBuilder>
{
    readonly int groupId;
    readonly List<string?> options = [];
    int selectedIndex = -1;
    Color? selectedColor;
    Color? unselectedColor;
    Color? indicatorColor;
    float fontSize = 16f;
    Color textColor = Color.WHITE;
    Handle font = Handle.Null;

    public UIRadioGroupBuilder(Commands commands, int? groupId) : base(commands)
    {
        this.groupId = groupId ?? UIRadioButtonBuilder.NextGroupId;
    }

    public UIRadioGroupBuilder Option()
    {
        options.Add(null);
        return this;
    }

    public UIRadioGroupBuilder Option(string label)
    {
        options.Add(label);
        return this;
    }

    public UIRadioGroupBuilder Selected(int index)
    {
        selectedIndex = index;
        return this;
    }

    public UIRadioGroupBuilder SelectedColor(Color color)
    {
        selectedColor = color;
        return this;
    }

    public UIRadioGroupBuilder UnselectedColor(Color color)
    {
        unselectedColor = color;
        return this;
    }

    public UIRadioGroupBuilder IndicatorColor(Color color)
    {
        indicatorColor = color;
        return this;
    }

    public UIRadioGroupBuilder FontSize(float size)
    {
        fontSize = size;
        return this;
    }

    public UIRadioGroupBuilder TextColor(Color color)
    {
        textColor = color;
        return this;
    }

    public UIRadioGroupBuilder Font(Handle font)
    {
        this.font = font;
        return this;
    }

    public new RadioGroupResult Spawn()
    {
        // 1. Create container panel
        var container = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle { Value = style }
        )).Entity;

        Setup(container);

        // 2. Create option entities
        var optionEntities = new Entity[options.Count];
        for (int i = 0; i < options.Count; i++)
        {
            var hasLabel = options[i] != null;

            var radioButton = new UIRadioButton
            {
                GroupId = groupId,
                IsSelected = i == selectedIndex,
            };
            if (selectedColor.HasValue)
                radioButton.SelectedColor = selectedColor.Value;
            if (unselectedColor.HasValue)
                radioButton.UnselectedColor = unselectedColor.Value;
            if (indicatorColor.HasValue)
                radioButton.IndicatorColor = indicatorColor.Value;

            var initialColor = radioButton.IsSelected ? radioButton.SelectedColor : radioButton.UnselectedColor;
            var indicatorBgColor = radioButton.IsSelected ? radioButton.IndicatorColor : Color.TRANSPARENT;
            var rbStyle = LayoutStyle.Default with
            {
                Size = new Size<Length>(Length.Px(18), Length.Px(18)),
            };

            var indicator = commands.Spawn(Entity.With(
                new UINode(),
                new BackgroundColor { Color = indicatorBgColor },
                new UIShape { Type = UIShapeType.Circle },
                new UIStyle
                {
                    Value = LayoutStyle.Default with
                    {
                        Size = new Size<Length>(Length.Percent(0.5f), Length.Percent(0.5f)),
                        Margin = Rect<Length>.All(Length.Auto),
                    }
                }
            )).Entity;

            radioButton.IndicatorEntity = indicator;

            if (hasLabel)
            {
                // Create a row container for the radio button + text
                var row = commands.Spawn(Entity.With(
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

                // Radio button entity
                var rbEntity = commands.Spawn(Entity.With(
                    new UINode(),
                    radioButton,
                    new UIInteraction { Focusable = true },
                    new Outline(),
                    new BackgroundColor { Color = initialColor },
                    new UIStyle { Value = rbStyle },
                    new UIShape { Type = UIShapeType.Circle }
                )).Entity;

                commands.AddChild(rbEntity, indicator);

                // Text label entity
                var textEntity = commands.Spawn(Entity.With(
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
                    commands.AddComponent(textEntity, new UITextFont { Font = font });

                commands.AddChild(row, rbEntity);
                commands.AddChild(row, textEntity);
                commands.AddChild(container, row);
                optionEntities[i] = row;
            }
            else
            {
                // Bare radio button - direct child of container
                var rbEntity = commands.Spawn(Entity.With(
                    new UINode(),
                    radioButton,
                    new UIInteraction { Focusable = true },
                    new Outline(),
                    new BackgroundColor { Color = initialColor },
                    new UIStyle { Value = rbStyle },
                    new UIShape { Type = UIShapeType.Circle }
                )).Entity;

                commands.AddChild(rbEntity, indicator);
                commands.AddChild(container, rbEntity);
                optionEntities[i] = rbEntity;
            }
        }

        return new RadioGroupResult
        {
            Entity = container,
            OptionEntities = optionEntities,
        };
    }
}
