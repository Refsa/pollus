namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.Utils;

public static class ExtractUITextSystem
{
    public const string Label = "ExtractUIText";

    public static ISystemBuilder Create() => FnSystem.Create(
        new(Label)
        {
            RunsAfter = [ExtractUIRectsSystem.Label],
        },
        static (
            UIFontBatches batches,
            Query query,
            Query<UILayoutRoot, ComputedNode>.Filter<All<UINode>> qRoots) =>
        {
            batches.Reset();

            uint sortIndex = 0;

            foreach (var root in qRoots)
            {
                var rootEntity = root.Entity;
                ref readonly var rootComputed = ref root.Component1;

                EmitNode(batches, ref sortIndex, query, rootEntity, rootComputed, Vec2f.Zero);
            }
        }
    );

    static void EmitNode(UIFontBatches batches, ref uint sortIndex, Query query, Entity entity, in ComputedNode computed, Vec2f parentAbsPos)
    {
        var absPos = parentAbsPos + computed.Position;
        var size = computed.Size;
        var nodeIndex = sortIndex++;

        if (size.X > 0 && size.Y > 0)
        {
            var entityRef = query.GetEntity(entity);

            // Emit text if entity has UIText + TextMesh + UITextFont
            if (entityRef.Has<UIText>() && entityRef.Has<TextMesh>() && entityRef.Has<UITextFont>())
            {
                ref readonly var textMesh = ref entityRef.Get<TextMesh>();
                ref readonly var textFont = ref entityRef.Get<UITextFont>();
                ref readonly var uiText = ref entityRef.Get<UIText>();

                if (textMesh.Mesh != Handle<TextMeshAsset>.Null && textFont.Material != Handle.Null)
                {
                    var batchKey = new UIFontBatchKey(textMesh.Mesh, textFont.Material);
                    var batch = batches.GetOrCreate(batchKey);
                    var sortKey = (ulong)nodeIndex * 4 + 3;

                    // Content offset: text starts inside padding+border
                    var contentOffset = new Vec2f(
                        computed.PaddingLeft + computed.BorderLeft,
                        computed.PaddingTop + computed.BorderTop);

                    batch.Draw(sortKey, absPos + contentOffset, uiText.Color);
                }
            }
        }

        // Walk children via Parent/Child linked list
        var entRef = query.GetEntity(entity);
        if (!entRef.Has<Parent>()) return;

        var childEntity = entRef.Get<Parent>().FirstChild;
        while (!childEntity.IsNull)
        {
            var childRef = query.GetEntity(childEntity);
            ref var childComputed = ref childRef.Get<ComputedNode>();
            EmitNode(batches, ref sortIndex, query, childEntity, childComputed, absPos);

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
