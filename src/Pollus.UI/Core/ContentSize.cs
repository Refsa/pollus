namespace Pollus.UI;

using Pollus.ECS;
using Pollus.UI.Layout;
using Pollus.Utils;

public delegate Size<float> MeasureFunc(
    Size<float?> knownDimensions,
    Size<AvailableSpace> availableSpace);

/// Marker component indicating this node has intrinsic content sizing.
/// The actual measure function is registered via UITreeAdapter.SetMeasureFunc.
public partial record struct ContentSize() : IComponent
{
    
}
