using Pollus.Collections;
using Pollus.ECS;
using Pollus.Utils;

namespace Pollus.UI;

[Required<UINode>, Required<ContentSize>]
public partial struct UIText() : IComponent, IDefault<UIText>
{
    public static UIText Default { get; } = new()
    {
        Color = Color.WHITE,
        Size = 16f,
        Text = NativeUtf8.Null,
    };

    static UIText()
    {
        Component.Register<UIText>();
        RequiredComponents.Init<UIText>();
    }

    public static void CollectRequired(Dictionary<ComponentID, byte[]> collector)
    {
        var selfId = Component.GetInfo<UIText>().ID;
        if (collector.ContainsKey(selfId)) return;
        collector[selfId] = CollectionUtils.GetBytes(Default);

        UINode.CollectRequired(collector);
        ContentSize.CollectRequired(collector);
    }

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
