namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Mathematics;

public partial record struct ComputedNode() : IComponent
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
}
