using Pollus.ECS;
using Pollus.Utils;
using Pollus.UI.Layout;

namespace Pollus.UI;

public partial record struct UIStyle() : IComponent, IDefault<UIStyle>
{
    public Layout.Style Value = Layout.Style.Default;

    public static UIStyle Default { get; } = new();

    static UIStyle()
    {
        Component.Register<UIStyle>();
        RequiredComponents.Init<UIStyle>();
    }

    public static void CollectRequired(Dictionary<ComponentID, byte[]> collector)
    {
        var selfId = Component.GetInfo<UIStyle>().ID;
        if (collector.ContainsKey(selfId)) return;
        collector[selfId] = CollectionUtils.GetBytes(Default);
    }
}
