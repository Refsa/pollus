namespace Pollus.UI.Layout;

using System.Buffers;
using System.Runtime.CompilerServices;

public static partial class FlexLayout
{
    #region Internal Types

    private struct FlexItem
    {
        public int NodeId;
        public int OrderIndex;
        public int CssOrder;

        public Rect<float> Padding;
        public Rect<float> Border;
        public Rect<float> Margin;

        public float InnerFlexBasis;
        public float TargetMainSize;
        public float HypotheticalCrossSize;
        public float TargetCrossSize;
        public float OuterTargetMainSize;

        public float FlexGrow;
        public float FlexShrink;
        public float ScaledShrinkFactor;
        public bool Frozen;
        public bool ViolationIsMin;
        public bool ViolationIsMax;

        public float MinMain;
        public float MaxMain;
        public float MinCross;
        public float MaxCross;

        public float ContentBoxMainAdj;
        public float ContentBoxCrossAdj;

        public float? FirstBaseline;
        public Point<float?> MeasuredFirstBaselines;

        public bool MarginMainStartAuto;
        public bool MarginMainEndAuto;
        public bool MarginCrossStartAuto;
        public bool MarginCrossEndAuto;

        public float OffsetMain;
        public float OffsetCross;

        public bool CrossSizeIsAuto;

        public Length FlexBasisStyle;
        public Size<Length> SizeStyle;
        public AlignSelf? AlignSelf;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float MarginMainAxisSum(FlexDirection dir) => Margin.MainAxisSum(dir);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float MarginCrossAxisSum(FlexDirection dir) => Margin.CrossAxisSum(dir);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float PaddingBorderMainAxisSum(FlexDirection dir) =>
            Padding.MainAxisSum(dir) + Border.MainAxisSum(dir);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float PaddingBorderCrossAxisSum(FlexDirection dir) =>
            Padding.CrossAxisSum(dir) + Border.CrossAxisSum(dir);
    }

    private struct FlexLine
    {
        public int StartIndex;
        public int Count;
        public float CrossSize;
        public float OffsetCross;
        public float MaxAboveBaseline;
    }

    #endregion

    #region ComputeFlexbox

