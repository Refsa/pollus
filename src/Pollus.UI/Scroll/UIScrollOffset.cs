namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Mathematics;

public partial record struct UIScrollOffset() : IComponent, IDefault<UIScrollOffset>
{
    public static UIScrollOffset Default => default;

    public Vec2f Offset; // positive = content scrolled up/left
    public Entity VerticalThumbEntity = Entity.Null;
    public Entity HorizontalThumbEntity = Entity.Null;
}
