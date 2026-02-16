namespace Pollus.UI;

using Pollus.ECS;

/// <summary>
/// Marker component. When present on a UILayoutRoot entity, the root's size
/// is automatically kept in sync with its target viewport dimensions.
/// </summary>
public partial record struct UIAutoResize() : IComponent;
