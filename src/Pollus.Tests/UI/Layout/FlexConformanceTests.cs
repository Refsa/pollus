using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class FlexConformanceTests
{
    static LayoutStyle S => LayoutStyle.Default;

    static Size<Dimension> Px(float w, float h) =>
        new(Dimension.Px(w), Dimension.Px(h));

    static Rect<Length> PadAll(float v) =>
        new(Length.Px(v), Length.Px(v),
            Length.Px(v), Length.Px(v));

    static Rect<Length> PadLRTB(float l, float r, float t, float b) =>
        new(Length.Px(l), Length.Px(r),
            Length.Px(t), Length.Px(b));

    static Rect<Length> BorderAll(float v) => PadAll(v);

    static Rect<LengthAuto> MarginAll(float v) =>
        new(LengthAuto.Px(v), LengthAuto.Px(v),
            LengthAuto.Px(v), LengthAuto.Px(v));

    static Rect<LengthAuto> MarginLRTB(float l, float r, float t, float b) =>
        new(LengthAuto.Px(l), LengthAuto.Px(r),
            LengthAuto.Px(t), LengthAuto.Px(b));

    static TestLayoutTree Compute(LayoutStyle rootStyle, float w, float h,
        params (LayoutStyle style, (LayoutStyle style, LayoutStyle[]? grandchildren)[]? children)[] children)
    {
        var tree = new TestLayoutTree();
        int root = tree.AddNode(rootStyle);

        foreach (var (childStyle, grandchildren) in children)
        {
            int child = tree.AddNode(childStyle);
            tree.AddChild(root, child);
            if (grandchildren != null)
            {
                foreach (var (gcStyle, _) in grandchildren)
                {
                    int gc = tree.AddNode(gcStyle);
                    tree.AddChild(child, gc);
                }
            }
        }

        tree.ComputeRoot(root, w, h);
        var self = tree;
        RoundLayout.Round(ref self, root);
        return tree;
    }

    static TestLayoutTree ComputeFlat(LayoutStyle rootStyle, float w, float h,
        params LayoutStyle[] childStyles)
    {
        var tree = new TestLayoutTree();
        int root = tree.AddNode(rootStyle);
        foreach (var style in childStyles)
        {
            int child = tree.AddNode(style);
            tree.AddChild(root, child);
        }
        tree.ComputeRoot(root, w, h);
        var self = tree;
        RoundLayout.Round(ref self, root);
        return tree;
    }

    static void AssertLayout(TestLayoutTree tree, int nodeId,
        float x, float y, float w, float h)
    {
        var layout = tree.GetNodeLayout(nodeId);
        Assert.Equal(x, layout.Location.X);
        Assert.Equal(y, layout.Location.Y);
        Assert.Equal(w, layout.Size.Width);
        Assert.Equal(h, layout.Size.Height);
    }

    [Fact]
    public void Row_ChildrenFlowHorizontally()
    {
        var tree = ComputeFlat(S, 300, 100,
            S with { Size = Px(50, 50) },
            S with { Size = Px(50, 50) },
            S with { Size = Px(50, 50) });

        AssertLayout(tree, 1, 0, 0, 50, 50);
        AssertLayout(tree, 2, 50, 0, 50, 50);
        AssertLayout(tree, 3, 100, 0, 50, 50);
    }

    [Fact]
    public void Column_ChildrenFlowVertically()
    {
        var tree = ComputeFlat(S with { FlexDirection = FlexDirection.Column }, 100, 300,
            S with { Size = Px(50, 50) },
            S with { Size = Px(50, 50) },
            S with { Size = Px(50, 50) });

        AssertLayout(tree, 1, 0, 0, 50, 50);
        AssertLayout(tree, 2, 0, 50, 50, 50);
        AssertLayout(tree, 3, 0, 100, 50, 50);
    }

    [Fact]
    public void RowReverse_ChildrenFlowRightToLeft()
    {
        var tree = ComputeFlat(S with { FlexDirection = FlexDirection.RowReverse }, 300, 100,
            S with { Size = Px(50, 50) },
            S with { Size = Px(50, 50) });

        AssertLayout(tree, 1, 250, 0, 50, 50);
        AssertLayout(tree, 2, 200, 0, 50, 50);
    }

    [Fact]
    public void ColumnReverse_ChildrenFlowBottomToTop()
    {
        var tree = ComputeFlat(S with { FlexDirection = FlexDirection.ColumnReverse }, 100, 300,
            S with { Size = Px(50, 50) },
            S with { Size = Px(50, 50) });

        AssertLayout(tree, 1, 0, 250, 50, 50);
        AssertLayout(tree, 2, 0, 200, 50, 50);
    }

    [Fact]
    public void Grow_SingleChild_FillsMainAxis()
    {
        var tree = ComputeFlat(S, 200, 100,
            S with { FlexGrow = 1 });

        AssertLayout(tree, 1, 0, 0, 200, 100);
    }

    [Fact]
    public void Grow_EqualRatio_EvenDistribution()
    {
        var tree = ComputeFlat(S, 300, 100,
            S with { FlexGrow = 1 },
            S with { FlexGrow = 1 },
            S with { FlexGrow = 1 });

        AssertLayout(tree, 1, 0, 0, 100, 100);
        AssertLayout(tree, 2, 100, 0, 100, 100);
        AssertLayout(tree, 3, 200, 0, 100, 100);
    }

    [Fact]
    public void Grow_UnequalRatio_ProportionalDistribution()
    {
        var tree = ComputeFlat(S, 300, 100,
            S with { FlexGrow = 1 },
            S with { FlexGrow = 2 });

        AssertLayout(tree, 1, 0, 0, 100, 100);
        AssertLayout(tree, 2, 100, 0, 200, 100);
    }

    [Fact]
    public void Grow_WithFixedSibling_DistributesRemaining()
    {
        var tree = ComputeFlat(S, 300, 100,
            S with { Size = Px(100, 50) },
            S with { FlexGrow = 1 });

        AssertLayout(tree, 1, 0, 0, 100, 50);
        AssertLayout(tree, 2, 100, 0, 200, 100);
    }

    [Fact]
    public void Grow_ClampedByMaxWidth()
    {
        var tree = ComputeFlat(S, 300, 100,
            S with { FlexGrow = 1, MaxSize = new Size<Dimension>(Dimension.Px(100), Dimension.Auto) },
            S with { FlexGrow = 1 });

        // First child clamped to 100, remaining 200 goes to second
        AssertLayout(tree, 1, 0, 0, 100, 100);
        AssertLayout(tree, 2, 100, 0, 200, 100);
    }

    [Fact]
    public void Shrink_ProportionalToFlexBasis()
    {
        var tree = ComputeFlat(S, 200, 100,
            S with { Size = Px(150, 50), FlexShrink = 1 },
            S with { Size = Px(150, 50), FlexShrink = 1 });

        // Total basis = 300, available = 200, overflow = 100
        // Each shrinks by 50: 150-50=100, 150-50=100
        AssertLayout(tree, 1, 0, 0, 100, 50);
        AssertLayout(tree, 2, 100, 0, 100, 50);
    }

    [Fact]
    public void Shrink_ZeroShrink_DoesNotShrink()
    {
        var tree = ComputeFlat(S, 200, 100,
            S with { Size = Px(150, 50), FlexShrink = 0 },
            S with { Size = Px(150, 50), FlexShrink = 1 });

        // First child doesn't shrink (150), second absorbs all overflow
        AssertLayout(tree, 1, 0, 0, 150, 50);
        AssertLayout(tree, 2, 150, 0, 50, 50);
    }

    [Fact]
    public void Shrink_ClampedByMinWidth()
    {
        var tree = ComputeFlat(S, 200, 100,
            S with { Size = Px(150, 50), FlexShrink = 1, MinSize = new Size<Dimension>(Dimension.Px(120), Dimension.Auto) },
            S with { Size = Px(150, 50), FlexShrink = 1 });

        // First clamped at 120 (min), second gets remaining: 200-120=80
        AssertLayout(tree, 1, 0, 0, 120, 50);
        AssertLayout(tree, 2, 120, 0, 80, 50);
    }

    [Fact]
    public void FlexBasis_Px_OverridesSize()
    {
        var tree = ComputeFlat(S, 300, 100,
            S with { Size = Px(50, 50), FlexBasis = Dimension.Px(100) });

        AssertLayout(tree, 1, 0, 0, 100, 50);
    }

    [Fact]
    public void FlexBasis_Percent_ResolvesAgainstContainer()
    {
        var tree = ComputeFlat(S, 400, 100,
            S with { FlexBasis = Dimension.Percent(0.5f) });

        AssertLayout(tree, 1, 0, 0, 200, 100);
    }

    [Fact]
    public void FlexBasis_Zero_WithGrow_EqualDistribution()
    {
        var tree = ComputeFlat(S, 300, 100,
            S with { FlexBasis = Dimension.Px(0), FlexGrow = 1 },
            S with { FlexBasis = Dimension.Px(0), FlexGrow = 1 },
            S with { FlexBasis = Dimension.Px(0), FlexGrow = 1 });

        AssertLayout(tree, 1, 0, 0, 100, 100);
        AssertLayout(tree, 2, 100, 0, 100, 100);
        AssertLayout(tree, 3, 200, 0, 100, 100);
    }

    [Fact]
    public void JustifyContent_FlexEnd_ItemsAtEnd()
    {
        var tree = ComputeFlat(S with { JustifyContent = JustifyContent.FlexEnd }, 300, 100,
            S with { Size = Px(50, 50) },
            S with { Size = Px(50, 50) });

        AssertLayout(tree, 1, 200, 0, 50, 50);
        AssertLayout(tree, 2, 250, 0, 50, 50);
    }

    [Fact]
    public void JustifyContent_Center_ItemsCentered()
    {
        var tree = ComputeFlat(S with { JustifyContent = JustifyContent.Center }, 300, 100,
            S with { Size = Px(50, 50) },
            S with { Size = Px(50, 50) });

        AssertLayout(tree, 1, 100, 0, 50, 50);
        AssertLayout(tree, 2, 150, 0, 50, 50);
    }

    [Fact]
    public void JustifyContent_SpaceBetween_EvenSpacingBetween()
    {
        var tree = ComputeFlat(S with { JustifyContent = JustifyContent.SpaceBetween }, 300, 100,
            S with { Size = Px(50, 50) },
            S with { Size = Px(50, 50) },
            S with { Size = Px(50, 50) });

        // Free space = 300-150=150, 2 gaps, 75 each
        AssertLayout(tree, 1, 0, 0, 50, 50);
        AssertLayout(tree, 2, 125, 0, 50, 50);
        AssertLayout(tree, 3, 250, 0, 50, 50);
    }

    [Fact]
    public void JustifyContent_SpaceAround_EvenSpaceAroundItems()
    {
        var tree = ComputeFlat(S with { JustifyContent = JustifyContent.SpaceAround }, 300, 100,
            S with { Size = Px(50, 50) },
            S with { Size = Px(50, 50) });

        // Free = 200, 2 items → space=100, half=50
        // item1 at 50, item2 at 200
        AssertLayout(tree, 1, 50, 0, 50, 50);
        AssertLayout(tree, 2, 200, 0, 50, 50);
    }

    [Fact]
    public void JustifyContent_SpaceEvenly_EvenSpaceEverywhere()
    {
        var tree = ComputeFlat(S with { JustifyContent = JustifyContent.SpaceEvenly }, 300, 100,
            S with { Size = Px(50, 50) },
            S with { Size = Px(50, 50) });

        // Free = 200, 3 slots → ~66.67 each
        var l1 = tree.GetNodeLayout(1);
        var l2 = tree.GetNodeLayout(2);
        Assert.Equal(50f, l1.Size.Width);
        Assert.Equal(50f, l2.Size.Width);
        // Spacing should be equal: gap = gap before first = gap after last
        float gap = l1.Location.X;
        Assert.Equal(gap, l2.Location.X - l1.Location.X - 50, 1f);
    }

    [Fact]
    public void AlignItems_Center_CrossAxisCentered()
    {
        var tree = ComputeFlat(S with { AlignItems = AlignItems.Center }, 200, 100,
            S with { Size = Px(50, 30) });

        AssertLayout(tree, 1, 0, 35, 50, 30);
    }

    [Fact]
    public void AlignItems_FlexEnd_CrossAxisEnd()
    {
        var tree = ComputeFlat(S with { AlignItems = AlignItems.FlexEnd }, 200, 100,
            S with { Size = Px(50, 30) });

        AssertLayout(tree, 1, 0, 70, 50, 30);
    }

    [Fact]
    public void AlignItems_Stretch_FillsCrossAxis()
    {
        var tree = ComputeFlat(S with { AlignItems = AlignItems.Stretch }, 200, 100,
            S with { Size = new Size<Dimension>(Dimension.Px(50), Dimension.Auto) });

        AssertLayout(tree, 1, 0, 0, 50, 100);
    }

    [Fact]
    public void AlignSelf_OverridesAlignItems()
    {
        var tree = ComputeFlat(S with { AlignItems = AlignItems.FlexStart }, 200, 100,
            S with { Size = Px(50, 30), AlignSelf = AlignSelf.FlexEnd });

        AssertLayout(tree, 1, 0, 70, 50, 30);
    }

    [Fact]
    public void AlignSelf_Center_OneOfMany()
    {
        var tree = ComputeFlat(S with { AlignItems = AlignItems.FlexStart }, 200, 100,
            S with { Size = Px(50, 30) },
            S with { Size = Px(50, 30), AlignSelf = AlignSelf.Center });

        AssertLayout(tree, 1, 0, 0, 50, 30);
        AssertLayout(tree, 2, 50, 35, 50, 30);
    }

    [Fact]
    public void Wrap_OverflowCreatesNewLine()
    {
        var tree = ComputeFlat(S with { FlexWrap = FlexWrap.Wrap }, 100, 200,
            S with { Size = Px(60, 40) },
            S with { Size = Px(60, 40) });

        AssertLayout(tree, 1, 0, 0, 60, 40);
        AssertLayout(tree, 2, 0, 40, 60, 40);
    }

    [Fact]
    public void WrapReverse_LinesReversed()
    {
        var tree = ComputeFlat(
            S with { FlexWrap = FlexWrap.WrapReverse, AlignContent = AlignContent.FlexStart },
            100, 200,
            S with { Size = Px(60, 40) },
            S with { Size = Px(60, 40) });

        // Lines are reversed: second line at bottom, first line above
        var l1 = tree.GetNodeLayout(1);
        var l2 = tree.GetNodeLayout(2);
        Assert.True(l1.Location.Y > l2.Location.Y);
    }

    [Fact]
    public void NoWrap_AllOnOneLine()
    {
        var tree = ComputeFlat(S with { FlexWrap = FlexWrap.NoWrap }, 100, 100,
            S with { Size = Px(60, 40) },
            S with { Size = Px(60, 40) });

        // Both on same line, may overflow
        var l1 = tree.GetNodeLayout(1);
        var l2 = tree.GetNodeLayout(2);
        Assert.Equal(0f, l1.Location.Y);
        Assert.Equal(0f, l2.Location.Y);
    }

    [Fact]
    public void RowGap_SpaceBetweenItems()
    {
        var tree = ComputeFlat(S with { Gap = new Size<Length>(Length.Px(10), Length.Px(0)) }, 300, 100,
            S with { Size = Px(50, 50) },
            S with { Size = Px(50, 50) },
            S with { Size = Px(50, 50) });

        AssertLayout(tree, 1, 0, 0, 50, 50);
        AssertLayout(tree, 2, 60, 0, 50, 50);
        AssertLayout(tree, 3, 120, 0, 50, 50);
    }

    [Fact]
    public void ColumnGap_SpaceBetweenItems()
    {
        var tree = ComputeFlat(
            S with { FlexDirection = FlexDirection.Column, Gap = new Size<Length>(Length.Px(0), Length.Px(20)) },
            100, 300,
            S with { Size = Px(50, 50) },
            S with { Size = Px(50, 50) });

        AssertLayout(tree, 1, 0, 0, 50, 50);
        AssertLayout(tree, 2, 0, 70, 50, 50);
    }

    [Fact]
    public void Gap_WithWrap_AppliesBetweenLines()
    {
        var tree = ComputeFlat(
            S with
            {
                FlexWrap = FlexWrap.Wrap,
                AlignContent = AlignContent.FlexStart,
                Gap = new Size<Length>(Length.Px(10), Length.Px(20)),
            },
            100, 300,
            S with { Size = Px(60, 30) },
            S with { Size = Px(60, 30) });

        // Items wrap to separate lines
        AssertLayout(tree, 1, 0, 0, 60, 30);
        // Cross gap of 20 between lines
        AssertLayout(tree, 2, 0, 50, 60, 30);
    }

    [Fact]
    public void Padding_ShiftsChildrenInward()
    {
        var tree = ComputeFlat(S with { Padding = PadAll(10) }, 200, 100,
            S with { Size = Px(50, 30) });

        AssertLayout(tree, 1, 10, 10, 50, 30);
    }

    [Fact]
    public void Border_ShiftsChildrenInward()
    {
        var tree = ComputeFlat(S with { Border = BorderAll(5) }, 200, 100,
            S with { Size = Px(50, 30) });

        AssertLayout(tree, 1, 5, 5, 50, 30);
    }

    [Fact]
    public void PaddingAndBorder_CombinedOffset()
    {
        var tree = ComputeFlat(
            S with { Padding = PadAll(10), Border = BorderAll(5) }, 200, 100,
            S with { Size = Px(50, 30) });

        AssertLayout(tree, 1, 15, 15, 50, 30);
    }

    [Fact]
    public void Padding_ChildPositionedInContentBox()
    {
        var tree = ComputeFlat(S with { Padding = PadAll(20) }, 200, 100,
            S with { Size = Px(50, 30) });

        // Child positioned inside content box (after padding)
        AssertLayout(tree, 1, 20, 20, 50, 30);
    }

    [Fact]
    public void AsymmetricPadding_CorrectOffsets()
    {
        var tree = ComputeFlat(S with { Padding = PadLRTB(10, 20, 30, 40) }, 200, 200,
            S with { Size = Px(50, 30) });

        AssertLayout(tree, 1, 10, 30, 50, 30);
    }

    [Fact]
    public void Margin_OffsetsItemFromSiblings()
    {
        var tree = ComputeFlat(S, 300, 100,
            S with { Size = Px(50, 50) },
            S with { Size = Px(50, 50), Margin = MarginLRTB(10, 0, 0, 0) });

        AssertLayout(tree, 1, 0, 0, 50, 50);
        AssertLayout(tree, 2, 60, 0, 50, 50); // 50 + 10 margin
    }

    [Fact]
    public void Margin_TopBottom_InColumn()
    {
        var tree = ComputeFlat(S with { FlexDirection = FlexDirection.Column }, 100, 300,
            S with { Size = Px(50, 50) },
            S with { Size = Px(50, 50), Margin = MarginLRTB(0, 0, 20, 0) });

        AssertLayout(tree, 1, 0, 0, 50, 50);
        AssertLayout(tree, 2, 0, 70, 50, 50); // 50 + 20 margin
    }

    [Fact]
    public void PercentWidth_ResolvesAgainstParent()
    {
        var tree = ComputeFlat(S, 400, 200,
            S with { Size = new Size<Dimension>(Dimension.Percent(0.5f), Dimension.Px(50)) });

        AssertLayout(tree, 1, 0, 0, 200, 50);
    }

    [Fact]
    public void PercentHeight_ResolvesAgainstParent()
    {
        var tree = ComputeFlat(S with { FlexDirection = FlexDirection.Column }, 200, 400,
            S with { Size = new Size<Dimension>(Dimension.Px(50), Dimension.Percent(0.25f)) });

        AssertLayout(tree, 1, 0, 0, 50, 100);
    }

    [Fact]
    public void PercentPadding_AllSidesResolveAgainstWidth()
    {
        var tree = ComputeFlat(
            S with
            {
                Padding = new Rect<Length>(
                    Length.Percent(0.1f), Length.Percent(0.1f),
                    Length.Percent(0.1f), Length.Percent(0.1f)),
            },
            400, 200,
            S with { Size = Px(50, 30) });

        // CSS spec: ALL padding percentages resolve against parent width
        // 10% of 400 = 40 for all sides
        AssertLayout(tree, 1, 40, 40, 50, 30);
    }

    [Fact]
    public void MinWidth_PreventsUndersize()
    {
        var tree = ComputeFlat(S, 200, 100,
            S with { Size = Px(30, 50), MinSize = new Size<Dimension>(Dimension.Px(50), Dimension.Auto) });

        AssertLayout(tree, 1, 0, 0, 50, 50);
    }

    [Fact]
    public void MaxWidth_PreventsOversize()
    {
        var tree = ComputeFlat(S, 200, 100,
            S with { Size = Px(150, 50), MaxSize = new Size<Dimension>(Dimension.Px(100), Dimension.Auto) });

        AssertLayout(tree, 1, 0, 0, 100, 50);
    }

    [Fact]
    public void MinHeight_WithStretch_ClampsMinimum()
    {
        var tree = ComputeFlat(
            S with { AlignItems = AlignItems.Stretch },
            200, 50,
            S with
            {
                Size = new Size<Dimension>(Dimension.Px(50), Dimension.Auto),
                MinSize = new Size<Dimension>(Dimension.Auto, Dimension.Px(80)),
            });

        var layout = tree.GetNodeLayout(1);
        Assert.True(layout.Size.Height >= 80);
    }

    [Fact]
    public void DisplayNone_ExcludedFromLayout()
    {
        var tree = ComputeFlat(S, 300, 100,
            S with { Size = Px(50, 50) },
            S with { Display = Display.None, Size = Px(50, 50) },
            S with { Size = Px(50, 50) });

        AssertLayout(tree, 1, 0, 0, 50, 50);
        AssertLayout(tree, 2, 0, 0, 0, 0); // zeroed
        AssertLayout(tree, 3, 50, 0, 50, 50); // immediately follows child1
    }

    [Fact]
    public void DisplayNone_DoesNotAffectGrow()
    {
        var tree = ComputeFlat(S, 300, 100,
            S with { Display = Display.None, FlexGrow = 1 },
            S with { FlexGrow = 1 });

        AssertLayout(tree, 2, 0, 0, 300, 100); // takes all space
    }

    [Fact]
    public void Absolute_InsetLeftTop_PositionsFromContainerEdge()
    {
        var tree = ComputeFlat(S, 200, 200,
            S with
            {
                Position = Position.Absolute,
                Inset = new Rect<LengthAuto>(
                    LengthAuto.Px(10), LengthAuto.Auto,
                    LengthAuto.Px(20), LengthAuto.Auto),
                Size = Px(50, 50),
            });

        AssertLayout(tree, 1, 10, 20, 50, 50);
    }

    [Fact]
    public void Absolute_InsetRightBottom_PositionsFromOppositeEdge()
    {
        var tree = ComputeFlat(S, 200, 200,
            S with
            {
                Position = Position.Absolute,
                Inset = new Rect<LengthAuto>(
                    LengthAuto.Auto, LengthAuto.Px(10),
                    LengthAuto.Auto, LengthAuto.Px(20)),
                Size = Px(50, 50),
            });

        AssertLayout(tree, 1, 140, 130, 50, 50);
    }

    [Fact]
    public void Absolute_DoesNotAffectFlowSiblings()
    {
        var tree = ComputeFlat(S, 300, 100,
            S with
            {
                Position = Position.Absolute,
                Inset = new Rect<LengthAuto>(
                    LengthAuto.Px(0), LengthAuto.Auto,
                    LengthAuto.Px(0), LengthAuto.Auto),
                Size = Px(50, 50),
            },
            S with { Size = Px(80, 40) });

        // Flow child starts at 0, not after absolute child
        AssertLayout(tree, 2, 0, 0, 80, 40);
    }

    [Fact]
    public void Absolute_StretchFromInsets()
    {
        var tree = ComputeFlat(S, 200, 200,
            S with
            {
                Position = Position.Absolute,
                Inset = new Rect<LengthAuto>(
                    LengthAuto.Px(10), LengthAuto.Px(10),
                    LengthAuto.Px(20), LengthAuto.Px(20)),
            });

        AssertLayout(tree, 1, 10, 20, 180, 160);
    }

    [Fact]
    public void Nested_RowInsideColumn()
    {
        var tree = new TestLayoutTree();
        int root = tree.AddNode(S with { FlexDirection = FlexDirection.Column });
        int row = tree.AddNode(S with { Size = new Size<Dimension>(Dimension.Auto, Dimension.Px(50)) });
        int a = tree.AddNode(S with { Size = Px(30, 30) });
        int b = tree.AddNode(S with { Size = Px(30, 30) });
        tree.AddChild(root, row);
        tree.AddChild(row, a);
        tree.AddChild(row, b);
        tree.ComputeRoot(root, 200, 200);
        var self = tree;
        RoundLayout.Round(ref self, root);

        // Row container at top of column
        AssertLayout(tree, row, 0, 0, 200, 50);
        // Children flow horizontally inside row
        AssertLayout(tree, a, 0, 0, 30, 30);
        AssertLayout(tree, b, 30, 0, 30, 30);
    }

    [Fact]
    public void Nested_ColumnInsideRow()
    {
        var tree = new TestLayoutTree();
        int root = tree.AddNode(S);
        int col = tree.AddNode(S with { FlexDirection = FlexDirection.Column, Size = Px(100, 100) });
        int a = tree.AddNode(S with { Size = Px(80, 30) });
        int b = tree.AddNode(S with { Size = Px(80, 30) });
        tree.AddChild(root, col);
        tree.AddChild(col, a);
        tree.AddChild(col, b);
        tree.ComputeRoot(root, 400, 200);
        var self = tree;
        RoundLayout.Round(ref self, root);

        AssertLayout(tree, col, 0, 0, 100, 100);
        AssertLayout(tree, a, 0, 0, 80, 30);
        AssertLayout(tree, b, 0, 30, 80, 30);
    }

    [Fact]
    public void Nested_GrowInNestedContainer()
    {
        var tree = new TestLayoutTree();
        int root = tree.AddNode(S);
        int container = tree.AddNode(S with { Size = Px(200, 100) });
        int child = tree.AddNode(S with { FlexGrow = 1 });
        tree.AddChild(root, container);
        tree.AddChild(container, child);
        tree.ComputeRoot(root, 400, 200);
        var self = tree;
        RoundLayout.Round(ref self, root);

        AssertLayout(tree, child, 0, 0, 200, 100);
    }

    [Fact]
    public void ContentBox_PaddingAddsToSize()
    {
        var tree = ComputeFlat(S, 400, 200,
            S with
            {
                BoxSizing = BoxSizing.ContentBox,
                Size = Px(100, 50),
                Padding = PadAll(10),
            });

        // Content box: specified size is content, padding is extra
        var layout = tree.GetNodeLayout(1);
        Assert.Equal(120f, layout.Size.Width); // 100 + 10 + 10
        Assert.Equal(70f, layout.Size.Height); // 50 + 10 + 10
    }

    [Fact]
    public void BorderBox_PaddingIncludedInSize()
    {
        var tree = ComputeFlat(S, 400, 200,
            S with
            {
                BoxSizing = BoxSizing.BorderBox,
                Size = Px(100, 50),
                Padding = PadAll(10),
            });

        var layout = tree.GetNodeLayout(1);
        Assert.Equal(100f, layout.Size.Width);
        Assert.Equal(50f, layout.Size.Height);
    }

    [Fact]
    public void AlignContent_Center_LinesCentered()
    {
        var tree = ComputeFlat(
            S with
            {
                FlexWrap = FlexWrap.Wrap,
                AlignContent = AlignContent.Center,
            },
            100, 200,
            S with { Size = Px(60, 30) },
            S with { Size = Px(60, 30) });

        // Two lines of height 30 each = 60 total. Container = 200.
        // Centered: offset = (200-60)/2 = 70
        var l1 = tree.GetNodeLayout(1);
        var l2 = tree.GetNodeLayout(2);
        Assert.Equal(70f, l1.Location.Y);
        Assert.Equal(100f, l2.Location.Y);
    }

    [Fact]
    public void AlignContent_SpaceBetween_LinesSpread()
    {
        var tree = ComputeFlat(
            S with
            {
                FlexWrap = FlexWrap.Wrap,
                AlignContent = AlignContent.SpaceBetween,
            },
            100, 200,
            S with { Size = Px(60, 30) },
            S with { Size = Px(60, 30) });

        // First line at 0, last line at 200-30=170
        AssertLayout(tree, 1, 0, 0, 60, 30);
        AssertLayout(tree, 2, 0, 170, 60, 30);
    }

    [Fact]
    public void Order_ReordersItems()
    {
        var tree = ComputeFlat(S, 300, 100,
            S with { Size = Px(50, 50), Order = 2 },
            S with { Size = Px(50, 50), Order = 0 },
            S with { Size = Px(50, 50), Order = 1 });

        // Sorted by order: node2(0), node3(1), node1(2)
        var l1 = tree.GetNodeLayout(1);
        var l2 = tree.GetNodeLayout(2);
        var l3 = tree.GetNodeLayout(3);
        Assert.Equal(100f, l1.Location.X); // order 2 → third
        Assert.Equal(0f, l2.Location.X);   // order 0 → first
        Assert.Equal(50f, l3.Location.X);  // order 1 → second
    }

    [Fact]
    public void HolyGrail_Layout()
    {
        // Classic "holy grail" layout: header, footer, left sidebar, right sidebar, main content
        var tree = new TestLayoutTree();
        int root = tree.AddNode(S with { FlexDirection = FlexDirection.Column });
        int header = tree.AddNode(S with { Size = new Size<Dimension>(Dimension.Auto, Dimension.Px(50)) });
        int body = tree.AddNode(S with { FlexGrow = 1 });
        int footer = tree.AddNode(S with { Size = new Size<Dimension>(Dimension.Auto, Dimension.Px(50)) });

        int left = tree.AddNode(S with { Size = new Size<Dimension>(Dimension.Px(100), Dimension.Auto) });
        int main = tree.AddNode(S with { FlexGrow = 1 });
        int right = tree.AddNode(S with { Size = new Size<Dimension>(Dimension.Px(100), Dimension.Auto) });

        tree.AddChild(root, header);
        tree.AddChild(root, body);
        tree.AddChild(root, footer);
        tree.AddChild(body, left);
        tree.AddChild(body, main);
        tree.AddChild(body, right);

        tree.ComputeRoot(root, 800, 600);
        var self = tree;
        RoundLayout.Round(ref self, root);

        AssertLayout(tree, header, 0, 0, 800, 50);
        AssertLayout(tree, footer, 0, 550, 800, 50);
        AssertLayout(tree, body, 0, 50, 800, 500);
        AssertLayout(tree, left, 0, 0, 100, 500);
        AssertLayout(tree, main, 100, 0, 600, 500);
        AssertLayout(tree, right, 700, 0, 100, 500);
    }

    [Fact]
    public void NavigationBar_EqualSpacedItems()
    {
        var tree = ComputeFlat(
            S with { JustifyContent = JustifyContent.SpaceBetween, AlignItems = AlignItems.Center },
            600, 60,
            S with { Size = Px(100, 40) }, // Logo
            S with { Size = Px(80, 30) },  // Nav 1
            S with { Size = Px(80, 30) },  // Nav 2
            S with { Size = Px(80, 30) }); // Nav 3

        var logo = tree.GetNodeLayout(1);
        var nav3 = tree.GetNodeLayout(4);

        Assert.Equal(0f, logo.Location.X);           // Logo at start
        Assert.Equal(520f, nav3.Location.X);          // Last nav at end (600-80=520)
        Assert.Equal(10f, logo.Location.Y);           // Centered: (60-40)/2=10
        Assert.Equal(15f, nav3.Location.Y);           // Centered: (60-30)/2=15
    }

    [Fact]
    public void ResponsiveGrid_WrappingItems()
    {
        // 3 items of 150px each in a 400px container with wrap + gap
        var tree = ComputeFlat(
            S with
            {
                FlexWrap = FlexWrap.Wrap,
                AlignContent = AlignContent.FlexStart,
                Gap = new Size<Length>(Length.Px(10), Length.Px(10)),
            },
            400, 400,
            S with { Size = Px(150, 100) },
            S with { Size = Px(150, 100) },
            S with { Size = Px(150, 100) });

        // First line: items 1,2 (150+10+150=310 < 400)
        // Second line: item 3
        AssertLayout(tree, 1, 0, 0, 150, 100);
        AssertLayout(tree, 2, 160, 0, 150, 100);
        AssertLayout(tree, 3, 0, 110, 150, 100);
    }
}
