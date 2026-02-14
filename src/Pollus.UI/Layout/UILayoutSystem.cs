using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.UI.Layout;

namespace Pollus.UI;

public class UILayoutSystem : ISystemSet
{
    public const string SyncTreeLabel = "UILayoutSystem::SyncTree";
    public const string ComputeLayoutLabel = "UILayoutSystem::ComputeLayout";
    public const string WriteBackLabel = "UILayoutSystem::WriteBack";

    public static readonly SystemBuilderDescriptor SyncTreeDescriptor = new()
    {
        Label = new SystemLabel(SyncTreeLabel),
        Stage = CoreStage.PostUpdate,
    };

    public static readonly SystemBuilderDescriptor ComputeLayoutDescriptor = new()
    {
        Label = new SystemLabel(ComputeLayoutLabel),
        Stage = CoreStage.PostUpdate,
        RunsAfter = [SyncTreeLabel],
    };

    public static readonly SystemBuilderDescriptor WriteBackDescriptor = new()
    {
        Label = new SystemLabel(WriteBackLabel),
        Stage = CoreStage.PostUpdate,
        RunsAfter = [ComputeLayoutLabel],
    };

    public static void AddToSchedule(Schedule schedule)
    {
        schedule.AddSystems(SyncTreeDescriptor.Stage, FnSystem.Create(SyncTreeDescriptor,
            (SystemDelegate<UITreeAdapter, Query<UINode>, Query>)SyncTree));
        schedule.AddSystems(ComputeLayoutDescriptor.Stage, FnSystem.Create(ComputeLayoutDescriptor,
            (SystemDelegate<UITreeAdapter, Query>)ComputeLayout));
        schedule.AddSystems(WriteBackDescriptor.Stage, FnSystem.Create(WriteBackDescriptor,
            (SystemDelegate<UITreeAdapter, Query>)WriteBack));
    }

    public static void SyncTree(UITreeAdapter adapter, Query<UINode> uiNodeQuery, Query query)
    {
        adapter.SyncFull(uiNodeQuery, query);
    }

    public static void ComputeLayout(UITreeAdapter adapter, Query query)
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

            var output = adapter.ComputeChildLayout(rootNodeId, input);

            // ComputeFlexbox doesn't write the root's own layout -- resolve here.
            var rootPadding = LayoutHelpers.ResolvePadding(rootStyle, parentSz);
            var rootBorder = LayoutHelpers.ResolveBorder(rootStyle, parentSz);

            ref var rootLayout = ref adapter.GetLayout(rootNodeId);
            rootLayout.Size = new Size<float>(
                innerW + rootPadding.HorizontalAxisSum() + rootBorder.HorizontalAxisSum(),
                innerH + rootPadding.VerticalAxisSum() + rootBorder.VerticalAxisSum());
            rootLayout.ContentSize = output.ContentSize;
            rootLayout.Padding = rootPadding;
            rootLayout.Border = rootBorder;
            rootLayout.Margin = LayoutHelpers.ResolveMargin(rootStyle, parentSz);

            // Snapshot unrounded layout before RoundLayout modifies it
            adapter.GetUnroundedLayout(rootNodeId) = rootLayout;

            var tree = adapter;
            RoundLayout.Round(ref tree, rootNodeId);
        }
    }

    public static void WriteBack(UITreeAdapter adapter, Query query)
    {
        if (!adapter.IsDirty) return;

        var enumerator = adapter.GetActiveNodes();
        while (enumerator.MoveNext())
        {
            var (entity, nodeId) = enumerator.Current;
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

        adapter.ClearDirty();
    }
}
