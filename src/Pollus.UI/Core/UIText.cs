namespace Pollus.UI;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.Utils;

[Required<UINode>, Required<ContentSize>]
public partial struct UIText() : IComponent, IDefault<UIText>
{
    public static UIText Default { get; } = new()
    {
        Color = Color.WHITE,
        Size = 16f,
        Text = NativeUtf8.Null,
    };

    public required Color Color { get; set; }

    public required float Size
    {
        get => size;
        set
        {
            if (size != value)
            {
                size = value;
                IsDirty = true;
            }
        }
    }

    public bool IsDirty { get; set; }
    public float LastBuildMaxWidth { get; set; } = -1f;

    NativeUtf8 text;
    float size;

    public required NativeUtf8 Text
    {
        get => text;
        set
        {
            text.Dispose();
            text = value;
            IsDirty = true;
        }
    }
}
