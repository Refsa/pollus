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
}
