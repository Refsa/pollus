namespace Pollus.UI.Layout;

using System.Runtime.CompilerServices;

public static partial class FlexLayout
{
    #region Resolve Flexible Lengths

    private static float ResolveFlexibleLengths(
        Span<FlexItem> items, FlexDirection dir, float mainGap,
        float? containerMainSize)
    {
        if (items.Length == 0) return 0f;

        float usedMain = 0f;
        for (int i = 0; i < items.Length; i++)
        {
            usedMain += items[i].InnerFlexBasis
                + items[i].MarginMainAxisSum(dir)
                + items[i].PaddingBorderMainAxisSum(dir);
        }
        if (items.Length > 1)
            usedMain += mainGap * (items.Length - 1);

        // When the container has no definite main size (auto height/width), items should
        // not be shrunk — the container will grow to fit its content. Only constrain items
        // when the container has a definite main size (explicit or from known dimensions).
        float availMain = containerMainSize ?? usedMain;
        float initialFreeSpace = availMain - usedMain;
        bool isGrowing = initialFreeSpace > 0f;

        for (int i = 0; i < items.Length; i++)
        {
            items[i].TargetMainSize = items[i].InnerFlexBasis;
            items[i].Frozen = (isGrowing && items[i].FlexGrow == 0f)
                || (!isGrowing && items[i].FlexShrink == 0f);
        }

        const int maxIterations = 10;
        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            // Pass A: Check allFrozen + compute remaining space and flex factors
            bool allFrozen = true;
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
                    allFrozen = false;
                    remaining -= items[i].InnerFlexBasis
                        + items[i].MarginMainAxisSum(dir)
                        + items[i].PaddingBorderMainAxisSum(dir);
                    totalGrowFactor += items[i].FlexGrow;
                    totalShrinkScaled += items[i].ScaledShrinkFactor;
                }
            }
            if (allFrozen) break;
            if (items.Length > 1)
                remaining -= mainGap * (items.Length - 1);

            // Pass B: Compute targets, check violations, accumulate totalViolation
            bool anyViolation = false;
            float totalViolation = 0f;

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Frozen) continue;

                if (isGrowing)
                {
                    if (totalGrowFactor == 0f || totalGrowFactor < 1f)
                    {
                        float ratio = totalGrowFactor > 0f ? items[i].FlexGrow / totalGrowFactor : 0f;
                        float space = totalGrowFactor < 1f
                            ? initialFreeSpace * items[i].FlexGrow
                            : remaining * ratio;
                        items[i].TargetMainSize = items[i].InnerFlexBasis + MathF.Max(space, 0f);
                    }
                    else
                    {
                        float ratio = items[i].FlexGrow / totalGrowFactor;
                        items[i].TargetMainSize = items[i].InnerFlexBasis + remaining * ratio;
                    }
                }
                else
                {
                    if (totalShrinkScaled > 0f)
                    {
                        float ratio = items[i].ScaledShrinkFactor / totalShrinkScaled;
                        items[i].TargetMainSize = items[i].InnerFlexBasis + remaining * ratio;
                    }
                    else
                    {
                        items[i].TargetMainSize = items[i].InnerFlexBasis;
                    }
                }

                float clamped = MathF.Max(MathF.Min(items[i].TargetMainSize, items[i].MaxMain), items[i].MinMain);
                if (clamped > items[i].TargetMainSize)
                {
                    anyViolation = true;
                    totalViolation += 1f;
                    items[i].ViolationIsMin = true;
                    items[i].ViolationIsMax = false;
                }
                else if (clamped < items[i].TargetMainSize)
                {
                    anyViolation = true;
                    totalViolation -= 1f;
                    items[i].ViolationIsMin = false;
                    items[i].ViolationIsMax = true;
                }
                else
                {
                    items[i].ViolationIsMin = false;
                    items[i].ViolationIsMax = false;
                }
                items[i].TargetMainSize = clamped;
            }

            // Pass C: Freeze items based on violations
            if (!anyViolation)
            {
                for (int i = 0; i < items.Length; i++)
                    items[i].Frozen = true;
            }
            else
            {
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

        float lineMainSize = 0f;
        for (int i = 0; i < items.Length; i++)
        {
            items[i].TargetMainSize = MathF.Max(items[i].TargetMainSize, 0f);
            items[i].OuterTargetMainSize = items[i].TargetMainSize
                + items[i].MarginMainAxisSum(dir)
                + items[i].PaddingBorderMainAxisSum(dir);
            lineMainSize += items[i].OuterTargetMainSize;
        }
        if (items.Length > 1)
            lineMainSize += mainGap * (items.Length - 1);
        return lineMainSize;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetMainMarginStart(ref FlexItem item, FlexDirection dir, float value)
    {
        if (dir.IsRow())
        {
            if (dir.IsReverse()) item.Margin.Right = value;
            else item.Margin.Left = value;
        }
        else
        {
            if (dir.IsReverse()) item.Margin.Bottom = value;
            else item.Margin.Top = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetMainMarginEnd(ref FlexItem item, FlexDirection dir, float value)
    {
        if (dir.IsRow())
        {
            if (dir.IsReverse()) item.Margin.Left = value;
            else item.Margin.Right = value;
        }
        else
        {
            if (dir.IsReverse()) item.Margin.Top = value;
            else item.Margin.Bottom = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetCrossMarginStart(ref FlexItem item, FlexDirection dir, float value)
    {
        if (dir.IsRow())
            item.Margin.Top = value;
        else
            item.Margin.Left = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        if (items.Length <= 1) return;

        // Early exit: skip sort when all CssOrder == 0 (common case)
        bool needsSort = false;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].CssOrder != 0) { needsSort = true; break; }
        }
        if (!needsSort) return;

        // Sort an index array (4 bytes per swap) instead of FlexItem structs
        Span<int> indices = items.Length <= 64
            ? stackalloc int[items.Length]
            : new int[items.Length];

        for (int i = 0; i < indices.Length; i++) indices[i] = i;

        // Insertion sort on indices
        for (int i = 1; i < indices.Length; i++)
        {
            int key = indices[i];
            int j = i - 1;
            while (j >= 0 && CompareOrder(in items[indices[j]], in items[key]) > 0)
            {
                indices[j + 1] = indices[j];
                j--;
            }
            indices[j + 1] = key;
        }

        // Apply permutation in-place using cycle sort
        for (int i = 0; i < indices.Length; i++)
        {
            if (indices[i] == i) continue;
            var temp = items[i];
            int j = i;
            while (indices[j] != i)
            {
                int next = indices[j];
                items[j] = items[next];
                indices[j] = j;
                j = next;
            }
            items[j] = temp;
            indices[j] = j;
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
