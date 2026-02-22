namespace Pollus.UI;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.Utils;

public class UITextBuilder : UINodeBuilder<UITextBuilder>
{
    string text;
    float fontSize = 16f;
    float lineHeight;
    Color textColor = Pollus.Utils.Color.WHITE;
    Handle font = Handle.Null;

    public UITextBuilder(Commands commands, string text) : base(commands)
    {
        this.text = text;
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

    public override Entity Spawn()
    {
        var entity = commands.Spawn(Entity.With(
            new UINode(),
            new ContentSize(),
            new UIText { Color = textColor, Size = fontSize, LineHeight = lineHeight, Text = new NativeUtf8(text) },
            new UIStyle { Value = style }
        )).Entity;

        if (!font.IsNull())
            commands.AddComponent(entity, new UITextFont { Font = font });

        Setup(entity);

        return entity;
    }
}
