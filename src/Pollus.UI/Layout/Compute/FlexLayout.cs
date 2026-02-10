using System.Buffers;
using System.Runtime.CompilerServices;

namespace Pollus.UI.Layout;

public static class FlexLayout
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

        public float FlexBasisContent;
        public float InnerFlexBasis;
        public float HypotheticalMainSize;
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

        public bool MarginMainStartAuto;
        public bool MarginMainEndAuto;
        public bool MarginCrossStartAuto;
        public bool MarginCrossEndAuto;

        public float OffsetMain;
        public float OffsetCross;

        public bool CrossSizeIsAuto;

        public Style Style;

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
    }

    #endregion

    #region ComputeFlexbox

    public static LayoutOutput ComputeFlexbox<TTree>(ref TTree tree, int nodeId, LayoutInput input)
        where TTree : ILayoutTree
    {
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

        var availableSpaceMain = input.AvailableSpace.Main(dir)
            .MaybeSet(containerMainInnerSize);
        var availableSpaceCross = input.AvailableSpace.Cross(dir)
            .MaybeSet(containerCrossInnerSize);

        float mainGap = style.Gap.Main(dir).Resolve(containerMainInnerSize ?? 0f);
        float crossGap = style.Gap.Cross(dir).Resolve(containerCrossInnerSize ?? 0f);

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
                return new LayoutOutput
                {
                    Size = measuredSize,
                    ContentSize = measured.ContentSize,
                    FirstBaselines = measured.FirstBaselines,
                };
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
            return LayoutOutput.FromOuterSize(emptySize);
        }

        var flexItemsArray = ArrayPool<FlexItem>.Shared.Rent(childCount);
        var absoluteItemIds = ArrayPool<int>.Shared.Rent(childCount);
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
                    absoluteItemIds[absoluteCount++] = childId;
                    continue;
                }

                var childPadding = LayoutHelpers.ResolvePadding(in childStyle, nodeInnerSize);
                var childBorder = LayoutHelpers.ResolveBorder(in childStyle, nodeInnerSize);
                var childMargin = LayoutHelpers.ResolveMargin(in childStyle, nodeInnerSize);
                var childContentBoxAdj = LayoutHelpers.ContentBoxAdjustment(childStyle.BoxSizing, childPadding, childBorder);

                var childMinSize = childStyle.MinSize.MaybeResolveNullable(nodeInnerSize);
                var childMaxSize = childStyle.MaxSize.MaybeResolveNullable(nodeInnerSize);

                float childMinMain = MathF.Max(0f,
                    (LayoutHelpers.MaybeAdd(childMinSize.Main(dir), childContentBoxAdj.Main(dir))) ?? 0f);
                float childMaxMain = (LayoutHelpers.MaybeAdd(childMaxSize.Main(dir), childContentBoxAdj.Main(dir)))
                    ?? float.PositiveInfinity;
                float childMinCross = MathF.Max(0f,
                    (LayoutHelpers.MaybeAdd(childMinSize.Cross(dir), childContentBoxAdj.Cross(dir))) ?? 0f);
                float childMaxCross = (LayoutHelpers.MaybeAdd(childMaxSize.Cross(dir), childContentBoxAdj.Cross(dir)))
                    ?? float.PositiveInfinity;

                float pbMainSum = childPadding.MainAxisSum(dir) + childBorder.MainAxisSum(dir);
                float pbCrossSum = childPadding.CrossAxisSum(dir) + childBorder.CrossAxisSum(dir);

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
                    Style = childStyle,
                };
            }

            var flexItems = flexItemsArray.AsSpan(0, flexItemCount);

            StableSort(flexItems);

            #endregion
            #region Phase 3: Determine flex base size

            for (int i = 0; i < flexItems.Length; i++)
            {
                ref var item = ref flexItems[i];
                ref readonly var childStyle = ref item.Style;

                float pbMain = item.PaddingBorderMainAxisSum(dir);

                float? flexBasisResolved = childStyle.FlexBasis.IsAuto()
                    ? null
                    : LayoutHelpers.MaybeAdd(
                        childStyle.FlexBasis.Resolve(containerMainInnerSize ?? 0f),
                        item.ContentBoxMainAdj);

                float? styledMainSize = flexBasisResolved
                    ?? LayoutHelpers.MaybeAdd(
                        childStyle.Size.Main(dir).Resolve(containerMainInnerSize ?? 0f),
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
                            childStyle.Size.Cross(dir).Resolve(containerCrossInnerSize ?? 0f),
                            item.ContentBoxCrossAdj));

                    var measureInput = new LayoutInput
                    {
                        RunMode = RunMode.ComputeSize,
                        SizingMode = SizingMode.ContentSize,
                        Axis = isRow ? RequestedAxis.Horizontal : RequestedAxis.Vertical,
                        KnownDimensions = Size<float?>.Zero,
                        ParentSize = nodeInnerSize,
                        AvailableSpace = Size<AvailableSpace>.FromMainCross(dir,
                            availableSpaceMain,
                            childAvailCross),
                    };

                    var measured = tree.ComputeChildLayout(item.NodeId, measureInput);
                    flexBasis = measured.Size.Main(dir);
                }

                item.FlexBasisContent = flexBasis;
                item.InnerFlexBasis = MathF.Max(MathF.Min(flexBasis, item.MaxMain), item.MinMain);
                item.ScaledShrinkFactor = item.InnerFlexBasis * item.FlexShrink;
            }

            #endregion
            #region Phase 4: Collect items into flex lines

            var linesArray = ArrayPool<FlexLine>.Shared.Rent(Math.Max(flexItemCount, 1));
            int lineCount = 0;

            try
            {
                if (flexItemCount == 0)
                {
                    linesArray[lineCount++] = new FlexLine { StartIndex = 0, Count = 0 };
                }
                else if (!isWrap)
                {
                    linesArray[lineCount++] = new FlexLine { StartIndex = 0, Count = flexItemCount };
                }
                else
                {
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
                }

                var lines = linesArray.AsSpan(0, lineCount);

                #endregion
                #region Phase 5: Resolve flexible lengths

                for (int li = 0; li < lines.Length; li++)
                {
                    ref var line = ref lines[li];
                    var lineItems = flexItems.Slice(line.StartIndex, line.Count);
                    ResolveFlexibleLengths(lineItems, dir, mainGap, containerMainInnerSize, availableSpaceMain);
                }

                #endregion
                #region Phase 6: Determine hypothetical cross size

                for (int i = 0; i < flexItems.Length; i++)
                {
                    ref var item = ref flexItems[i];
                    ref readonly var childStyle = ref item.Style;

                    float? childCrossStyleSize = LayoutHelpers.MaybeAdd(
                        childStyle.Size.Cross(dir).Resolve(containerCrossInnerSize ?? 0f),
                        item.ContentBoxCrossAdj);

                    float? childCrossKnown = childCrossStyleSize;

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

                    item.HypotheticalCrossSize = MathF.Max(MathF.Min(crossMeasured, item.MaxCross), item.MinCross);
                    item.TargetCrossSize = item.HypotheticalCrossSize;

                    item.FirstBaseline = measured.FirstBaselines.Main(dir);
                }

                #endregion
                #region Phase 7: Calculate line cross sizes

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
                    for (int i = 0; i < lineItems.Length; i++)
                    {
                        float outerCross = lineItems[i].HypotheticalCrossSize
                            + lineItems[i].MarginCrossAxisSum(dir)
                            + lineItems[i].PaddingBorderCrossAxisSum(dir);
                        if (outerCross > maxCross) maxCross = outerCross;
                    }
                    line.CrossSize = maxCross;
                }

                if (lineCount == 1 && containerCrossInnerSize.HasValue)
                {
                    lines[0].CrossSize = MathF.Max(lines[0].CrossSize, containerCrossInnerSize.Value);
                }

                #endregion
                #region Phase 7b: Align-content stretch for multi-line

                var alignContent = style.AlignContent ?? (lineCount == 1 ? AlignContent.Stretch : AlignContent.FlexStart);

                if (containerCrossInnerSize.HasValue && lineCount > 1
                    && alignContent == AlignContent.Stretch)
                {
                    float totalLineCross = 0f;
                    for (int li = 0; li < lines.Length; li++)
                        totalLineCross += lines[li].CrossSize;
                    totalLineCross += crossGap * (lineCount - 1);

                    float freeSpace = containerCrossInnerSize.Value - totalLineCross;
                    if (freeSpace > 0f)
                    {
                        float perLine = freeSpace / lineCount;
                        for (int li = 0; li < lines.Length; li++)
                            lines[li].CrossSize += perLine;
                    }
                }

                #endregion
                #region Phase 7c: Stretch items on cross axis

                var defaultAlignItems = style.AlignItems ?? AlignItems.Stretch;

                for (int li = 0; li < lines.Length; li++)
                {
                    ref var line = ref lines[li];
                    var lineItems = flexItems.Slice(line.StartIndex, line.Count);

                    for (int i = 0; i < lineItems.Length; i++)
                    {
                        ref var item = ref lineItems[i];
                        var itemAlign = ToAlignItems(item.Style.AlignSelf) ?? defaultAlignItems;

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

                #endregion
                #region Phase 8: Determine container size
                float totalMainContent = 0f;
                for (int li = 0; li < lines.Length; li++)
                {
                    ref var line = ref lines[li];
                    var lineItems = flexItems.Slice(line.StartIndex, line.Count);

                    float lineMain = 0f;
                    for (int i = 0; i < lineItems.Length; i++)
                    {
                        lineMain += lineItems[i].TargetMainSize
                            + lineItems[i].MarginMainAxisSum(dir)
                            + lineItems[i].PaddingBorderMainAxisSum(dir);
                    }
                    if (lineItems.Length > 1)
                        lineMain += mainGap * (lineItems.Length - 1);

                    if (lineMain > totalMainContent)
                        totalMainContent = lineMain;
                }

                float totalCrossContent = 0f;
                for (int li = 0; li < lines.Length; li++)
                    totalCrossContent += lines[li].CrossSize;
                if (lineCount > 1)
                    totalCrossContent += crossGap * (lineCount - 1);

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

                if (input.RunMode == RunMode.ComputeSize)
                {
                    var sizeResult = Size<float>.FromMainCross(dir,
                        containerOuterMainSize, containerOuterCrossSize);
                    return LayoutOutput.FromOuterSize(sizeResult);
                }

                #endregion
                #region Phase 9: Position items on main axis (JustifyContent)

                var justifyContent = style.JustifyContent ?? JustifyContent.FlexStart;

                for (int li = 0; li < lines.Length; li++)
                {
                    ref var line = ref lines[li];
                    var lineItems = flexItems.Slice(line.StartIndex, line.Count);

                    if (lineItems.Length == 0) continue;

                    float usedMain = 0f;
                    int autoMarginCount = 0;
                    for (int i = 0; i < lineItems.Length; i++)
                    {
                        usedMain += lineItems[i].TargetMainSize
                            + lineItems[i].MarginMainAxisSum(dir)
                            + lineItems[i].PaddingBorderMainAxisSum(dir);
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
                            {
                                if (dir.IsRow())
                                {
                                    if (dir.IsReverse())
                                        lineItems[i].Margin.Right = perAutoMargin;
                                    else
                                        lineItems[i].Margin.Left = perAutoMargin;
                                }
                                else
                                {
                                    if (dir.IsReverse())
                                        lineItems[i].Margin.Bottom = perAutoMargin;
                                    else
                                        lineItems[i].Margin.Top = perAutoMargin;
                                }
                            }
                            if (lineItems[i].MarginMainEndAuto)
                            {
                                if (dir.IsRow())
                                {
                                    if (dir.IsReverse())
                                        lineItems[i].Margin.Left = perAutoMargin;
                                    else
                                        lineItems[i].Margin.Right = perAutoMargin;
                                }
                                else
                                {
                                    if (dir.IsReverse())
                                        lineItems[i].Margin.Top = perAutoMargin;
                                    else
                                        lineItems[i].Margin.Bottom = perAutoMargin;
                                }
                            }
                        }
                        freeMain = 0f;
                    }
                    else
                    {
                        freeMain = MathF.Max(freeMain, 0f);
                    }

                    int itemCount = lineItems.Length;
                    float initialOffset;
                    float betweenOffset;

                    bool isReverse = dir.IsReverse();
                    var jc = isReverse ? FlipJustify(justifyContent) : justifyContent;

                    if (autoMarginCount > 0)
                    {
                        jc = isReverse ? JustifyContent.FlexEnd : JustifyContent.FlexStart;
                        freeMain = 0f;
                    }

                    ComputeAlignment(jc, freeMain, itemCount, mainGap,
                        out initialOffset, out betweenOffset);

                    float mainCursor = paddingBorder.MainStart(dir) + initialOffset;
                    for (int i = 0; i < lineItems.Length; i++)
                    {
                        int idx = isReverse ? (lineItems.Length - 1 - i) : i;
                        ref var item = ref lineItems[idx];

                        mainCursor += item.Margin.MainStart(dir);
                        item.OffsetMain = mainCursor + item.Border.MainStart(dir) + item.Padding.MainStart(dir);

                        mainCursor += item.PaddingBorderMainAxisSum(dir) + item.TargetMainSize
                            + item.Margin.MainEnd(dir);

                        if (i < lineItems.Length - 1)
                            mainCursor += betweenOffset;
                    }
                }

                #endregion
                #region Phase 10: Align items on cross axis
                float totalLineCross2 = 0f;
                for (int li = 0; li < lines.Length; li++)
                    totalLineCross2 += lines[li].CrossSize;
                if (lineCount > 1)
                    totalLineCross2 += crossGap * (lineCount - 1);

                float freeCross = containerCrossSize - totalLineCross2;
                freeCross = MathF.Max(freeCross, 0f);

                float crossInitialOffset;
                float crossBetweenOffset;

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
                    out crossInitialOffset, out crossBetweenOffset);

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

                for (int li = 0; li < lineCount; li++)
                {
                    ref var line = ref lines[li];
                    var lineItems = flexItems.Slice(line.StartIndex, line.Count);

                    for (int i = 0; i < lineItems.Length; i++)
                    {
                        ref var item = ref lineItems[i];
                        var itemAlign = ToAlignItems(item.Style.AlignSelf) ?? defaultAlignItems;

                        float itemOuterCross = item.TargetCrossSize
                            + item.MarginCrossAxisSum(dir)
                            + item.PaddingBorderCrossAxisSum(dir);

                        float lineFreeForItem = line.CrossSize - itemOuterCross;
                        lineFreeForItem = MathF.Max(lineFreeForItem, 0f);

                        if (item.MarginCrossStartAuto && item.MarginCrossEndAuto)
                        {
                            float halfMargin = lineFreeForItem / 2f;
                            SetCrossMarginStart(ref item, dir, halfMargin);
                            SetCrossMarginEnd(ref item, dir, halfMargin);
                            item.OffsetCross = line.OffsetCross + item.Margin.CrossStart(dir)
                                + item.Border.CrossStart(dir) + item.Padding.CrossStart(dir);
                        }
                        else if (item.MarginCrossStartAuto)
                        {
                            SetCrossMarginStart(ref item, dir, lineFreeForItem);
                            item.OffsetCross = line.OffsetCross + item.Margin.CrossStart(dir)
                                + item.Border.CrossStart(dir) + item.Padding.CrossStart(dir);
                        }
                        else if (item.MarginCrossEndAuto)
                        {
                            SetCrossMarginEnd(ref item, dir, lineFreeForItem);
                            item.OffsetCross = line.OffsetCross + item.Margin.CrossStart(dir)
                                + item.Border.CrossStart(dir) + item.Padding.CrossStart(dir);
                        }
                        else
                        {
                            float crossOffset = itemAlign switch
                            {
                                AlignItems.Start or AlignItems.FlexStart => 0f,
                                AlignItems.End or AlignItems.FlexEnd => lineFreeForItem,
                                AlignItems.Center => lineFreeForItem / 2f,
                                AlignItems.Stretch => 0f,
                                AlignItems.Baseline => 0f, // Baseline handled as start for now
                                _ => 0f,
                            };

                            item.OffsetCross = line.OffsetCross + crossOffset
                                + item.Margin.CrossStart(dir)
                                + item.Border.CrossStart(dir) + item.Padding.CrossStart(dir);
                        }
                    }
                }

                #endregion
                #region Phase 11: Perform final layout
                float maxContentMain = 0f;
                float maxContentCross = 0f;

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

                    var nodeLayout = new NodeLayout
                    {
                        Order = (uint)item.OrderIndex,
                        Location = location,
                        Size = outerSize,
                        ContentSize = childOutput.ContentSize,
                        Border = item.Border,
                        Padding = item.Padding,
                        Margin = item.Margin,
                        ScrollbarSize = Size<float>.Zero,
                    };

                    tree.SetUnroundedLayout(item.NodeId, in nodeLayout);

                    float itemEndMain = locMain + outerMainSize + item.Margin.MainEnd(dir);
                    float itemEndCross = locCross + outerCrossSize + item.Margin.CrossEnd(dir);
                    if (itemEndMain > maxContentMain) maxContentMain = itemEndMain;
                    if (itemEndCross > maxContentCross) maxContentCross = itemEndCross;
                }

                #endregion
                #region Phase 12: Absolutely-positioned children

                for (int ai = 0; ai < absoluteCount; ai++)
                {
                    int childId = absoluteItemIds[ai];
                    ref readonly var childStyle = ref tree.GetStyle(childId);

                    var childPadding = LayoutHelpers.ResolvePadding(in childStyle, nodeInnerSize);
                    var childBorder = LayoutHelpers.ResolveBorder(in childStyle, nodeInnerSize);
                    var childMargin = LayoutHelpers.ResolveMargin(in childStyle, nodeInnerSize);
                    var childInset = LayoutHelpers.ResolveInset(in childStyle, nodeOuterSize);
                    var childContentBoxAdj2 = LayoutHelpers.ContentBoxAdjustment(childStyle.BoxSizing, childPadding, childBorder);

                    float childPbW = childPadding.HorizontalAxisSum() + childBorder.HorizontalAxisSum();
                    float childPbH = childPadding.VerticalAxisSum() + childBorder.VerticalAxisSum();

                    var childSizeResolved = childStyle.Size.MaybeResolveNullable(nodeInnerSize);
                    var childMinResolved = childStyle.MinSize.MaybeResolveNullable(nodeInnerSize);
                    var childMaxResolved = childStyle.MaxSize.MaybeResolveNullable(nodeInnerSize);

                    var adjChildSize = new Size<float?>(
                        LayoutHelpers.MaybeAdd(childSizeResolved.Width, childContentBoxAdj2.Width),
                        LayoutHelpers.MaybeAdd(childSizeResolved.Height, childContentBoxAdj2.Height));
                    var adjChildMin = new Size<float?>(
                        MaybeMaxZero(LayoutHelpers.MaybeAdd(childMinResolved.Width, childContentBoxAdj2.Width)),
                        MaybeMaxZero(LayoutHelpers.MaybeAdd(childMinResolved.Height, childContentBoxAdj2.Height)));
                    var adjChildMax = new Size<float?>(
                        LayoutHelpers.MaybeAdd(childMaxResolved.Width, childContentBoxAdj2.Width),
                        LayoutHelpers.MaybeAdd(childMaxResolved.Height, childContentBoxAdj2.Height));

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

                    uint absOrder = 0;
                    for (int ci = 0; ci < childIds.Length; ci++)
                    {
                        if (childIds[ci] == childId) { absOrder = (uint)ci; break; }
                    }

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

                #endregion
                #region Phase 13: Final container size

                var containerSize = Size<float>.FromMainCross(dir,
                    containerOuterMainSize, containerOuterCrossSize);

                var contentSize = Size<float>.FromMainCross(dir,
                    MathF.Max(maxContentMain, 0f),
                    MathF.Max(maxContentCross, 0f));

                return new LayoutOutput
                {
                    Size = containerSize,
                    ContentSize = contentSize,
                    FirstBaselines = Point<float?>.Zero,
                };

            }
            finally
            {
                ArrayPool<FlexLine>.Shared.Return(linesArray);
            }
        }
        finally
        {
            ArrayPool<FlexItem>.Shared.Return(flexItemsArray);
            ArrayPool<int>.Shared.Return(absoluteItemIds);
        }

        #endregion
    }

    #endregion

    #region Resolve Flexible Lengths

    private static void ResolveFlexibleLengths(
        Span<FlexItem> items, FlexDirection dir, float mainGap,
        float? containerMainSize, AvailableSpace availableSpaceMain)
    {
        if (items.Length == 0) return;

        float usedMain = 0f;
        for (int i = 0; i < items.Length; i++)
        {
            usedMain += items[i].InnerFlexBasis
                + items[i].MarginMainAxisSum(dir)
                + items[i].PaddingBorderMainAxisSum(dir);
        }
        if (items.Length > 1)
            usedMain += mainGap * (items.Length - 1);

        float availMain = containerMainSize ?? availableSpaceMain.UnwrapOr(0f);
        float initialFreeSpace = availMain - usedMain;
        bool isGrowing = initialFreeSpace > 0f;

        for (int i = 0; i < items.Length; i++)
        {
            items[i].Frozen = false;
            items[i].TargetMainSize = items[i].InnerFlexBasis;
        }

        for (int i = 0; i < items.Length; i++)
        {
            if (isGrowing && items[i].FlexGrow == 0f)
            {
                items[i].Frozen = true;
                items[i].TargetMainSize = items[i].InnerFlexBasis;
            }
            else if (!isGrowing && items[i].FlexShrink == 0f)
            {
                items[i].Frozen = true;
                items[i].TargetMainSize = items[i].InnerFlexBasis;
            }
        }

        const int maxIterations = 10;
        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            bool allFrozen = true;
            for (int i = 0; i < items.Length; i++)
            {
                if (!items[i].Frozen) { allFrozen = false; break; }
            }
            if (allFrozen) break;

            float remaining = availMain;
            float totalGrowFactor = 0f;
            float totalShrinkScaled = 0f;

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Frozen)
                {
                    remaining -= items[i].TargetMainSize
                        + items[i].MarginMainAxisSum(dir)
                        + items[i].PaddingBorderMainAxisSum(dir);
                }
                else
                {
                    remaining -= items[i].InnerFlexBasis
                        + items[i].MarginMainAxisSum(dir)
                        + items[i].PaddingBorderMainAxisSum(dir);
                    totalGrowFactor += items[i].FlexGrow;
                    totalShrinkScaled += items[i].ScaledShrinkFactor;
                }
            }
            if (items.Length > 1)
                remaining -= mainGap * (items.Length - 1);

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Frozen) continue;

                float ratio;
                if (isGrowing)
                {
                    if (totalGrowFactor == 0f || totalGrowFactor < 1f)
                    {
                        ratio = totalGrowFactor > 0f ? items[i].FlexGrow / totalGrowFactor : 0f;
                        float space = totalGrowFactor < 1f
                            ? initialFreeSpace * items[i].FlexGrow
                            : remaining * ratio;
                        items[i].TargetMainSize = items[i].InnerFlexBasis + MathF.Max(space, 0f);
                    }
                    else
                    {
                        ratio = items[i].FlexGrow / totalGrowFactor;
                        items[i].TargetMainSize = items[i].InnerFlexBasis + remaining * ratio;
                    }
                }
                else
                {
                    if (totalShrinkScaled > 0f)
                    {
                        ratio = items[i].ScaledShrinkFactor / totalShrinkScaled;
                        items[i].TargetMainSize = items[i].InnerFlexBasis + remaining * ratio;
                    }
                    else
                    {
                        items[i].TargetMainSize = items[i].InnerFlexBasis;
                    }
                }
            }

            bool anyViolation = false;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Frozen) continue;

                float clamped = MathF.Max(MathF.Min(items[i].TargetMainSize, items[i].MaxMain), items[i].MinMain);

                items[i].ViolationIsMin = clamped > items[i].TargetMainSize;
                items[i].ViolationIsMax = clamped < items[i].TargetMainSize;

                if (items[i].ViolationIsMin || items[i].ViolationIsMax)
                    anyViolation = true;

                items[i].TargetMainSize = clamped;
            }

            if (!anyViolation)
            {
                for (int i = 0; i < items.Length; i++)
                    items[i].Frozen = true;
            }
            else
            {
                float totalViolation = 0f;
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].Frozen) continue;
                    if (items[i].ViolationIsMin)
                        totalViolation += 1f;
                    else if (items[i].ViolationIsMax)
                        totalViolation -= 1f;
                }

                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].Frozen) continue;

                    if (totalViolation > 0f && items[i].ViolationIsMin)
                        items[i].Frozen = true;
                    else if (totalViolation < 0f && items[i].ViolationIsMax)
                        items[i].Frozen = true;
                    else if (totalViolation == 0f)
                        items[i].Frozen = true;
                }
            }
        }

        for (int i = 0; i < items.Length; i++)
        {
            items[i].TargetMainSize = MathF.Max(items[i].TargetMainSize, 0f);
            items[i].OuterTargetMainSize = items[i].TargetMainSize
                + items[i].MarginMainAxisSum(dir)
                + items[i].PaddingBorderMainAxisSum(dir);
        }
    }

    #endregion

    #region Alignment Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ComputeAlignment(
        JustifyContent jc, float freeSpace, int itemCount, float gap,
        out float initialOffset, out float betweenOffset)
    {
        betweenOffset = gap;

        switch (jc)
        {
            case JustifyContent.Start:
            case JustifyContent.FlexStart:
                initialOffset = 0f;
                break;
            case JustifyContent.End:
            case JustifyContent.FlexEnd:
                initialOffset = freeSpace;
                break;
            case JustifyContent.Center:
                initialOffset = freeSpace / 2f;
                break;
            case JustifyContent.SpaceBetween:
                initialOffset = 0f;
                betweenOffset = itemCount > 1
                    ? gap + freeSpace / (itemCount - 1)
                    : gap;
                break;
            case JustifyContent.SpaceAround:
                if (itemCount > 0)
                {
                    float perItem = freeSpace / itemCount;
                    initialOffset = perItem / 2f;
                    betweenOffset = gap + perItem;
                }
                else
                {
                    initialOffset = freeSpace / 2f;
                }
                break;
            case JustifyContent.SpaceEvenly:
                if (itemCount > 0)
                {
                    float perSlot = freeSpace / (itemCount + 1);
                    initialOffset = perSlot;
                    betweenOffset = gap + perSlot;
                }
                else
                {
                    initialOffset = freeSpace / 2f;
                }
                break;
            case JustifyContent.Stretch:
                initialOffset = 0f;
                break;
            default:
                initialOffset = 0f;
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ComputeAlignContent(
        AlignContent ac, float freeSpace, int lineCount, float gap,
        out float initialOffset, out float betweenOffset)
    {
        betweenOffset = gap;

        switch (ac)
        {
            case AlignContent.Start:
            case AlignContent.FlexStart:
                initialOffset = 0f;
                break;
            case AlignContent.End:
            case AlignContent.FlexEnd:
                initialOffset = freeSpace;
                break;
            case AlignContent.Center:
                initialOffset = freeSpace / 2f;
                break;
            case AlignContent.Stretch:
                initialOffset = 0f;
                break;
            case AlignContent.SpaceBetween:
                initialOffset = 0f;
                betweenOffset = lineCount > 1
                    ? gap + freeSpace / (lineCount - 1)
                    : gap;
                break;
            case AlignContent.SpaceAround:
                if (lineCount > 0)
                {
                    float perLine = freeSpace / lineCount;
                    initialOffset = perLine / 2f;
                    betweenOffset = gap + perLine;
                }
                else
                {
                    initialOffset = freeSpace / 2f;
                }
                break;
            case AlignContent.SpaceEvenly:
                if (lineCount > 0)
                {
                    float perSlot = freeSpace / (lineCount + 1);
                    initialOffset = perSlot;
                    betweenOffset = gap + perSlot;
                }
                else
                {
                    initialOffset = freeSpace / 2f;
                }
                break;
            default:
                initialOffset = 0f;
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static JustifyContent FlipJustify(JustifyContent jc) => jc switch
    {
        JustifyContent.FlexStart => JustifyContent.FlexEnd,
        JustifyContent.FlexEnd => JustifyContent.FlexStart,
        _ => jc,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static AlignItems? ToAlignItems(AlignSelf? alignSelf)
    {
        if (!alignSelf.HasValue) return null;
        return (AlignItems)(byte)alignSelf.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float AbsAlignOffset(JustifyContent jc, float freeSpace) => jc switch
    {
        JustifyContent.End or JustifyContent.FlexEnd => freeSpace,
        JustifyContent.Center or JustifyContent.SpaceAround or JustifyContent.SpaceEvenly => freeSpace / 2f,
        _ => 0f, // Start, FlexStart, Stretch, SpaceBetween
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float AbsAlignOffset(AlignItems ai, float freeSpace) => ai switch
    {
        AlignItems.End or AlignItems.FlexEnd => freeSpace,
        AlignItems.Center => freeSpace / 2f,
        _ => 0f, // Start, FlexStart, Stretch, Baseline
    };

    #endregion

    #region Utility Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Clamp(float value, float? min, float? max)
    {
        float v = value;
        if (min.HasValue && v < min.Value) v = min.Value;
        if (max.HasValue && v > max.Value) v = max.Value;
        return v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float? MaybeMaxZero(float? value)
    {
        if (!value.HasValue) return null;
        return MathF.Max(value.Value, 0f);
    }

    private static void SetCrossMarginStart(ref FlexItem item, FlexDirection dir, float value)
    {
        if (dir.IsRow())
            item.Margin.Top = value;
        else
            item.Margin.Left = value;
    }

    private static void SetCrossMarginEnd(ref FlexItem item, FlexDirection dir, float value)
    {
        if (dir.IsRow())
            item.Margin.Bottom = value;
        else
            item.Margin.Right = value;
    }

    #endregion

    #region Stable Sort

    private static void StableSort(Span<FlexItem> items)
    {
        for (int i = 1; i < items.Length; i++)
        {
            var key = items[i];
            int j = i - 1;
            while (j >= 0 && CompareOrder(items[j], key) > 0)
            {
                items[j + 1] = items[j];
                j--;
            }
            items[j + 1] = key;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CompareOrder(in FlexItem a, in FlexItem b)
    {
        int cmp = a.CssOrder.CompareTo(b.CssOrder);
        return cmp != 0 ? cmp : a.OrderIndex.CompareTo(b.OrderIndex);
    }

    #endregion
}
