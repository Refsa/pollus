using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.Utils;

namespace Pollus.UI;

public partial record struct BackgroundColor() : IComponent, IDefault<BackgroundColor>
{
    public Color Color;

    public static BackgroundColor Default { get; } = default;

    static BackgroundColor()
    {
        Component.Register<BackgroundColor>();
        RequiredComponents.Init<BackgroundColor>();
    }

    public static void CollectRequired(Dictionary<ComponentID, byte[]> collector)
    {
        var selfId = Component.GetInfo<BackgroundColor>().ID;
        if (collector.ContainsKey(selfId)) return;
        collector[selfId] = CollectionUtils.GetBytes(Default);
    }
}

public partial record struct BorderColor() : IComponent, IDefault<BorderColor>
{
    public Color Top;
    public Color Right;
    public Color Bottom;
    public Color Left;

    public static BorderColor Default { get; } = default;

    static BorderColor()
    {
        Component.Register<BorderColor>();
        RequiredComponents.Init<BorderColor>();
    }

    public static void CollectRequired(Dictionary<ComponentID, byte[]> collector)
    {
        var selfId = Component.GetInfo<BorderColor>().ID;
        if (collector.ContainsKey(selfId)) return;
        collector[selfId] = CollectionUtils.GetBytes(Default);
    }
}

public partial record struct BoxShadow() : IComponent, IDefault<BoxShadow>
{
    public Vec2f Offset;
    public float Blur;
    public float Spread;
    public Color Color;

    public static BoxShadow Default { get; } = default;

    static BoxShadow()
    {
        Component.Register<BoxShadow>();
        RequiredComponents.Init<BoxShadow>();
    }

    public static void CollectRequired(Dictionary<ComponentID, byte[]> collector)
    {
        var selfId = Component.GetInfo<BoxShadow>().ID;
        if (collector.ContainsKey(selfId)) return;
        collector[selfId] = CollectionUtils.GetBytes(Default);
    }
}

public partial record struct BorderRadius() : IComponent, IDefault<BorderRadius>
{
    public float TopLeft;
    public float TopRight;
    public float BottomRight;
    public float BottomLeft;

    public static BorderRadius Default { get; } = default;

    static BorderRadius()
    {
        Component.Register<BorderRadius>();
        RequiredComponents.Init<BorderRadius>();
    }

    public static void CollectRequired(Dictionary<ComponentID, byte[]> collector)
    {
        var selfId = Component.GetInfo<BorderRadius>().ID;
        if (collector.ContainsKey(selfId)) return;
        collector[selfId] = CollectionUtils.GetBytes(Default);
    }
}
