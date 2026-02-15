namespace Pollus.UI;

using System.Collections.Generic;
using Pollus.ECS;
using Pollus.Mathematics;

public class UIHitTestResult
{
    public Entity HoveredEntity = Entity.Null;
    public Entity PreviousHoveredEntity = Entity.Null;
    public Entity PressedEntity = Entity.Null;
    public Entity CapturedEntity = Entity.Null;
    public Vec2f MousePosition;
    public Vec2f PreviousMousePosition;

    // Reusable buffer for deferred absolute-positioned hit test nodes
    internal readonly List<(Entity entity, Vec2f parentAbsPos)> DeferredBuffer = [];
}