    public static LayoutOutput ComputeFlexbox<TTree>(ref TTree tree, int nodeId, LayoutInput input)
        where TTree : ILayoutTree
    {
        if (tree.TryCacheGet(nodeId, in input, out var cached))
            return cached;

        ref readonly var style = ref tree.GetStyle(nodeId);

        #region Phase 1: Initial setup

        var dir = style.FlexDirection;
        var isRow = dir.IsRow();
        var isWrap = style.FlexWrap != FlexWrap.NoWrap;
        var isWrapReverse = style.FlexWrap == FlexWrap.WrapReverse;

        var parentSize = input.ParentSize;

        var padding = LayoutHelpers.ResolvePadding(in style, parentSize);
        var border = LayoutHelpers.ResolveBorder(in style, parentSize);
        var paddingBorder = new Rect<float>(
            padding.Left + border.Left,
            padding.Right + border.Right,
            padding.Top + border.Top,
            padding.Bottom + border.Bottom
        );
        var paddingBorderSum = paddingBorder.SumAxes();

        var contentBoxAdj = LayoutHelpers.ContentBoxAdjustment(style.BoxSizing, padding, border);

        var knownDimensions = input.KnownDimensions;

        var styleSize = style.Size.MaybeResolveNullable(parentSize);
        var styleMinSize = style.MinSize.MaybeResolveNullable(parentSize);
        var styleMaxSize = style.MaxSize.MaybeResolveNullable(parentSize);

        var adjustedStyleSize = new Size<float?>(
            LayoutHelpers.MaybeAdd(styleSize.Width, contentBoxAdj.Width),
            LayoutHelpers.MaybeAdd(styleSize.Height, contentBoxAdj.Height)
        );
        var adjustedMinSize = new Size<float?>(
            MaybeMaxZero(LayoutHelpers.MaybeAdd(styleMinSize.Width, contentBoxAdj.Width)),
            MaybeMaxZero(LayoutHelpers.MaybeAdd(styleMinSize.Height, contentBoxAdj.Height))
        );
        var adjustedMaxSize = new Size<float?>(
            LayoutHelpers.MaybeAdd(styleMaxSize.Width, contentBoxAdj.Width),
            LayoutHelpers.MaybeAdd(styleMaxSize.Height, contentBoxAdj.Height)
        );

        var nodeInnerWidth = knownDimensions.Width
                             ?? LayoutHelpers.MaybeClamp(adjustedStyleSize.Width, adjustedMinSize.Width, adjustedMaxSize.Width);
        var nodeInnerHeight = knownDimensions.Height
                              ?? LayoutHelpers.MaybeClamp(adjustedStyleSize.Height, adjustedMinSize.Height, adjustedMaxSize.Height);

        var nodeInnerSize = new Size<float?>(nodeInnerWidth, nodeInnerHeight);
        var nodeOuterSize = new Size<float?>(
            nodeInnerWidth.HasValue ? nodeInnerWidth.Value + paddingBorderSum.Width : null,
            nodeInnerHeight.HasValue ? nodeInnerHeight.Value + paddingBorderSum.Height : null
        );

        var containerMainInnerSize = nodeInnerSize.Main(dir);
        var containerCrossInnerSize = nodeInnerSize.Cross(dir);

        // Scroll container: override available space on scroll axes
        var overflowMain = style.Overflow.Main(dir);
        var overflowCross = style.Overflow.Cross(dir);
        bool scrollMain = overflowMain == Overflow.Scroll;
        bool scrollCross = overflowCross == Overflow.Scroll;

        const float DefaultScrollbarWidth = 16f;
        var scrollbarSize = new Size<float>(
            style.Overflow.Y == Overflow.Scroll ? DefaultScrollbarWidth : 0f,
            style.Overflow.X == Overflow.Scroll ? DefaultScrollbarWidth : 0f);

        var availableSpaceMain = scrollMain
            ? AvailableSpace.MaxContent
            : input.AvailableSpace.Main(dir).MaybeSet(containerMainInnerSize);
        var availableSpaceCross = scrollCross
            ? AvailableSpace.MaxContent
            : input.AvailableSpace.Cross(dir).MaybeSet(containerCrossInnerSize);

        // For scroll axes, children see unbounded space — clear the definite inner size
        var childContainerMainInnerSize = scrollMain ? null : containerMainInnerSize;
        var childContainerCrossInnerSize = scrollCross ? null : containerCrossInnerSize;

        float mainGap = style.Gap.Main(dir).ResolveOr(childContainerMainInnerSize ?? 0f, 0f);
        float crossGap = style.Gap.Cross(dir).ResolveOr(childContainerCrossInnerSize ?? 0f, 0f);

        #endregion

        #region Phase 2: Generate flex items

        var childIds = tree.GetChildIds(nodeId);
        int childCount = childIds.Length;

        if (style.Display == Display.None)
        {
            for (int i = 0; i < childCount; i++)
            {
                tree.SetUnroundedLayout(childIds[i], NodeLayout.Zero);
            }

            tree.CacheStore(nodeId, in input, LayoutOutput.Zero);
            return LayoutOutput.Zero;
        }

        if (childCount == 0)
        {
            if (tree.HasMeasureFunc(nodeId))
            {
                var measured = tree.Measure(nodeId, input);
                var measuredSize = new Size<float>(
                    Clamp(measured.Size.Width, adjustedMinSize.Width, adjustedMaxSize.Width),
                    Clamp(measured.Size.Height, adjustedMinSize.Height, adjustedMaxSize.Height)
                );
                var measuredResult = new LayoutOutput
                {
                    Size = measuredSize,
                    ContentSize = measured.ContentSize,
                    FirstBaselines = measured.FirstBaselines,
                };
                tree.CacheStore(nodeId, in input, in measuredResult);
                return measuredResult;
            }

            var emptySize = new Size<float>(
                knownDimensions.Width ?? Clamp(
                    paddingBorderSum.Width,
                    adjustedMinSize.Width,
                    adjustedMaxSize.Width),
                knownDimensions.Height ?? Clamp(
                    paddingBorderSum.Height,
                    adjustedMinSize.Height,
                    adjustedMaxSize.Height)
            );
            var emptyResult = LayoutOutput.FromOuterSize(emptySize);
            tree.CacheStore(nodeId, in input, in emptyResult);
            return emptyResult;
        }

        var flexItemsArray = ArrayPool<FlexItem>.Shared.Rent(childCount);
        var absoluteItemIds = ArrayPool<int>.Shared.Rent(childCount);
        var absoluteItemOrders = ArrayPool<int>.Shared.Rent(childCount);
        int flexItemCount = 0;
        int absoluteCount = 0;

        try
        {
            for (int i = 0; i < childCount; i++)
            {
                int childId = childIds[i];
                ref readonly var childStyle = ref tree.GetStyle(childId);

                if (childStyle.Display == Display.None)
                {
                    tree.SetUnroundedLayout(childId, NodeLayout.Zero);
                    continue;
                }

                if (childStyle.Position == Position.Absolute)
                {
                    absoluteItemOrders[absoluteCount] = i;
                    absoluteItemIds[absoluteCount++] = childId;
                    continue;
                }

                var childPadding = LayoutHelpers.ResolvePadding(in childStyle, nodeInnerSize);
                var childBorder = LayoutHelpers.ResolveBorder(in childStyle, nodeInnerSize);
                var childMargin = LayoutHelpers.ResolveMargin(in childStyle, nodeInnerSize);
                var childContentBoxAdj = LayoutHelpers.ContentBoxAdjustment(childStyle.BoxSizing, childPadding, childBorder);

                var childMinSize = childStyle.MinSize.MaybeResolveNullable(nodeInnerSize);
                var childMaxSize = childStyle.MaxSize.MaybeResolveNullable(nodeInnerSize);

                float childMinMain = MathF.Max(0f, LayoutHelpers.MaybeAdd(childMinSize.Main(dir), childContentBoxAdj.Main(dir)) ?? 0f);
                float childMaxMain = LayoutHelpers.MaybeAdd(childMaxSize.Main(dir), childContentBoxAdj.Main(dir)) ?? float.PositiveInfinity;
                float childMinCross = MathF.Max(0f, LayoutHelpers.MaybeAdd(childMinSize.Cross(dir), childContentBoxAdj.Cross(dir)) ?? 0f);
                float childMaxCross = LayoutHelpers.MaybeAdd(childMaxSize.Cross(dir), childContentBoxAdj.Cross(dir)) ?? float.PositiveInfinity;

                bool crossIsAuto = childStyle.Size.Cross(dir).IsAuto();

                bool marginMainStartAuto, marginMainEndAuto, marginCrossStartAuto, marginCrossEndAuto;
                if (isRow)
                {
                    marginMainStartAuto = dir.IsReverse()
                        ? childStyle.Margin.Right.IsAuto()
                        : childStyle.Margin.Left.IsAuto();
                    marginMainEndAuto = dir.IsReverse()
                        ? childStyle.Margin.Left.IsAuto()
                        : childStyle.Margin.Right.IsAuto();
                    marginCrossStartAuto = childStyle.Margin.Top.IsAuto();
                    marginCrossEndAuto = childStyle.Margin.Bottom.IsAuto();
                }
                else
                {
                    marginMainStartAuto = dir.IsReverse()
                        ? childStyle.Margin.Bottom.IsAuto()
                        : childStyle.Margin.Top.IsAuto();
                    marginMainEndAuto = dir.IsReverse()
                        ? childStyle.Margin.Top.IsAuto()
                        : childStyle.Margin.Bottom.IsAuto();
                    marginCrossStartAuto = childStyle.Margin.Left.IsAuto();
                    marginCrossEndAuto = childStyle.Margin.Right.IsAuto();
                }

                flexItemsArray[flexItemCount++] = new FlexItem
                {
                    NodeId = childId,
                    OrderIndex = i,
                    CssOrder = childStyle.Order,
                    Padding = childPadding,
                    Border = childBorder,
                    Margin = childMargin,
                    FlexGrow = childStyle.FlexGrow,
                    FlexShrink = childStyle.FlexShrink,
                    MinMain = childMinMain,
                    MaxMain = childMaxMain,
                    MinCross = childMinCross,
                    MaxCross = childMaxCross,
                    ContentBoxMainAdj = childContentBoxAdj.Main(dir) ?? 0f,
                    ContentBoxCrossAdj = childContentBoxAdj.Cross(dir) ?? 0f,
                    CrossSizeIsAuto = crossIsAuto,
                    MarginMainStartAuto = marginMainStartAuto,
                    MarginMainEndAuto = marginMainEndAuto,
                    MarginCrossStartAuto = marginCrossStartAuto,
                    MarginCrossEndAuto = marginCrossEndAuto,
                    FlexBasisStyle = childStyle.FlexBasis,
                    SizeStyle = childStyle.Size,
                    AlignSelf = childStyle.AlignSelf,
                };
            }

            var flexItems = flexItemsArray.AsSpan(0, flexItemCount);

            StableSort(flexItems);

            #endregion

            #region Phase 3: Determine flex base size

            DetermineFlexBaseSize(ref tree, flexItems, dir,
                childContainerMainInnerSize, childContainerCrossInnerSize,
                availableSpaceMain, availableSpaceCross, nodeInnerSize, isRow);

            #endregion

            #region Phase 4: Collect items into flex lines

            FlexLine[]? linesArray = null;
            int lineCount = 0;
            scoped Span<FlexLine> lines;

            Span<FlexLine> singleLineBuffer = stackalloc FlexLine[1];

            if (flexItemCount == 0)
            {
                singleLineBuffer[0] = new FlexLine { StartIndex = 0, Count = 0 };
                lines = singleLineBuffer;
                lineCount = 1;
            }
            else if (!isWrap)
            {
                singleLineBuffer[0] = new FlexLine { StartIndex = 0, Count = flexItemCount };
                lines = singleLineBuffer;
                lineCount = 1;
            }
            else
            {
                linesArray = ArrayPool<FlexLine>.Shared.Rent(flexItemCount);
                float? mainAvailForWrap = availableSpaceMain.AsDefinite();
                int lineStart = 0;
                float lineMainSize = 0f;

                for (int i = 0; i < flexItemCount; i++)
                {
                    ref var item = ref flexItems[i];
                    float outerBasis = item.InnerFlexBasis + item.MarginMainAxisSum(dir)
                                                           + item.PaddingBorderMainAxisSum(dir);

                    if (i > lineStart)
                    {
                        float gapForItem = mainGap;
                        float newSize = lineMainSize + gapForItem + outerBasis;

                        if (mainAvailForWrap.HasValue && newSize > mainAvailForWrap.Value)
                        {
                            linesArray[lineCount++] = new FlexLine
                            {
                                StartIndex = lineStart,
                                Count = i - lineStart,
                            };
                            lineStart = i;
                            lineMainSize = outerBasis;
                            continue;
                        }

                        lineMainSize = newSize;
                    }
                    else
                    {
                        lineMainSize = outerBasis;
                    }
                }

                linesArray[lineCount++] = new FlexLine
                {
                    StartIndex = lineStart,
                    Count = flexItemCount - lineStart,
                };
                lines = linesArray.AsSpan(0, lineCount);
            }

            #endregion


            try
            {
                #region Phase 5: Resolve flexible lengths

                float totalMainContent = 0f;
                for (int li = 0; li < lines.Length; li++)
                {
                    ref var line = ref lines[li];
                    var lineItems = flexItems.Slice(line.StartIndex, line.Count);
                    float lineMain = ResolveFlexibleLengths(lineItems, dir, mainGap, childContainerMainInnerSize);
                    if (lineMain > totalMainContent)
                        totalMainContent = lineMain;
                }

                #endregion

                #region Phases 6 + 7: Determine cross sizes

                var alignContent = style.AlignContent ?? (lineCount == 1 ? AlignContent.Stretch : AlignContent.FlexStart);
                var defaultAlignItems = style.AlignItems ?? AlignItems.Stretch;

                float totalCrossContent = DetermineCrossSizes(ref tree, lines, flexItems, dir,
                    childContainerCrossInnerSize, availableSpaceCross, nodeInnerSize, isRow,
                    defaultAlignItems, alignContent, lineCount, crossGap);

                #endregion

                #region Phase 8: Determine container size

                float containerMainSize = containerMainInnerSize
                                          ?? MathF.Max(MathF.Min(
                                                  totalMainContent,
                                                  adjustedMaxSize.Main(dir) ?? float.PositiveInfinity),
                                              adjustedMinSize.Main(dir) ?? 0f);

                float containerCrossSize = containerCrossInnerSize
                                           ?? MathF.Max(MathF.Min(
                                                   totalCrossContent,
                                                   adjustedMaxSize.Cross(dir) ?? float.PositiveInfinity),
                                               adjustedMinSize.Cross(dir) ?? 0f);

                var containerOuterMainSize = containerMainSize + paddingBorderSum.Main(dir);
                var containerOuterCrossSize = containerCrossSize + paddingBorderSum.Cross(dir);

                // Compute propagated baselines from first flex item
                var containerFirstBaselines = Point<float?>.Zero;
                if (flexItemCount > 0)
                {
                    ref var firstItem = ref flexItems[0];
                    float firstLocMain = paddingBorder.MainStart(dir) + firstItem.Margin.MainStart(dir);
                    float firstLocCross = paddingBorder.CrossStart(dir) + firstItem.Margin.CrossStart(dir);

                    var firstLoc = Point<float>.Zero
                        .WithMain(dir, firstLocMain)
                        .WithCross(dir, firstLocCross);

                    float? bx = firstItem.MeasuredFirstBaselines.X.HasValue
                        ? firstLoc.X + firstItem.Border.Left + firstItem.Padding.Left
                          + firstItem.MeasuredFirstBaselines.X.Value
                        : null;
                    float? by = firstItem.MeasuredFirstBaselines.Y.HasValue
                        ? firstLoc.Y + firstItem.Border.Top + firstItem.Padding.Top
                          + firstItem.MeasuredFirstBaselines.Y.Value
                        : null;
                    containerFirstBaselines = new Point<float?>(bx, by);
                }

                if (input.RunMode == RunMode.ComputeSize)
                {
                    var sizeResult = Size<float>.FromMainCross(dir,
                        containerOuterMainSize, containerOuterCrossSize);
                    var computeSizeResult = new LayoutOutput
                    {
                        Size = sizeResult,
                        ContentSize = Size<float>.Zero,
                        FirstBaselines = containerFirstBaselines,
                        ScrollbarSize = scrollbarSize,
                    };
                    tree.CacheStore(nodeId, in input, in computeSizeResult);
                    return computeSizeResult;
                }

                #endregion

                #region Phases 9 + 10: Position and align items

                var justifyContent = style.JustifyContent ?? JustifyContent.FlexStart;

                PositionAndAlignItems(lines, flexItems, dir,
                    justifyContent, containerMainSize, mainGap,
                    alignContent, defaultAlignItems, lineCount,
                    containerCrossSize, totalCrossContent, crossGap,
                    paddingBorder, isWrapReverse);

                #endregion

                #region Phase 11: Perform final layout

                PerformFinalLayout(ref tree, flexItems, dir, nodeInnerSize,
                    ref containerFirstBaselines, out float maxContentMain, out float maxContentCross);

                #endregion

                #region Phase 12: Absolutely-positioned children

                LayoutAbsoluteChildren(ref tree, in style,
                    absoluteItemIds, absoluteItemOrders, absoluteCount,
                    dir, isRow, nodeInnerSize, nodeOuterSize,
                    containerMainSize, containerCrossSize,
                    paddingBorder, paddingBorderSum,
                    availableSpaceMain, availableSpaceCross);

                #endregion

                #region Phase 13: Final container size

                var containerSize = Size<float>.FromMainCross(dir,
                    containerOuterMainSize, containerOuterCrossSize);

                var contentSize = Size<float>.FromMainCross(dir,
                    MathF.Max(maxContentMain, 0f),
                    MathF.Max(maxContentCross, 0f));

                var finalResult = new LayoutOutput
                {
                    Size = containerSize,
                    ContentSize = contentSize,
                    FirstBaselines = containerFirstBaselines,
                    ScrollbarSize = scrollbarSize,
                };
                tree.CacheStore(nodeId, in input, in finalResult);
                return finalResult;
            }
            finally
            {
                if (linesArray is not null)
                    ArrayPool<FlexLine>.Shared.Return(linesArray);
            }
        }
        finally
        {
            ArrayPool<FlexItem>.Shared.Return(flexItemsArray);
            ArrayPool<int>.Shared.Return(absoluteItemIds);
            ArrayPool<int>.Shared.Return(absoluteItemOrders);
        }

        #endregion
    }

