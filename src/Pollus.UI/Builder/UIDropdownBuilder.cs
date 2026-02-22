namespace Pollus.UI;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.UI.Layout;
using Pollus.Utils;
using System.Diagnostics.CodeAnalysis;
using LayoutStyle = Pollus.UI.Layout.Style;

public struct UIDropdownBuilder : IUINodeBuilder<UIDropdownBuilder>
{
    internal UINodeBuilderState state;
    [UnscopedRef] public ref UINodeBuilderState State => ref state;

    List<string> options;
    string placeholder;
    float fontSize;
    Color textColor;
    Handle font;

    public UIDropdownBuilder(Commands commands)
    {
        state = new UINodeBuilderState(commands);
        options = [];
        placeholder = "";
        fontSize = 16f;
        textColor = Color.WHITE;
        font = Handle.Null;
    }

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

    public UIDropdownBuilder Options(params ReadOnlySpan<string> texts)
    {
        foreach (var text in texts)
            options.Add(text);
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

    public DropdownResult Spawn()
    {
        // 1. Create display text entity
        var displayText = state.commands.Spawn(Entity.With(
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
            state.commands.AddComponent(displayText, new UITextFont { Font = font });

        // 2. Create trigger entity (the dropdown itself)
        state.interactable = true;
        state.focusable = true;
        state.backgroundColor ??= new Color();

        var trigger = state.commands.Spawn(Entity.With(
            new UINode(),
            new UIDropdown { SelectedIndex = -1, DisplayTextEntity = displayText },
            new UIStyle { Value = state.style }
        )).Entity;

        state.commands.AddChild(trigger, displayText);

        // 3. Create absolute popup panel for options
        var heightPx = state.style.Size.Height.Tag == Length.Kind.Px ? state.style.Size.Height.Value : 0f;
        var padTopPx = state.style.Padding.Top.Tag == Length.Kind.Px ? state.style.Padding.Top.Value : 0f;
        var borderTopPx = state.style.Border.Top.Tag == Length.Kind.Px ? state.style.Border.Top.Value : 0f;
        var insetTop = Length.Px(heightPx - padTopPx - borderTopPx);

        var popupPanel = state.commands.Spawn(Entity.With(
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

        if (state.backgroundColor.HasValue)
            state.commands.AddComponent(popupPanel, new BackgroundColor { Color = state.backgroundColor.Value });

        if (state.borderColor.HasValue)
            state.commands.AddComponent(popupPanel, state.borderColor.Value);

        if (state.borderRadius.HasValue)
            state.commands.AddComponent(popupPanel, state.borderRadius.Value);

        // 4. Create option entities inside the popup panel
        var optionEntities = new Entity[options.Count];
        var optionTextEntities = new Entity[options.Count];
        for (int i = 0; i < options.Count; i++)
        {
            // Option text child
            var optionText = state.commands.Spawn(Entity.With(
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
                state.commands.AddComponent(optionText, new UITextFont { Font = font });

            // Option entity - acts as a button with hover/active states
            var optionBg = state.backgroundColor.HasValue ? state.backgroundColor.Value : new Color(0.2f, 0.2f, 0.25f, 1f);
            var option = state.commands.Spawn(Entity.With(
                new UINode(),
                new UIInteraction { Focusable = true },
                new Outline(),
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
                        Size = new Size<Length>(Length.Percent(1f), state.style.Size.Height),
                        Padding = state.style.Padding,
                        AlignItems = state.style.AlignItems,
                    }
                }
            )).Entity;

            state.commands.AddChild(option, optionText);
            state.commands.AddChild(popupPanel, option);
            optionEntities[i] = option;
            optionTextEntities[i] = optionText;
        }

        // 5. Apply visual components and hierarchy to trigger
        state.Setup(trigger);

        state.commands.AddChild(trigger, popupPanel);

        // Set PopupRootEntity on the dropdown component
        state.commands.SetComponent(trigger, new UIDropdown { SelectedIndex = -1, DisplayTextEntity = displayText, PopupRootEntity = popupPanel });

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
