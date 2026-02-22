namespace Pollus.UI;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.Utils;
using System.Diagnostics.CodeAnalysis;

public struct UITextBuilder : IUINodeBuilder<UITextBuilder>
{
    internal UINodeBuilderState state;
    [UnscopedRef] public ref UINodeBuilderState State => ref state;

    string text;
    float fontSize;
    float lineHeight;
    Color textColor;
    Handle font;

    public UITextBuilder(Commands commands, string text)
    {
        state = new UINodeBuilderState(commands);
        this.text = text;
        fontSize = 16f;
        textColor = Pollus.Utils.Color.WHITE;
        font = Handle.Null;
    }

    public UITextBuilder FontSize(float size)
    {
        fontSize = size;
        return this;
    }

    public UITextBuilder Color(Color color)
    {
        textColor = color;
        return this;
    }

    public UITextBuilder Font(Handle font)
    {
        this.font = font;
        return this;
    }

    public UITextBuilder LineHeight(float value)
    {
        lineHeight = value;
        return this;
    }

    public Entity Spawn()
    {
        var entity = state.commands.Spawn(Entity.With(
            new UINode(),
            new ContentSize(),
            new UIText { Color = textColor, Size = fontSize, LineHeight = lineHeight, Text = new NativeUtf8(text) },
            new UIStyle { Value = state.style }
        )).Entity;

        if (!font.IsNull())
            state.commands.AddComponent(entity, new UITextFont { Font = font });

        state.Setup(entity);

        return entity;
    }
}
