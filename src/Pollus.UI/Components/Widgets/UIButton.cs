using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.Utils;

namespace Pollus.UI;

public partial record struct UIButton() : IComponent, IDefault<UIButton>
{
    public Color NormalColor = new(0.8f, 0.8f, 0.8f, 1f);
    public Color HoverColor = new(0.9f, 0.9f, 0.9f, 1f);
    public Color PressedColor = new(0.6f, 0.6f, 0.6f, 1f);
    public Color DisabledColor = new(0.5f, 0.5f, 0.5f, 0.5f);

    public static UIButton Default { get; } = new();

    static UIButton()
    {
        Component.Register<UIButton>();
        RequiredComponents.Init<UIButton>();
    }

    public static void CollectRequired(Dictionary<ComponentID, byte[]> collector)
    {
        var selfId = Component.GetInfo<UIButton>().ID;
        if (collector.ContainsKey(selfId)) return;
        collector[selfId] = CollectionUtils.GetBytes(Default);
    }
}
