namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Input;
using Pollus.Mathematics;
using Pollus.UI.Layout;

public class UIScrollSystem : ISystemSet
{
    const float ScrollSpeed = 30f;

    public static readonly SystemBuilderDescriptor UpdateDescriptor = new()
    {
        Label = new SystemLabel("UIScrollSystem::Update"),
        Stage = CoreStage.PostUpdate,
        RunsAfter = [UIInteractionSystem.HitTestLabel],
    };

    public static void AddToSchedule(Schedule schedule)
    {
        schedule.AddSystems(UpdateDescriptor.Stage, FnSystem.Create(UpdateDescriptor,
            (SystemDelegate<CurrentDevice<Mouse>, UIHitTestResult, Query, Query<UIScrollOffset, ComputedNode>>)Update));
    }

    public static void Update(
        CurrentDevice<Mouse> currentMouse,
        UIHitTestResult hitResult,
        Query query,
        Query<UIScrollOffset, ComputedNode> qScroll)
    {
        var mouse = currentMouse.Value;
        if (mouse == null) return;

        var scrollY = mouse.GetAxis(MouseAxis.ScrollY);
        if (scrollY == 0f) return;

        // Find the nearest ancestor (or self) with UIScrollOffset starting from hovered entity
        var target = hitResult.HoveredEntity;
        if (target.IsNull)
        {
            // Fallback: check if mouse is within any scroll container's bounds
            var mousePos = hitResult.MousePosition;
            foreach (var row in qScroll)
            {
                ref readonly var computed = ref row.Component1;
                var absPos = ComputeAbsolutePosition(query, row.Entity);
                if (mousePos.X >= absPos.X && mousePos.X < absPos.X + computed.Size.X &&
                    mousePos.Y >= absPos.Y && mousePos.Y < absPos.Y + computed.Size.Y)
                {
                    target = row.Entity;
                    break;
                }
            }
            if (target.IsNull) return;
        }

        var scrollEntity = FindScrollAncestor(query, target);
        if (scrollEntity.IsNull) return;

        ref var scrollOffset = ref query.Get<UIScrollOffset>(scrollEntity);
        ref readonly var scrollComputed = ref query.Get<ComputedNode>(scrollEntity);

        var innerHeight = scrollComputed.Size.Y - scrollComputed.PaddingTop - scrollComputed.PaddingBottom
            - scrollComputed.BorderTop - scrollComputed.BorderBottom;
        var maxScrollY = MathF.Max(0, scrollComputed.ContentSize.Y - innerHeight);
        scrollOffset.Offset.Y = Math.Clamp(scrollOffset.Offset.Y - scrollY * ScrollSpeed, 0f, maxScrollY);
    }

    static Vec2f ComputeAbsolutePosition(Query query, Entity entity)
    {
        var pos = Vec2f.Zero;
        var current = entity;
        while (!current.IsNull)
        {
            if (query.Has<ComputedNode>(current))
                pos += query.Get<ComputedNode>(current).Position;

            if (query.Has<Child>(current))
                current = query.Get<Child>(current).Parent;
            else
                break;
        }
        return pos;
    }

    static Entity FindScrollAncestor(Query query, Entity entity)
    {
        var current = entity;
        while (!current.IsNull)
        {
            if (query.Has<UIScrollOffset>(current))
                return current;

            if (query.Has<Child>(current))
                current = query.Get<Child>(current).Parent;
            else
                break;
        }
        return Entity.Null;
    }
}
