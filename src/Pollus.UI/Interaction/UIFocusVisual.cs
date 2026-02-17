namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Utils;

public partial record struct UIFocusVisual() : IComponent
{
    /// <summary>Override color. Default (transparent) = use global style.</summary>
    public Color Color = Color.TRANSPARENT;
    /// <summary>Override width. -1 = use global style.</summary>
    public float Width = -1f;
    /// <summary>Override offset. -1 = use global style.</summary>
    public float Offset = -1f;
    public bool Disabled;
    /// <summary>Redirect outline to a different entity. Entity.Null = self.</summary>
    public Entity Target = Entity.Null;

    public bool HasColor => Color.A > 0f;
    public bool HasWidth => Width >= 0f;
    public bool HasOffset => Offset >= 0f;
}
