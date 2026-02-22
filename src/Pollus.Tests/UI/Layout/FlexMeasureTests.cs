using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class FlexMeasureTests
{
    [Fact]
    public void LeafWithMeasureFunc_UsesIntrinsicSize()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(300), Length.Px(200)),
        });
        var leaf = tree.AddNode(LayoutStyle.Default);
        tree.AddChild(root, leaf);

        tree.SetMeasureFunc(leaf, _ => new LayoutOutput
        {
            Size = new Size<float>(100, 20),
        });

        tree.ComputeRoot(root, 300, 200);
        var layout = tree.GetNodeLayout(leaf);

        Assert.Equal(100, layout.Size.Width);
    }

    [Fact]
    public void MeasureFunc_ReceivesAvailableSpace()
    {
        Size<AvailableSpace> receivedAvailable = default;

        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(400), Length.Px(300)),
            FlexDirection = FlexDirection.Row,
        });
        var leaf = tree.AddNode(LayoutStyle.Default);
        tree.AddChild(root, leaf);

        tree.SetMeasureFunc(leaf, input =>
        {
            receivedAvailable = input.AvailableSpace;
            return new LayoutOutput
            {
                Size = new Size<float>(80, 25),
            };
        });

        tree.ComputeRoot(root, 400, 300);

        Assert.True(receivedAvailable.Width.IsDefinite());
    }

    [Fact]
    public void MeasureFunc_FlexBasisAuto_UsesIntrinsicSize()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(400), Length.Px(200)),
            FlexDirection = FlexDirection.Row,
        });

        // Two children: one with measure func, one with fixed size
        var measured = tree.AddNode(LayoutStyle.Default with
        {
            FlexBasis = Length.Auto,
        });
        var fixedChild = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(100), Length.Auto),
        });
        tree.AddChild(root, measured);
        tree.AddChild(root, fixedChild);

        tree.SetMeasureFunc(measured, _ => new LayoutOutput
        {
            Size = new Size<float>(150, 30),
        });

        tree.ComputeRoot(root, 400, 200);
        var measuredLayout = tree.GetNodeLayout(measured);
        var fixedLayout = tree.GetNodeLayout(fixedChild);

        Assert.Equal(150, measuredLayout.Size.Width);
        Assert.Equal(100, fixedLayout.Size.Width);
    }

    [Fact]
    public void MeasureFunc_WithFlexGrow_ExpandsBeyondIntrinsic()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(400), Length.Px(200)),
            FlexDirection = FlexDirection.Row,
        });

        var measured = tree.AddNode(LayoutStyle.Default with
        {
            FlexGrow = 1,
        });
        var fixedChild = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(100), Length.Auto),
        });
        tree.AddChild(root, measured);
        tree.AddChild(root, fixedChild);

        tree.SetMeasureFunc(measured, _ => new LayoutOutput
        {
            Size = new Size<float>(50, 30),
        });

        tree.ComputeRoot(root, 400, 200);
        var measuredLayout = tree.GetNodeLayout(measured);

        // Intrinsic 50 + fixed 100 = 150, remaining 250 → measured gets 300
        Assert.Equal(300, measuredLayout.Size.Width);
    }

    [Fact]
    public void MeasureFunc_ClampedByMinMax()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(300), Length.Px(200)),
        });
        var leaf = tree.AddNode(LayoutStyle.Default with
        {
            MinSize = new Size<Length>(Length.Px(120), Length.Auto),
            MaxSize = new Size<Length>(Length.Px(200), Length.Px(50)),
        });
        tree.AddChild(root, leaf);

        tree.SetMeasureFunc(leaf, _ => new LayoutOutput
        {
            Size = new Size<float>(80, 100), // below min width, above max height
        });

        tree.ComputeRoot(root, 300, 200);
        var layout = tree.GetNodeLayout(leaf);

        Assert.Equal(120, layout.Size.Width);
        Assert.Equal(50, layout.Size.Height);
    }

    [Fact]
    public void MeasureFunc_WithKnownDimensions_PassedThrough()
    {
        Size<float?> receivedKnown = default;

        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(300), Length.Px(200)),
            FlexDirection = FlexDirection.Row,
        });
        var leaf = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(80), Length.Auto),
        });
        tree.AddChild(root, leaf);

        tree.SetMeasureFunc(leaf, input =>
        {
            receivedKnown = input.KnownDimensions;
            return new LayoutOutput
            {
                Size = new Size<float>(
                    input.KnownDimensions.Width ?? 80,
                    input.KnownDimensions.Height ?? 30),
            };
        });

        tree.ComputeRoot(root, 300, 200);

        // During the PerformLayout pass, known dimensions should include the
        // resolved width (80) from style
        Assert.NotNull(receivedKnown.Width);
        Assert.Equal(80, receivedKnown.Width!.Value);
    }

    [Fact]
    public void MeasureFunc_TextLikeWrapping()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(200), Length.Px(400)),
            FlexDirection = FlexDirection.Column,
        });
        var text = tree.AddNode(LayoutStyle.Default);
        tree.AddChild(root, text);

        // Simulate text: 500px of content at 20px height per line
        const float totalContentWidth = 500f;
        const float lineHeight = 20f;

        tree.SetMeasureFunc(text, input =>
        {
            float availWidth = input.KnownDimensions.Width
                ?? input.AvailableSpace.Width.UnwrapOr(float.MaxValue);
            float lines = MathF.Ceiling(totalContentWidth / availWidth);
            return new LayoutOutput
            {
                Size = new Size<float>(MathF.Min(totalContentWidth, availWidth), lines * lineHeight),
            };
        });

        tree.ComputeRoot(root, 200, 400);
        var layout = tree.GetNodeLayout(text);

        // At 200px width, 500px content = 3 lines = 60px height
        Assert.Equal(200, layout.Size.Width);
        Assert.Equal(60, layout.Size.Height);
    }

    [Fact]
    public void MeasureFunc_WithPadding_OuterSizeIncludesPadding()
    {
        // Default BoxSizing is BorderBox, default direction is Row.
        // Use AlignItems.FlexStart to prevent cross-axis stretching so we
        // can verify the intrinsic height is content + padding.
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(400), Length.Px(300)),
            AlignItems = AlignItems.FlexStart,
        });
        var leaf = tree.AddNode(LayoutStyle.Default with
        {
            Padding = Rect<Length>.All(10),
        });
        tree.AddChild(root, leaf);

        tree.SetMeasureFunc(leaf, _ => new LayoutOutput
        {
            Size = new Size<float>(100, 20),
        });

        tree.ComputeRoot(root, 400, 300);
        var layout = tree.GetNodeLayout(leaf);

        // Content (100x20) + padding (10 each side) = outer 120x40
        Assert.Equal(120, layout.Size.Width);
        Assert.Equal(40, layout.Size.Height);
        Assert.Equal(10, layout.Padding.Left);
        Assert.Equal(10, layout.Padding.Top);
    }

    [Fact]
    public void MeasureFunc_WithPadding_ContentAreaNotShrunk()
    {
        // Regression: padding was double-subtracted, making the content area too small.
        // Column layout, leaf with width=100% height=Auto padding=8.
        // Default BoxSizing is BorderBox so 100% width includes padding.
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(300), Length.Px(200)),
            FlexDirection = FlexDirection.Column,
        });
        var leaf = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Percent(1f), Length.Auto),
            Padding = Rect<Length>.All(8),
        });
        tree.AddChild(root, leaf);

        // Simulate text: measured at available width, returns content size
        tree.SetMeasureFunc(leaf, input =>
        {
            float w = input.KnownDimensions.Width
                      ?? input.AvailableSpace.Width.UnwrapOr(float.MaxValue);
            return new LayoutOutput
            {
                Size = new Size<float>(w, 40),
            };
        });

        tree.ComputeRoot(root, 300, 200);
        var layout = tree.GetNodeLayout(leaf);

        // BorderBox: 100% of 300 = 300 outer, content = 300 - 16 = 284
        Assert.Equal(300, layout.Size.Width);
        // Height: 40 content + 16 padding = 56 outer
        Assert.Equal(56, layout.Size.Height);

        // Content area = outer - padding = the original measured size
        float contentW = layout.Size.Width - layout.Padding.Left - layout.Padding.Right;
        float contentH = layout.Size.Height - layout.Padding.Top - layout.Padding.Bottom;
        Assert.Equal(284, contentW);
        Assert.Equal(40, contentH);
    }

    [Fact]
    public void MeasureFunc_WithPadding_FlexGrowRespectsPadding()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(400), Length.Px(200)),
            FlexDirection = FlexDirection.Row,
        });

        var measured = tree.AddNode(LayoutStyle.Default with
        {
            FlexGrow = 1,
            Padding = Rect<Length>.All(10),
        });
        var fixedChild = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(100), Length.Auto),
        });
        tree.AddChild(root, measured);
        tree.AddChild(root, fixedChild);

        tree.SetMeasureFunc(measured, _ => new LayoutOutput
        {
            Size = new Size<float>(50, 30),
        });

        tree.ComputeRoot(root, 400, 200);
        var measuredLayout = tree.GetNodeLayout(measured);

        // Intrinsic outer = 50 + 20 (padding) = 70. Fixed = 100.
        // Total = 170, free = 230. Measured grows by 230 -> outer = 300.
        // Content = 300 - 20 = 280.
        Assert.Equal(300, measuredLayout.Size.Width);
        float contentW = measuredLayout.Size.Width - measuredLayout.Padding.Left - measuredLayout.Padding.Right;
        Assert.Equal(280, contentW);
    }

    [Fact]
    public void MeasureFunc_WithMargin_OffsetsPosition()
    {
        // Verify margin on a measured leaf creates a gap from the parent edge.
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(300), Length.Px(200)),
            FlexDirection = FlexDirection.Column,
            AlignItems = AlignItems.FlexStart,
        });
        var leaf = tree.AddNode(LayoutStyle.Default with
        {
            Margin = new Rect<Length>(Length.Px(0), Length.Px(0), Length.Px(5), Length.Px(5)),
        });
        tree.AddChild(root, leaf);

        tree.SetMeasureFunc(leaf, _ => new LayoutOutput
        {
            Size = new Size<float>(80, 30),
        });

        tree.ComputeRoot(root, 300, 200);
        var layout = tree.GetNodeLayout(leaf);

        // Column: main=Y, cross=X. Margin top=5, bottom=5.
        // Position should be offset by margin from parent's content box.
        Assert.Equal(5, layout.Margin.Top);
        Assert.Equal(5, layout.Margin.Bottom);
        Assert.Equal(5, layout.Location.Y);
        Assert.Equal(0, layout.Location.X);

        // Outer size should NOT include margin (margin is external).
        Assert.Equal(80, layout.Size.Width);
        Assert.Equal(30, layout.Size.Height);
    }
}
