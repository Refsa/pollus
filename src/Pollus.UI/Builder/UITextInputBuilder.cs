namespace Pollus.UI;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.UI.Layout;
using Pollus.Utils;
using System.Diagnostics.CodeAnalysis;
using LayoutStyle = Pollus.UI.Layout.Style;

public struct UITextInputBuilder : IUINodeBuilder<UITextInputBuilder>
{
    internal UINodeBuilderState state;
    [UnscopedRef] public ref UINodeBuilderState State => ref state;

    UITextInput textInput;
    string initialText;
    float fontSize;
    Color textColor;
    Handle font;

    public UITextInputBuilder(Commands commands)
    {
        state = new UINodeBuilderState(commands);
        state.focusable = true;
        textInput = new();
        initialText = "";
        fontSize = 16f;
        textColor = Color.WHITE;
        font = Handle.Null;
    }

    public UITextInputBuilder Text(string text)
    {
        initialText = text;
        return this;
    }

    public UITextInputBuilder Filter(UIInputFilterType filter)
    {
        textInput.Filter = filter;
        return this;
    }

    public UITextInputBuilder FontSize(float size)
    {
        fontSize = size;
        return this;
    }

    public UITextInputBuilder TextColor(Color color)
    {
        textColor = color;
        return this;
    }

    public UITextInputBuilder Font(Handle font)
    {
        this.font = font;
        return this;
    }

    public TextInputResult Spawn()
    {
        // Create text display child entity
        var textEntity = state.commands.Spawn(Entity.With(
            new UINode(),
            new ContentSize(),
            new UIText { Color = textColor, Size = fontSize, Text = new NativeUtf8(initialText) },
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    Size = new Size<Length>(Length.Percent(1f), Length.Percent(1f)),
                }
            }
        )).Entity;

        if (!font.IsNull())
            state.commands.AddComponent(textEntity, new UITextFont { Font = font });

        textInput.TextEntity = textEntity;

        state.interactable = true;
        state.backgroundColor ??= new Color();

        var entity = state.commands.Spawn(Entity.With(
            new UINode(),
            textInput,
            new UIStyle { Value = state.style }
        )).Entity;

        state.commands.AddChild(entity, textEntity);

        // Defer text buffer initialization
        var capturedEntity = entity;
        var capturedText = initialText;
        state.commands.Defer(world =>
        {
            var bufs = world.Resources.Get<UITextBuffers>();
            bufs.Set(capturedEntity, capturedText);
        });

        state.Setup(entity);

        return new TextInputResult { Entity = entity, TextEntity = textEntity };
    }
}
