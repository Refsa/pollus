using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class FlexWrapTests
{
    static LayoutStyle DefaultStyle => LayoutStyle.Default;

    [Fact]
    public void Wrap_ThreeChildren_TwoLines()
    {
        // 3 children 80px each in a 200px container with FlexWrap=Wrap.
        // First line fits 2 items (80+80=160 <= 200), second line gets the third.
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            FlexWrap = FlexWrap.Wrap,
            Size = new Size<Dimension>(Dimension.Px(200f), Dimension.Px(200f)),
        });
        var child1 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(80f), Dimension.Px(40f)),
        });
        var child2 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(80f), Dimension.Px(40f)),
        });
        var child3 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(80f), Dimension.Px(40f)),
        });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.AddChild(root, child3);
        tree.ComputeRoot(root, 200f, 200f);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);
        var l3 = tree.GetNodeLayout(child3);

        // First line: child1 at X=0, child2 at X=80
        Assert.Equal(0f, l1.Location.X);
        Assert.Equal(0f, l1.Location.Y);
        Assert.Equal(80f, l2.Location.X);
        Assert.Equal(0f, l2.Location.Y);

        // Second line: child3 at X=0, Y > 0
        Assert.Equal(0f, l3.Location.X);
        Assert.True(l3.Location.Y > 0f, "child3 should be on a second line");
    }

    [Fact]
    public void Wrap_ItemsCrossAxisStretch()
    {
        // Wrapped items should stretch to their line's cross size.
        // Force two lines so that each line's cross size is determined by
        // its tallest item (not clamped to the container like single-line).
        // Line 1: child1 (auto height) + child2 (60px tall) -> line cross = 60.
        // Line 2: child3 (30px tall) on its own -> line cross = 30.
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            FlexWrap = FlexWrap.Wrap,
            AlignContent = AlignContent.FlexStart,
            Size = new Size<Dimension>(Dimension.Px(200f), Dimension.Px(200f)),
        });
        var child1 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(80f), Dimension.Auto),
        });
        var child2 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(80f), Dimension.Px(60f)),
        });
        var child3 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(150f), Dimension.Px(30f)),
        });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.AddChild(root, child3);
        tree.ComputeRoot(root, 200f, 200f);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);

        // child2 is 60px tall, making the first line's cross size 60px.
        // child1 has auto height and should stretch to match line cross size.
        Assert.Equal(60f, l2.Size.Height);
        Assert.Equal(l2.Size.Height, l1.Size.Height);
    }

    [Fact]
    public void WrapReverse_ReversesLineOrder()
    {
        // FlexWrap=WrapReverse: first line items should be at the bottom,
        // last line items at the top.
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            FlexWrap = FlexWrap.WrapReverse,
            AlignContent = AlignContent.FlexStart,
            Size = new Size<Dimension>(Dimension.Px(200f), Dimension.Px(200f)),
        });
        var child1 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(150f), Dimension.Px(40f)),
        });
        var child2 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(150f), Dimension.Px(40f)),
        });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.ComputeRoot(root, 200f, 200f);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);

        // With wrap-reverse, the first line (child1) should be placed below
        // the second line (child2).
        Assert.True(l1.Location.Y > l2.Location.Y,
            "First line should appear below second line in wrap-reverse");
    }

    [Fact]
    public void NoWrap_OverflowsContainer()
    {
        // Default NoWrap: 3 children 100px each in a 200px container.
        // All items stay on one line and shrink (FlexShrink=1 by default).
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(200f), Dimension.Px(100f)),
        });
        var child1 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(100f), Dimension.Px(50f)),
        });
        var child2 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(100f), Dimension.Px(50f)),
        });
        var child3 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(100f), Dimension.Px(50f)),
        });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.AddChild(root, child3);
        tree.ComputeRoot(root, 200f, 100f);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);
        var l3 = tree.GetNodeLayout(child3);

        // All items on the same line (Y=0)
        Assert.Equal(0f, l1.Location.Y);
        Assert.Equal(0f, l2.Location.Y);
        Assert.Equal(0f, l3.Location.Y);

        // Items should have shrunk from 100px each to fit in 200px total.
        // With equal flex-basis and equal shrink factor, each gets 200/3.
        float expectedWidth = 200f / 3f;
        Assert.Equal(expectedWidth, l1.Size.Width, 1f);
        Assert.Equal(expectedWidth, l2.Size.Width, 1f);
        Assert.Equal(expectedWidth, l3.Size.Width, 1f);

        // Positions should be sequential
        Assert.Equal(0f, l1.Location.X, 1f);
        Assert.Equal(expectedWidth, l2.Location.X, 1f);
        Assert.Equal(expectedWidth * 2f, l3.Location.X, 1f);
    }
}
