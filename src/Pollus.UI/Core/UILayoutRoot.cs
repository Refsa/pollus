using Pollus.ECS;
using Pollus.UI.Layout;
using Pollus.Utils;

namespace Pollus.UI;

public partial record struct UILayoutRoot() : IComponent, IDefault<UILayoutRoot>
{
    public Size<float> Size;

    public static UILayoutRoot Default { get; } = default;

    static UILayoutRoot()
    {
        Component.Register<UILayoutRoot>();
        RequiredComponents.Init<UILayoutRoot>();
    }

    public static void CollectRequired(Dictionary<ComponentID, byte[]> collector)
    {
        var selfId = Component.GetInfo<UILayoutRoot>().ID;
        if (collector.ContainsKey(selfId)) return;
        collector[selfId] = CollectionUtils.GetBytes(Default);
    }
}
