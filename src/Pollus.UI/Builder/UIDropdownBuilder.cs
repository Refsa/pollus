namespace Pollus.UI;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;

public class UIDropdownBuilder : UINodeBuilder<UIDropdownBuilder>
{
    readonly List<string> options = [];
    string placeholder = "";
    float fontSize = 16f;
    Color textColor = Color.WHITE;
    Handle font = Handle.Null;

    public UIDropdownBuilder(Commands commands) : base(commands) { }

    public UIDropdownBuilder Placeholder(string text)
    {
        placeholder = text;
        return this;
    }

    public UIDropdownBuilder Option(string text)
    {
        options.Add(text);
        return this;
    }

    public UIDropdownBuilder Options(params string[] texts)
    {
        options.AddRange(texts);
        return this;
    }

    public UIDropdownBuilder FontSize(float size)
    {
        fontSize = size;
        return this;
    }

    public UIDropdownBuilder TextColor(Color color)
    {
        textColor = color;
        return this;
    }

    public UIDropdownBuilder Font(Handle font)
    {
        this.font = font;
        return this;
    }

    public new DropdownResult Spawn()
    {
        // 1. Create display text entity
        var displayText = commands.Spawn(Entity.With(
            new UINode(),
            new ContentSize(),
            new UIText { Color = textColor, Size = fontSize, Text = new NativeUtf8(placeholder) },
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Length>(Length.Percent(1f), Length.Percent(1f)),
                }
            }
        )).Entity;

        if (!font.IsNull())
            commands.AddComponent(displayText, new UITextFont { Font = font });

        // 2. Create trigger entity (the dropdown itself)
        interactable = true;
        focusable = true;
        backgroundColor ??= new Color();

        var trigger = commands.Spawn(Entity.With(
            new UINode(),
            new UIDropdown { SelectedIndex = -1, DisplayTextEntity = displayText },
            new UIStyle { Value = style }
        )).Entity;

        commands.AddChild(trigger, displayText);

        // 3. Create absolute popup panel for options
        var heightPx = style.Size.Height.Tag == Length.Kind.Px ? style.Size.Height.Value : 0f;
        var padTopPx = style.Padding.Top.Tag == Length.Kind.Px ? style.Padding.Top.Value : 0f;
        var borderTopPx = style.Border.Top.Tag == Length.Kind.Px ? style.Border.Top.Value : 0f;
        var insetTop = Length.Px(heightPx - padTopPx - borderTopPx);

        var popupPanel = commands.Spawn(Entity.With(
            new UINode(),
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Display = Display.None,
                    Position = Position.Absolute,
                    Inset = new Rect<Length>(Length.Px(0), Length.Auto, insetTop, Length.Auto),
                    FlexDirection = FlexDirection.Column,
                    Size = new Size<Length>(Length.Percent(1f), Length.Auto),
                }
            }
        )).Entity;

        if (backgroundColor.HasValue)
            commands.AddComponent(popupPanel, new BackgroundColor { Color = backgroundColor.Value });

        if (borderColor.HasValue)
            commands.AddComponent(popupPanel, borderColor.Value);

        if (borderRadius.HasValue)
            commands.AddComponent(popupPanel, borderRadius.Value);

        // 4. Create option entities inside the popup panel
        var optionEntities = new Entity[options.Count];
        var optionTextEntities = new Entity[options.Count];
        for (int i = 0; i < options.Count; i++)
        {
            // Option text child
            var optionText = commands.Spawn(Entity.With(
                new UINode(),
                new ContentSize(),
                new UIText { Color = textColor, Size = fontSize, Text = new NativeUtf8(options[i]) },
                new UIStyle
                {
                    Value = LayoutStyle.Default with
                    {
                        Size = new Size<Length>(Length.Percent(1f), Length.Percent(1f)),
                    }
                }
            )).Entity;

            if (!font.IsNull())
                commands.AddComponent(optionText, new UITextFont { Font = font });

            // Option entity - acts as a button with hover/active states
            var optionBg = backgroundColor.HasValue ? backgroundColor.Value : new Color(0.2f, 0.2f, 0.25f, 1f);
            var option = commands.Spawn(Entity.With(
                new UINode(),
                new UIInteraction { Focusable = true },
                new UIDropdownOptionTag { DropdownEntity = trigger, OptionIndex = i },
                new UIButton
                {
                    NormalColor = optionBg,
                    HoverColor = optionBg.Lighten(0.15f),
                    PressedColor = optionBg.Darken(0.1f),
                },
                new BackgroundColor { Color = optionBg },
                new UIStyle
                {
                    Value = LayoutStyle.Default with
                    {
                        Size = new Size<Length>(Length.Percent(1f), style.Size.Height),
                        Padding = style.Padding,
                        AlignItems = style.AlignItems,
                    }
                }
            )).Entity;

            commands.AddChild(option, optionText);
            commands.AddChild(popupPanel, option);
            optionEntities[i] = option;
            optionTextEntities[i] = optionText;
        }

        // 5. Apply visual components and hierarchy to trigger
        Setup(trigger);

        commands.AddChild(trigger, popupPanel);

        // Set PopupRootEntity on the dropdown component
        commands.SetComponent(trigger, new UIDropdown { SelectedIndex = -1, DisplayTextEntity = displayText, PopupRootEntity = popupPanel });

        return new DropdownResult
        {
            Entity = trigger,
            DisplayTextEntity = displayText,
            PopupPanelEntity = popupPanel,
            OptionEntities = optionEntities,
            OptionTextEntities = optionTextEntities,
        };
    }
}