    #endregion

    #region Phase 3: Determine flex base size

    private static void DetermineFlexBaseSize<TTree>(
        ref TTree tree, Span<FlexItem> flexItems, FlexDirection dir,
        float? childContainerMainInnerSize, float? childContainerCrossInnerSize,
        AvailableSpace availableSpaceMain, AvailableSpace availableSpaceCross,
        Size<float?> nodeInnerSize, bool isRow)
        where TTree : ILayoutTree
    {
        for (int i = 0; i < flexItems.Length; i++)
        {
            ref var item = ref flexItems[i];

            float pbMain = item.PaddingBorderMainAxisSum(dir);

            float? flexBasisResolved = item.FlexBasisStyle.IsAuto()
                ? null
                : LayoutHelpers.MaybeAdd(
                    item.FlexBasisStyle.Resolve(childContainerMainInnerSize ?? 0f),
                    item.ContentBoxMainAdj);

            float? styledMainSize = flexBasisResolved
                                    ?? LayoutHelpers.MaybeAdd(
                                        item.SizeStyle.Main(dir).Resolve(childContainerMainInnerSize ?? 0f),
                                        item.ContentBoxMainAdj);

            float flexBasis;
            if (styledMainSize.HasValue)
            {
                flexBasis = MathF.Max(styledMainSize.Value, 0f);
            }
            else
            {
                var childAvailCross = availableSpaceCross
                    .MaybeSet(LayoutHelpers.MaybeAdd(
                        item.SizeStyle.Cross(dir).Resolve(childContainerCrossInnerSize ?? 0f),
                        item.ContentBoxCrossAdj));

                var measureInput = new LayoutInput
                {
                    RunMode = RunMode.ComputeSize,
                    SizingMode = SizingMode.ContentSize,
                    Axis = isRow ? RequestedAxis.Horizontal : RequestedAxis.Vertical,
                    KnownDimensions = Size<float?>.Zero,
                    ParentSize = nodeInnerSize,
                    AvailableSpace = Size<AvailableSpace>.FromMainCross(dir,
                        AvailableSpace.MaxContent,
                        childAvailCross),
                };

                var measured = tree.ComputeChildLayout(item.NodeId, measureInput);
                // ComputeFlexbox returns outer sizes for non-leaf nodes.
                // Convert to content size since PaddingBorder is added back later.
                flexBasis = MathF.Max(measured.Size.Main(dir) - pbMain, 0f);
            }

            item.InnerFlexBasis = MathF.Max(MathF.Min(flexBasis, item.MaxMain), item.MinMain);
            item.ScaledShrinkFactor = item.InnerFlexBasis * item.FlexShrink;
        }
    }

