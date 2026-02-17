namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.Utils;

public partial record struct BackgroundColor() : IComponent
{
    public Color Color;
}

public partial record struct BorderColor() : IComponent
{
    public Color Top;
    public Color Right;
    public Color Bottom;
    public Color Left;
}

public partial record struct BoxShadow() : IComponent
{
    public Vec2f Offset;
    public float Blur;
    public float Spread;
    public Color Color;
}

public partial record struct Outline() : IComponent
{
    public Color Color = Color.TRANSPARENT;
    public float Width;
    public float Offset;
}

public partial record struct BorderRadius() : IComponent
{
    public float TopLeft;
    public float TopRight;
    public float BottomRight;
    public float BottomLeft;
}
