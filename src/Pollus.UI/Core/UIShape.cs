namespace Pollus.UI;

using Pollus.ECS;

public enum UIShapeType : uint
{
    RoundedRect = 0,
    Circle = 1,
    Checkmark = 2,
    DownArrow = 3,
}

public partial record struct UIShape() : IComponent, IDefault<UIShape>
{
    public static UIShape Default => new() { Type = UIShapeType.RoundedRect };

    public UIShapeType Type = UIShapeType.RoundedRect;
}