    #endregion

    #region Phases 6 + 7: Determine cross sizes

    private static float DetermineCrossSizes<TTree>(
        ref TTree tree, Span<FlexLine> lines, Span<FlexItem> flexItems, FlexDirection dir,
        float? childContainerCrossInnerSize, AvailableSpace availableSpaceCross,
        Size<float?> nodeInnerSize, bool isRow,
        AlignItems defaultAlignItems, AlignContent alignContent,
        int lineCount, float crossGap)
        where TTree : ILayoutTree
    {
        // Phase 6 + 7: Compute hypothetical cross sizes and line cross metrics in one pass
        for (int li = 0; li < lines.Length; li++)
        {
            ref var line = ref lines[li];
            var lineItems = flexItems.Slice(line.StartIndex, line.Count);

            if (lineItems.Length == 0)
            {
                line.CrossSize = 0f;
                continue;
            }

            float maxCross = 0f;
            float maxAbove = 0f;
            float maxBelow = 0f;
            bool hasBaselineItem = false;

            for (int i = 0; i < lineItems.Length; i++)
            {
                ref var item = ref lineItems[i];

                // --- Hypothetical cross size (was Phase 6) ---
                float? childCrossKnown = LayoutHelpers.MaybeAdd(
                    item.SizeStyle.Cross(dir).Resolve(childContainerCrossInnerSize ?? 0f),
                    item.ContentBoxCrossAdj);

                var childKnown = Size<float?>.FromMainCross(dir, item.TargetMainSize, childCrossKnown);

                var childAvailMain = AvailableSpace.Definite(
                    item.TargetMainSize + item.PaddingBorderMainAxisSum(dir));
                var childAvailCross = availableSpaceCross.MaybeSet(childCrossKnown.HasValue
                    ? childCrossKnown.Value + item.PaddingBorderCrossAxisSum(dir)
                    : null);

                var measureInput = new LayoutInput
                {
                    RunMode = RunMode.ComputeSize,
                    SizingMode = SizingMode.ContentSize,
                    Axis = isRow ? RequestedAxis.Vertical : RequestedAxis.Horizontal,
                    KnownDimensions = childKnown,
                    ParentSize = nodeInnerSize,
                    AvailableSpace = Size<AvailableSpace>.FromMainCross(dir,
                        childAvailMain, childAvailCross),
                };

                var measured = tree.ComputeChildLayout(item.NodeId, measureInput);
                float crossMeasured = measured.Size.Cross(dir);

                // Convert outer size to content size to avoid double-counting PaddingBorder
                float crossContent = childCrossKnown
                                     ?? MathF.Max(crossMeasured - item.PaddingBorderCrossAxisSum(dir), 0f);

                item.HypotheticalCrossSize = MathF.Max(MathF.Min(crossContent, item.MaxCross), item.MinCross);
                item.TargetCrossSize = item.HypotheticalCrossSize;
                item.FirstBaseline = measured.FirstBaselines.Cross(dir);
                item.MeasuredFirstBaselines = measured.FirstBaselines;

                // --- Line cross metrics (was Phase 7) ---
                float outerCross = item.HypotheticalCrossSize
                                   + item.MarginCrossAxisSum(dir)
                                   + item.PaddingBorderCrossAxisSum(dir);
                if (outerCross > maxCross) maxCross = outerCross;

                var itemAlign = ToAlignItems(item.AlignSelf) ?? defaultAlignItems;
                if (itemAlign == AlignItems.Baseline)
                {
                    float baseline = item.FirstBaseline ?? item.HypotheticalCrossSize;
                    float above = item.Margin.CrossStart(dir) + item.Border.CrossStart(dir)
                                                              + item.Padding.CrossStart(dir) + baseline;
                    float below = (item.HypotheticalCrossSize - baseline)
                                  + item.Padding.CrossEnd(dir) + item.Border.CrossEnd(dir)
                                  + item.Margin.CrossEnd(dir);

                    if (above > maxAbove) maxAbove = above;
                    if (below > maxBelow) maxBelow = below;
                    hasBaselineItem = true;
                }
            }

            line.CrossSize = maxCross;
            if (hasBaselineItem)
            {
                line.MaxAboveBaseline = maxAbove;
                float baselineCross = maxAbove + maxBelow;
                if (baselineCross > line.CrossSize)
                    line.CrossSize = baselineCross;
            }
        }

        if (lineCount == 1 && childContainerCrossInnerSize.HasValue)
        {
            lines[0].CrossSize = childContainerCrossInnerSize.Value;
        }

        // Phase 7b: Compute align-content stretch adjustment
        float lineCrossStretchAdj = 0f;
        if (childContainerCrossInnerSize.HasValue && lineCount > 1
                                                  && alignContent == AlignContent.Stretch)
        {
            float totalLineCross = 0f;
            for (int li = 0; li < lines.Length; li++)
                totalLineCross += lines[li].CrossSize;
            totalLineCross += crossGap * (lineCount - 1);

            float freeSpace = childContainerCrossInnerSize.Value - totalLineCross;
            if (freeSpace > 0f)
                lineCrossStretchAdj = freeSpace / lineCount;
        }

        // Phase 7b apply + 7c: Stretch items, compute totalCrossContent
        float totalCrossContent = 0f;
        for (int li = 0; li < lines.Length; li++)
        {
            ref var line = ref lines[li];
            line.CrossSize += lineCrossStretchAdj;
            totalCrossContent += line.CrossSize;

            var lineItems = flexItems.Slice(line.StartIndex, line.Count);
            for (int i = 0; i < lineItems.Length; i++)
            {
                ref var item = ref lineItems[i];
                var itemAlign = ToAlignItems(item.AlignSelf) ?? defaultAlignItems;

                if (itemAlign == AlignItems.Stretch && item.CrossSizeIsAuto
                                                    && !item.MarginCrossStartAuto && !item.MarginCrossEndAuto)
                {
                    float stretchedCross = line.CrossSize
                                           - item.MarginCrossAxisSum(dir)
                                           - item.PaddingBorderCrossAxisSum(dir);
                    stretchedCross = MathF.Max(stretchedCross, 0f);
                    stretchedCross = MathF.Max(MathF.Min(stretchedCross, item.MaxCross), item.MinCross);
                    item.TargetCrossSize = stretchedCross;
                }
            }
        }
        if (lineCount > 1)
            totalCrossContent += crossGap * (lineCount - 1);

        return totalCrossContent;
    }

