using Pollus.ECS;
using Pollus.Utils;

namespace Pollus.UI;

public partial record struct UIToggle() : IComponent, IDefault<UIToggle>
{
    public bool IsOn;
    public Color OnColor = new(0.2f, 0.7f, 0.2f, 1f);
    public Color OffColor = new(0.8f, 0.8f, 0.8f, 1f);

    public static UIToggle Default { get; } = new();

    static UIToggle()
    {
        Component.Register<UIToggle>();
        RequiredComponents.Init<UIToggle>();
    }

    public static void CollectRequired(Dictionary<ComponentID, byte[]> collector)
    {
        var selfId = Component.GetInfo<UIToggle>().ID;
        if (collector.ContainsKey(selfId)) return;
        collector[selfId] = CollectionUtils.GetBytes(Default);
    }
}

public static class UIToggleEvents
{
    public struct UIToggleEvent
    {
        public Entity Entity;
        public bool IsOn;
    }
}
