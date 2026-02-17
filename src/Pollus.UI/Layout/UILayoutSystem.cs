namespace Pollus.UI;

using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.UI.Layout;

[SystemSet]
public partial class UILayoutSystem
{
    [System(nameof(SyncTree))]
    static readonly SystemBuilderDescriptor SyncTreeDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
    };

    [System(nameof(ComputeLayout))]
    static readonly SystemBuilderDescriptor ComputeLayoutDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UILayoutSystem::SyncTree"],
    };

    [System(nameof(WriteBack))]
    static readonly SystemBuilderDescriptor WriteBackDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = ["UILayoutSystem::ComputeLayout"],
    };

    static void SyncTree(UITreeAdapter adapter, Query<UINode> uiNodeQuery, Query query)
    {
        adapter.SyncFull(uiNodeQuery, query);
    }

    static void ComputeLayout(UITreeAdapter adapter, Query query)
    {
        if (!adapter.IsDirty) return;

        foreach (var rootNodeId in adapter.Roots)
        {
            var rootEntity = adapter.GetEntity(rootNodeId);
            if (!query.Has<UILayoutRoot>(rootEntity)) continue;

            ref readonly var layoutRoot = ref query.Get<UILayoutRoot>(rootEntity);
            float width = layoutRoot.Size.Width;
            float height = layoutRoot.Size.Height;

            // padding+border so the outer size equals the viewport.
            ref readonly var rootStyle = ref adapter.GetStyle(rootNodeId);
            var parentSz = new Size<float?>(width, height);
            var adj = LayoutHelpers.ContentBoxAdjustment(
                rootStyle.BoxSizing,
                LayoutHelpers.ResolvePadding(rootStyle, parentSz),
                LayoutHelpers.ResolveBorder(rootStyle, parentSz));
            float innerW = width + (adj.Width ?? 0f);
            float innerH = height + (adj.Height ?? 0f);

            var input = new LayoutInput
            {
                RunMode = RunMode.PerformLayout,
                SizingMode = SizingMode.InherentSize,
                Axis = RequestedAxis.Both,
                KnownDimensions = new Size<float?>(innerW, innerH),
                ParentSize = new Size<float?>(width, height),
                AvailableSpace = new Size<AvailableSpace>(
                    AvailableSpace.Definite(width),
                    AvailableSpace.Definite(height)
                ),
            };

            // Use UITreeRef struct for AOT-friendly devirtualized layout calls
            var treeRef = new UITreeRef(adapter);
            var output = FlexLayout.ComputeFlexbox(ref treeRef, rootNodeId, input);

            // ComputeFlexbox doesn't write the root's own layout -- resolve here.
            var rootPadding = LayoutHelpers.ResolvePadding(rootStyle, parentSz);
            var rootBorder = LayoutHelpers.ResolveBorder(rootStyle, parentSz);

            var rootLayout = new NodeLayout
            {
                Size = new Size<float>(
                    innerW + rootPadding.HorizontalAxisSum() + rootBorder.HorizontalAxisSum(),
                    innerH + rootPadding.VerticalAxisSum() + rootBorder.VerticalAxisSum()),
                ContentSize = output.ContentSize,
                Padding = rootPadding,
                Border = rootBorder,
                Margin = LayoutHelpers.ResolveMargin(rootStyle, parentSz),
            };
            adapter.SetUnroundedLayout(rootNodeId, in rootLayout);

            treeRef = new UITreeRef(adapter);
            RoundLayout.Round(ref treeRef, rootNodeId);
        }
    }

    static void WriteBack(UITreeAdapter adapter, Query query)
    {
        if (!adapter.IsDirty) return;

        foreach (var entity in adapter.ActiveEntities)
        {
            int nodeId = adapter.GetNodeId(entity);
            if (nodeId < 0) continue;
            if (!adapter.LayoutChanged(nodeId)) continue;
            if (!query.Has<ComputedNode>(entity)) continue;

            ref readonly var rounded = ref adapter.GetRoundedLayout(nodeId);
            ref readonly var unrounded = ref adapter.GetUnroundedLayout(nodeId);

            ref var computed = ref query.GetTracked<ComputedNode>(entity);
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

        adapter.ClearDirty();
    }
}