    #endregion

    #region Phases 9 + 10: Position and align items

    private static void PositionAndAlignItems(
        Span<FlexLine> lines, Span<FlexItem> flexItems, FlexDirection dir,
        JustifyContent justifyContent, float containerMainSize, float mainGap,
        AlignContent alignContent, AlignItems defaultAlignItems,
        int lineCount, float containerCrossSize, float totalLineCross,
        float crossGap, Rect<float> paddingBorder, bool isWrapReverse)
    {
        // Compute line cross offsets first (all lines needed before per-item pass)
        float freeCross = MathF.Max(containerCrossSize - totalLineCross, 0f);

        var ac = alignContent;
        if (isWrapReverse)
        {
            ac = ac switch
            {
                AlignContent.FlexStart => AlignContent.FlexEnd,
                AlignContent.FlexEnd => AlignContent.FlexStart,
                _ => ac,
            };
        }

        ComputeAlignContent(ac, freeCross, lineCount, crossGap,
            out float crossInitialOffset, out float crossBetweenOffset);

        float crossCursor = paddingBorder.CrossStart(dir) + crossInitialOffset;
        for (int rawLi = 0; rawLi < lineCount; rawLi++)
        {
            int li = isWrapReverse ? (lineCount - 1 - rawLi) : rawLi;
            ref var line = ref lines[li];

            line.OffsetCross = crossCursor;
            crossCursor += line.CrossSize;
            if (rawLi < lineCount - 1)
                crossCursor += crossBetweenOffset;
        }

        // Position on main axis + align on cross axis in one pass per line
        bool isReverse = dir.IsReverse();
        var jcBase = isReverse ? FlipJustify(justifyContent) : justifyContent;

        for (int li = 0; li < lineCount; li++)
        {
            ref var line = ref lines[li];
            var lineItems = flexItems.Slice(line.StartIndex, line.Count);

            if (lineItems.Length == 0) continue;

            // Pre-pass: usedMain + auto margin count
            float usedMain = 0f;
            int autoMarginCount = 0;
            for (int i = 0; i < lineItems.Length; i++)
            {
                usedMain += lineItems[i].OuterTargetMainSize;
                if (lineItems[i].MarginMainStartAuto) autoMarginCount++;
                if (lineItems[i].MarginMainEndAuto) autoMarginCount++;
            }

            if (lineItems.Length > 1)
                usedMain += mainGap * (lineItems.Length - 1);

            float freeMain = containerMainSize - usedMain;

            if (autoMarginCount > 0 && freeMain > 0f)
            {
                float perAutoMargin = freeMain / autoMarginCount;
                for (int i = 0; i < lineItems.Length; i++)
                {
                    if (lineItems[i].MarginMainStartAuto)
                        SetMainMarginStart(ref lineItems[i], dir, perAutoMargin);
                    if (lineItems[i].MarginMainEndAuto)
                        SetMainMarginEnd(ref lineItems[i], dir, perAutoMargin);
                }

                freeMain = 0f;
            }
            else
            {
                freeMain = MathF.Max(freeMain, 0f);
            }

            var jc = jcBase;
            if (autoMarginCount > 0)
            {
                jc = isReverse ? JustifyContent.FlexEnd : JustifyContent.FlexStart;
                freeMain = 0f;
            }

            ComputeAlignment(jc, freeMain, lineItems.Length, mainGap,
                out float initialOffset, out float betweenOffset);

            // Main pass: set OffsetMain + OffsetCross per item
            float mainCursor = paddingBorder.MainStart(dir) + initialOffset;
            for (int i = 0; i < lineItems.Length; i++)
            {
                int idx = isReverse ? (lineItems.Length - 1 - i) : i;
                ref var item = ref lineItems[idx];

                // Main axis
                mainCursor += item.Margin.MainStart(dir);
                item.OffsetMain = mainCursor + item.Border.MainStart(dir) + item.Padding.MainStart(dir);
                mainCursor += item.PaddingBorderMainAxisSum(dir) + item.TargetMainSize
                                                                 + item.Margin.MainEnd(dir);
                if (i < lineItems.Length - 1)
                    mainCursor += betweenOffset;

                // Cross axis
                var itemAlign = ToAlignItems(item.AlignSelf) ?? defaultAlignItems;
                float itemOuterCross = item.TargetCrossSize
                                       + item.MarginCrossAxisSum(dir)
                                       + item.PaddingBorderCrossAxisSum(dir);
                float lineFreeForItem = MathF.Max(line.CrossSize - itemOuterCross, 0f);

                float crossOffset = 0f;
                if (item.MarginCrossStartAuto && item.MarginCrossEndAuto)
                {
                    float halfMargin = lineFreeForItem / 2f;
                    SetCrossMarginStart(ref item, dir, halfMargin);
                    SetCrossMarginEnd(ref item, dir, halfMargin);
                }
                else if (item.MarginCrossStartAuto)
                {
                    SetCrossMarginStart(ref item, dir, lineFreeForItem);
                }
                else if (item.MarginCrossEndAuto)
                {
                    SetCrossMarginEnd(ref item, dir, lineFreeForItem);
                }
                else
                {
                    crossOffset = itemAlign switch
                    {
                        AlignItems.Start or AlignItems.FlexStart => 0f,
                        AlignItems.End or AlignItems.FlexEnd => lineFreeForItem,
                        AlignItems.Center => lineFreeForItem / 2f,
                        AlignItems.Stretch => 0f,
                        AlignItems.Baseline => line.MaxAboveBaseline
                                               - item.Margin.CrossStart(dir) - item.Border.CrossStart(dir)
                                               - item.Padding.CrossStart(dir)
                                               - (item.FirstBaseline ?? item.TargetCrossSize),
                        _ => 0f,
                    };
                }

                item.OffsetCross = line.OffsetCross + crossOffset
                                                    + item.Margin.CrossStart(dir)
                                                    + item.Border.CrossStart(dir) + item.Padding.CrossStart(dir);
            }
        }
    }

