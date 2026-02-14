using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.Utils;

namespace Pollus.UI;

public partial record struct ComputedNode() : IComponent, IDefault<ComputedNode>
{
    public Vec2f Size;
    public Vec2f ContentSize;
    public Vec2f Position;
    public float BorderLeft;
    public float BorderRight;
    public float BorderTop;
    public float BorderBottom;
    public float PaddingLeft;
    public float PaddingRight;
    public float PaddingTop;
    public float PaddingBottom;
    public float MarginLeft;
    public float MarginRight;
    public float MarginTop;
    public float MarginBottom;
    public Vec2f UnroundedSize;
    public Vec2f UnroundedPosition;

    public static ComputedNode Default { get; } = default;

    static ComputedNode()
    {
        Component.Register<ComputedNode>();
        RequiredComponents.Init<ComputedNode>();
    }

    public static void CollectRequired(Dictionary<ComponentID, byte[]> collector)
    {
        var selfId = Component.GetInfo<ComputedNode>().ID;
        if (collector.ContainsKey(selfId)) return;
        collector[selfId] = CollectionUtils.GetBytes(Default);
    }
}
