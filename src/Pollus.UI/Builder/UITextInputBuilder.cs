namespace Pollus.UI;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.UI.Layout;
using Pollus.Utils;
using LayoutStyle = Pollus.UI.Layout.Style;

public class UITextInputBuilder : UINodeBuilder<UITextInputBuilder>
{
    UITextInput textInput = new();
    string initialText = "";
    float fontSize = 16f;
    Color textColor = Color.WHITE;
    Handle font = Handle.Null;

    public UITextInputBuilder(Commands commands) : base(commands)
    {
        focusable = true;
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

    public new TextInputResult Spawn()
    {
        // Create text display child entity
        var textEntity = commands.Spawn(Entity.With(
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
            commands.AddComponent(textEntity, new UITextFont { Font = font });

        textInput.TextEntity = textEntity;

        var entity = commands.Spawn(Entity.With(
            new UINode(),
            new UIInteraction { Focusable = focusable },
            textInput,
            backgroundColor.HasValue
                ? new BackgroundColor { Color = backgroundColor.Value }
                : new BackgroundColor(),
            new UIStyle { Value = style }
        )).Entity;

        commands.AddChild(entity, textEntity);

        // Defer text buffer initialization
        var capturedEntity = entity;
        var capturedText = initialText;
        commands.Defer(world =>
        {
            var bufs = world.Resources.Get<UITextBuffers>();
            bufs.Set(capturedEntity, capturedText);
        });

        if (borderColor.HasValue)
            commands.AddComponent(entity, borderColor.Value);

        if (borderRadius.HasValue)
            commands.AddComponent(entity, borderRadius.Value);

        if (boxShadow.HasValue)
            commands.AddComponent(entity, boxShadow.Value);

        if (parentEntity.HasValue)
            commands.AddChild(parentEntity.Value, entity);

        if (children != null)
        {
            foreach (var child in children)
                commands.AddChild(entity, child);
        }

        return new TextInputResult { Entity = entity, TextEntity = textEntity };
    }
}