    #endregion

    #region Phase 11: Perform final layout

    private static void PerformFinalLayout<TTree>(
        ref TTree tree, Span<FlexItem> flexItems, FlexDirection dir,
        Size<float?> nodeInnerSize,
        ref Point<float?> containerFirstBaselines,
        out float maxContentMain, out float maxContentCross)
        where TTree : ILayoutTree
    {
        maxContentMain = 0f;
        maxContentCross = 0f;

        for (int i = 0; i < flexItems.Length; i++)
        {
            ref var item = ref flexItems[i];

            float outerMainSize = item.TargetMainSize + item.PaddingBorderMainAxisSum(dir);
            float outerCrossSize = item.TargetCrossSize + item.PaddingBorderCrossAxisSum(dir);
            var outerSize = Size<float>.FromMainCross(dir, outerMainSize, outerCrossSize);

            float locMain = item.OffsetMain - item.Padding.MainStart(dir) - item.Border.MainStart(dir);
            float locCross = item.OffsetCross - item.Padding.CrossStart(dir) - item.Border.CrossStart(dir);

            var location = Point<float>.Zero
                .WithMain(dir, locMain)
                .WithCross(dir, locCross);

            var childKnown = Size<float?>.FromMainCross(dir,
                (float?)item.TargetMainSize, (float?)item.TargetCrossSize);

            var childInput = new LayoutInput
            {
                RunMode = RunMode.PerformLayout,
                SizingMode = SizingMode.InherentSize,
                Axis = RequestedAxis.Both,
                KnownDimensions = childKnown,
                ParentSize = nodeInnerSize,
                AvailableSpace = Size<AvailableSpace>.FromMainCross(dir,
                    AvailableSpace.Definite(item.TargetMainSize + item.PaddingBorderMainAxisSum(dir)),
                    AvailableSpace.Definite(item.TargetCrossSize + item.PaddingBorderCrossAxisSum(dir))),
            };

            var childOutput = tree.ComputeChildLayout(item.NodeId, childInput);

            // Propagate first child's baselines to container
            if (i == 0)
            {
                float? bx = childOutput.FirstBaselines.X.HasValue
                    ? location.X + item.Border.Left + item.Padding.Left + childOutput.FirstBaselines.X.Value
                    : null;
                float? by = childOutput.FirstBaselines.Y.HasValue
                    ? location.Y + item.Border.Top + item.Padding.Top + childOutput.FirstBaselines.Y.Value
                    : null;
                containerFirstBaselines = new Point<float?>(bx, by);
            }

            var nodeLayout = new NodeLayout
            {
                Order = (uint)item.OrderIndex,
                Location = location,
                Size = outerSize,
                ContentSize = childOutput.ContentSize,
                Border = item.Border,
                Padding = item.Padding,
                Margin = item.Margin,
                ScrollbarSize = childOutput.ScrollbarSize,
            };

            tree.SetUnroundedLayout(item.NodeId, in nodeLayout);

            float itemEndMain = locMain + outerMainSize + item.Margin.MainEnd(dir);
            float itemEndCross = locCross + outerCrossSize + item.Margin.CrossEnd(dir);
            if (itemEndMain > maxContentMain) maxContentMain = itemEndMain;
            if (itemEndCross > maxContentCross) maxContentCross = itemEndCross;
        }
    }

