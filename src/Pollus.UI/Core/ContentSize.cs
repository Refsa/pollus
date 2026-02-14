using Pollus.ECS;
using Pollus.UI.Layout;
using Pollus.Utils;

namespace Pollus.UI;

/// Delegate for measuring intrinsic content size (e.g. text, images).
public delegate Size<float> MeasureFunc(
    Size<float?> knownDimensions,
    Size<AvailableSpace> availableSpace);

/// Marker component indicating this node has intrinsic content sizing.
/// The actual measure function is registered via UITreeAdapter.SetMeasureFunc.
public partial record struct ContentSize() : IComponent, IDefault<ContentSize>
{
    public static ContentSize Default { get; } = default;

    static ContentSize()
    {
        Component.Register<ContentSize>();
        RequiredComponents.Init<ContentSize>();
    }

    public static void CollectRequired(Dictionary<ComponentID, byte[]> collector)
    {
        var selfId = Component.GetInfo<ContentSize>().ID;
        if (collector.ContainsKey(selfId)) return;
        collector[selfId] = CollectionUtils.GetBytes(Default);
    }
}
