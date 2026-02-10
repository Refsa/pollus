using Pollus.ECS;
using Pollus.Utils;

namespace Pollus.UI;

public partial record struct UINode() : IComponent, IDefault<UINode>
{
    public static UINode Default { get; } = default;

    static UINode()
    {
        Component.Register<UINode>();
        RequiredComponents.Init<UINode>();
    }

    public static void CollectRequired(Dictionary<ComponentID, byte[]> collector)
    {
        var selfId = Component.GetInfo<UINode>().ID;
        if (collector.ContainsKey(selfId)) return;

        collector[selfId] = CollectionUtils.GetBytes(Default);

        UIStyle.CollectRequired(collector);
        ComputedNode.CollectRequired(collector);
    }
}
