namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.Utils;

public record struct UIRenderResources
{
    public Handle<UIRectMaterial> Material;
}

public static class ExtractUIRectsSystem
{
    public const string Label = "ExtractUIRects";

    public static ISystemBuilder Create() => FnSystem.Create(
        new(Label)
        {
            RunsAfter = [RenderingPlugin.BeginFrameSystem],
        },
        static (
            UIRectBatches batches,
            UIRenderResources resources,
            Query query,
            Query<UILayoutRoot, ComputedNode>.Filter<All<UINode>> qRoots) =>
        {
            batches.Reset();

            var batchKey = new UIRectBatchKey(resources.Material);
            var batch = batches.GetOrCreate(batchKey);
            uint sortIndex = 0;

            foreach (var root in qRoots)
            {
                var rootEntity = root.Entity;
                ref readonly var rootComputed = ref root.Component1;

                EmitNode(batch, ref sortIndex, query, rootEntity, rootComputed, Vec2f.Zero);
            }
        }
    );

    static void EmitNode(UIRectBatch batch, ref uint sortIndex, Query query, Entity entity, in ComputedNode computed, Vec2f parentAbsPos)
    {
        var absPos = parentAbsPos + computed.Position;
        var size = computed.Size;
        var nodeIndex = sortIndex++;

        if (size.X > 0 && size.Y > 0)
        {
            var entityRef = query.GetEntity(entity);

            var bgColor = Vec4f.Zero;
            var borderColor = Vec4f.Zero;
            var borderRadius = Vec4f.Zero;
            var borderWidths = new Vec4f(computed.BorderTop, computed.BorderRight, computed.BorderBottom, computed.BorderLeft);

            bool hasBg = false;
            bool hasBorder = false;

            if (entityRef.Has<BackgroundColor>())
            {
                var bg = entityRef.Get<BackgroundColor>();
                bgColor = (Vec4f)bg.Color;
                hasBg = true;
            }

            if (entityRef.Has<BorderColor>())
            {
                var bc = entityRef.Get<BorderColor>();
                // Use average of all border colors for now (single-color shader)
                borderColor = ((Vec4f)bc.Top + (Vec4f)bc.Right + (Vec4f)bc.Bottom + (Vec4f)bc.Left) * 0.25f;
                hasBorder = true;
            }

            if (entityRef.Has<BorderRadius>())
            {
                var br = entityRef.Get<BorderRadius>();
                borderRadius = new Vec4f(br.TopLeft, br.TopRight, br.BottomRight, br.BottomLeft);
            }

            if (hasBg || hasBorder)
            {
                var sortKey = (ulong)nodeIndex * 2;
                batch.Draw(sortKey, new UIRectBatch.InstanceData
                {
                    PosSize = new Vec4f(absPos.X, absPos.Y, size.X, size.Y),
                    BackgroundColor = bgColor,
                    BorderColor = borderColor,
                    BorderRadius = borderRadius,
                    BorderWidths = borderWidths,
                });
            }
        }

        // Walk children via Parent/Child linked list
        var entRef = query.GetEntity(entity);
        if (!entRef.Has<Parent>()) return;

        // Child positions from the flex layout already include the
        // parent's padding+border offset, so we pass absPos directly
        // without adding a content offset.
        var childEntity = entRef.Get<Parent>().FirstChild;
        while (!childEntity.IsNull)
        {
            var childRef = query.GetEntity(childEntity);
            ref var childComputed = ref childRef.Get<ComputedNode>();
            EmitNode(batch, ref sortIndex, query, childEntity, childComputed, absPos);

            if (childRef.Has<Child>())
            {
                childEntity = childRef.Get<Child>().NextSibling;
            }
            else
            {
                break;
            }
        }
    }
}
