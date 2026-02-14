using Pollus.ECS;
using Pollus.Mathematics;

namespace Pollus.UI;

public class UIHitTestResult
{
    public Entity HoveredEntity = Entity.Null;
    public Entity PreviousHoveredEntity = Entity.Null;
    public Entity PressedEntity = Entity.Null;
    public Entity CapturedEntity = Entity.Null;
    public Vec2f MousePosition;
    public Vec2f PreviousMousePosition;
}
