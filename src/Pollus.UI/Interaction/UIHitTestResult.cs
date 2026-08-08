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
    public bool PointerOverUI;
    public bool ConsumesPointerInput => PointerOverUI || !CapturedEntity.IsNull;
    public Vec2f MousePosition;
    public Vec2f PreviousMousePosition;

    // Reusable buffer for deferred absolute-positioned hit test nodes
    internal readonly List<(Entity entity, Vec2f parentAbsPos, UIClipRect? clip)> DeferredBuffer = [];
}

internal readonly record struct UIClipRect(float Left, float Top, float Right, float Bottom)
{
    public bool Contains(Vec2f point) =>
        point.X >= Left && point.X < Right && point.Y >= Top && point.Y < Bottom;

    public UIClipRect Intersect(UIClipRect other) => new(
        MathF.Max(Left, other.Left),
        MathF.Max(Top, other.Top),
        MathF.Min(Right, other.Right),
        MathF.Min(Bottom, other.Bottom));
}
