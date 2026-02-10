using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.UI.Layout;

namespace Pollus.UI;

public static class UILayoutSystem
{
    public const string SyncTreeLabel = "UILayoutSystem::SyncTree";
    public const string ComputeLayoutLabel = "UILayoutSystem::ComputeLayout";
    public const string WriteBackLabel = "UILayoutSystem::WriteBack";

    public static SystemBuilder SyncTree() => FnSystem.Create(
        new(SyncTreeLabel),
        static (UITreeAdapter adapter, Query query) =>
        {
            adapter.SyncFull(query);
        }
    );

    public static SystemBuilder ComputeLayout() => FnSystem.Create(
        new(ComputeLayoutLabel) { RunsAfter = [SyncTreeLabel] },
        static (UITreeAdapter adapter, Query query) =>
        {
            foreach (var rootNodeId in adapter.Roots)
            {
                var rootEntity = adapter.GetEntity(rootNodeId);
                if (!query.Has<UILayoutRoot>(rootEntity)) continue;

                ref readonly var layoutRoot = ref query.Get<UILayoutRoot>(rootEntity);
                float width = layoutRoot.Size.Width;
                float height = layoutRoot.Size.Height;

                // Check for viewport resize — marks entire subtree dirty
                if (adapter.CheckAndUpdateRootSize(rootEntity, layoutRoot.Size))
                {
                    adapter.MarkSubtreeDirty(rootNodeId);
                }

                var input = new LayoutInput
                {
                    RunMode = RunMode.PerformLayout,
                    SizingMode = SizingMode.InherentSize,
                    Axis = RequestedAxis.Both,
                    KnownDimensions = new Size<float?>(width, height),
                    ParentSize = new Size<float?>(width, height),
                    AvailableSpace = new Size<AvailableSpace>(
                        AvailableSpace.Definite(width),
                        AvailableSpace.Definite(height)
                    ),
                };

                var output = adapter.ComputeChildLayout(rootNodeId, input);

                ref var rootLayout = ref adapter.GetLayout(rootNodeId);
                rootLayout.Size = output.Size;
                rootLayout.ContentSize = output.ContentSize;

                // ComputeFlexbox returns root size but doesn't write its own
                // padding/border/margin — resolve them here for WriteBack.
                ref readonly var rootStyle = ref adapter.GetStyle(rootNodeId);
                var parentSize = new Size<float?>(width, height);
                rootLayout.Padding = LayoutHelpers.ResolvePadding(rootStyle, parentSize);
                rootLayout.Border = LayoutHelpers.ResolveBorder(rootStyle, parentSize);
                rootLayout.Margin = LayoutHelpers.ResolveMargin(rootStyle, parentSize);

                // Snapshot unrounded layout before RoundLayout modifies it
                adapter.GetUnroundedLayout(rootNodeId) = rootLayout;

                var tree = adapter;
                RoundLayout.Round(ref tree, rootNodeId);
            }
        }
    );

    public static SystemBuilder WriteBack() => FnSystem.Create(
        new(WriteBackLabel) { RunsAfter = [ComputeLayoutLabel] },
        static (UITreeAdapter adapter, Query query) =>
        {
            for (int nodeId = 0; nodeId < adapter.NodeCapacity; nodeId++)
            {
                var entity = adapter.GetEntity(nodeId);
                if (entity.IsNull) continue;
                if (!query.Has<ComputedNode>(entity)) continue;

                ref readonly var rounded = ref adapter.GetRoundedLayout(nodeId);
                ref readonly var unrounded = ref adapter.GetUnroundedLayout(nodeId);

                ref var computed = ref query.Get<ComputedNode>(entity);
                computed.Size = new Vec2f(rounded.Size.Width, rounded.Size.Height);
                computed.ContentSize = new Vec2f(rounded.ContentSize.Width, rounded.ContentSize.Height);
                computed.Position = new Vec2f(rounded.Location.X, rounded.Location.Y);
                computed.BorderLeft = rounded.Border.Left;
                computed.BorderRight = rounded.Border.Right;
                computed.BorderTop = rounded.Border.Top;
                computed.BorderBottom = rounded.Border.Bottom;
                computed.PaddingLeft = rounded.Padding.Left;
                computed.PaddingRight = rounded.Padding.Right;
                computed.PaddingTop = rounded.Padding.Top;
                computed.PaddingBottom = rounded.Padding.Bottom;
                computed.MarginLeft = rounded.Margin.Left;
                computed.MarginRight = rounded.Margin.Right;
                computed.MarginTop = rounded.Margin.Top;
                computed.MarginBottom = rounded.Margin.Bottom;
                computed.UnroundedSize = new Vec2f(unrounded.Size.Width, unrounded.Size.Height);
                computed.UnroundedPosition = new Vec2f(unrounded.Location.X, unrounded.Location.Y);
            }
        }
    );
}