    #endregion

    #region Phase 12: Layout absolute children

    private static void LayoutAbsoluteChildren<TTree>(
        ref TTree tree, in Style style,
        int[] absoluteItemIds, int[] absoluteItemOrders, int absoluteCount,
        FlexDirection dir, bool isRow,
        Size<float?> nodeInnerSize, Size<float?> nodeOuterSize,
        float containerMainSize, float containerCrossSize,
        Rect<float> paddingBorder, Size<float> paddingBorderSum,
        AvailableSpace availableSpaceMain, AvailableSpace availableSpaceCross)
        where TTree : ILayoutTree
    {
        float? nodeInnerWidth = nodeInnerSize.Width;
        float? nodeInnerHeight = nodeInnerSize.Height;

        for (int ai = 0; ai < absoluteCount; ai++)
        {
            int childId = absoluteItemIds[ai];
            ref readonly var childStyle = ref tree.GetStyle(childId);

            var childPadding = LayoutHelpers.ResolvePadding(in childStyle, nodeInnerSize);
            var childBorder = LayoutHelpers.ResolveBorder(in childStyle, nodeInnerSize);
            var childMargin = LayoutHelpers.ResolveMargin(in childStyle, nodeInnerSize);
            var childInset = LayoutHelpers.ResolveInset(in childStyle, nodeOuterSize);
            var childContentBoxAdj = LayoutHelpers.ContentBoxAdjustment(childStyle.BoxSizing, childPadding, childBorder);

            float childPbW = childPadding.HorizontalAxisSum() + childBorder.HorizontalAxisSum();
            float childPbH = childPadding.VerticalAxisSum() + childBorder.VerticalAxisSum();

            var childSizeResolved = childStyle.Size.MaybeResolveNullable(nodeInnerSize);
            var childMinResolved = childStyle.MinSize.MaybeResolveNullable(nodeInnerSize);
            var childMaxResolved = childStyle.MaxSize.MaybeResolveNullable(nodeInnerSize);

            var adjChildSize = new Size<float?>(
                LayoutHelpers.MaybeAdd(childSizeResolved.Width, childContentBoxAdj.Width),
                LayoutHelpers.MaybeAdd(childSizeResolved.Height, childContentBoxAdj.Height));
            var adjChildMin = new Size<float?>(
                MaybeMaxZero(LayoutHelpers.MaybeAdd(childMinResolved.Width, childContentBoxAdj.Width)),
                MaybeMaxZero(LayoutHelpers.MaybeAdd(childMinResolved.Height, childContentBoxAdj.Height)));
            var adjChildMax = new Size<float?>(
                LayoutHelpers.MaybeAdd(childMaxResolved.Width, childContentBoxAdj.Width),
                LayoutHelpers.MaybeAdd(childMaxResolved.Height, childContentBoxAdj.Height));

            float? widthFromInset = null;
            float absContainerInnerW = nodeInnerWidth
                                       ?? (isRow ? containerMainSize : containerCrossSize);
            float absContainerInnerH = nodeInnerHeight
                                       ?? (isRow ? containerCrossSize : containerMainSize);

            if (childInset.Left.HasValue && childInset.Right.HasValue && !adjChildSize.Width.HasValue)
            {
                widthFromInset = absContainerInnerW
                                 - childInset.Left.Value - childInset.Right.Value
                                 - childMargin.Left - childMargin.Right
                                 - childPbW;
                widthFromInset = MathF.Max(widthFromInset.Value, 0f);
            }

            float? heightFromInset = null;
            if (childInset.Top.HasValue && childInset.Bottom.HasValue && !adjChildSize.Height.HasValue)
            {
                heightFromInset = absContainerInnerH
                                  - childInset.Top.Value - childInset.Bottom.Value
                                  - childMargin.Top - childMargin.Bottom
                                  - childPbH;
                heightFromInset = MathF.Max(heightFromInset.Value, 0f);
            }

            float? knownW = LayoutHelpers.MaybeClamp(
                adjChildSize.Width ?? widthFromInset, adjChildMin.Width, adjChildMax.Width);
            float? knownH = LayoutHelpers.MaybeClamp(
                adjChildSize.Height ?? heightFromInset, adjChildMin.Height, adjChildMax.Height);

            var availableW = isRow ? availableSpaceMain : availableSpaceCross;
            var availableH = isRow ? availableSpaceCross : availableSpaceMain;

            var absInput = new LayoutInput
            {
                RunMode = RunMode.PerformLayout,
                SizingMode = SizingMode.InherentSize,
                Axis = RequestedAxis.Both,
                KnownDimensions = new Size<float?>(knownW, knownH),
                ParentSize = nodeInnerSize,
                AvailableSpace = new Size<AvailableSpace>(
                    knownW.HasValue
                        ? AvailableSpace.Definite(knownW.Value + childPbW)
                        : availableW.IsDefinite()
                            ? AvailableSpace.Definite(availableW.Value)
                            : AvailableSpace.MaxContent,
                    knownH.HasValue
                        ? AvailableSpace.Definite(knownH.Value + childPbH)
                        : availableH.IsDefinite()
                            ? AvailableSpace.Definite(availableH.Value)
                            : AvailableSpace.MaxContent),
            };

            var absOutput = tree.ComputeChildLayout(childId, absInput);

            float absOuterW = knownW.HasValue ? knownW.Value + childPbW : absOutput.Size.Width;
            float absOuterH = knownH.HasValue ? knownH.Value + childPbH : absOutput.Size.Height;

            float absX, absY;
            float absContainerOuterW = absContainerInnerW + paddingBorderSum.Width;
            float absContainerOuterH = absContainerInnerH + paddingBorderSum.Height;

            if (childInset.Left.HasValue)
            {
                absX = paddingBorder.Left + childInset.Left.Value + childMargin.Left;
            }
            else if (childInset.Right.HasValue)
            {
                absX = absContainerOuterW - paddingBorder.Right - childInset.Right.Value
                       - childMargin.Right - absOuterW;
            }
            else
            {
                float freeX = MathF.Max(absContainerInnerW - absOuterW - childMargin.Left - childMargin.Right, 0f);
                float alignOffsetX;
                if (isRow)
                {
                    var jc = style.JustifyContent ?? JustifyContent.FlexStart;
                    if (dir.IsReverse()) jc = FlipJustify(jc);
                    alignOffsetX = AbsAlignOffset(jc, freeX);
                }
                else
                {
                    var xAlign = ToAlignItems(childStyle.AlignSelf) ?? style.AlignItems ?? AlignItems.Stretch;
                    alignOffsetX = AbsAlignOffset(xAlign, freeX);
                }

                absX = paddingBorder.Left + childMargin.Left + alignOffsetX;
            }

            if (childInset.Top.HasValue)
            {
                absY = paddingBorder.Top + childInset.Top.Value + childMargin.Top;
            }
            else if (childInset.Bottom.HasValue)
            {
                absY = absContainerOuterH - paddingBorder.Bottom - childInset.Bottom.Value
                       - childMargin.Bottom - absOuterH;
            }
            else
            {
                float freeY = MathF.Max(absContainerInnerH - absOuterH - childMargin.Top - childMargin.Bottom, 0f);
                float alignOffsetY;
                if (!isRow)
                {
                    var jc = style.JustifyContent ?? JustifyContent.FlexStart;
                    if (dir.IsReverse()) jc = FlipJustify(jc);
                    alignOffsetY = AbsAlignOffset(jc, freeY);
                }
                else
                {
                    var yAlign = ToAlignItems(childStyle.AlignSelf) ?? style.AlignItems ?? AlignItems.Stretch;
                    alignOffsetY = AbsAlignOffset(yAlign, freeY);
                }

                absY = paddingBorder.Top + childMargin.Top + alignOffsetY;
            }

            uint absOrder = (uint)absoluteItemOrders[ai];

            var absLayout = new NodeLayout
            {
                Order = absOrder,
                Location = new Point<float>(absX, absY),
                Size = new Size<float>(absOuterW, absOuterH),
                ContentSize = absOutput.ContentSize,
                Border = childBorder,
                Padding = childPadding,
                Margin = childMargin,
                ScrollbarSize = Size<float>.Zero,
            };
            tree.SetUnroundedLayout(childId, in absLayout);
        }
    }

    #endregion
}
