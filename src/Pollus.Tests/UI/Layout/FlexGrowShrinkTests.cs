using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class FlexGrowShrinkTests
{
    static LayoutStyle DefaultStyle => LayoutStyle.Default;

    [Fact]
    public void FlexGrow_SingleChild_FillsContainer()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child = tree.AddNode(DefaultStyle with
        {
            FlexGrow = 1f,
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(200f, childLayout.Size.Width);
    }

    [Fact]
    public void FlexGrow_TwoChildren_EqualGrow()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child1 = tree.AddNode(DefaultStyle with { FlexGrow = 1f });
        var child2 = tree.AddNode(DefaultStyle with { FlexGrow = 1f });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.ComputeRoot(root, 200f, 200f);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);
        Assert.Equal(100f, l1.Size.Width);
        Assert.Equal(100f, l2.Size.Width);
    }

    [Fact]
    public void FlexGrow_UnequalRatios()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child1 = tree.AddNode(DefaultStyle with { FlexGrow = 1f });
        var child2 = tree.AddNode(DefaultStyle with { FlexGrow = 3f });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.ComputeRoot(root, 200f, 200f);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);
        Assert.Equal(50f, l1.Size.Width);
        Assert.Equal(150f, l2.Size.Width);
    }

    [Fact]
    public void FlexGrow_WithFixedChild()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child1 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Auto),
        });
        var child2 = tree.AddNode(DefaultStyle with { FlexGrow = 1f });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.ComputeRoot(root, 200f, 200f);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);
        Assert.Equal(50f, l1.Size.Width);
        Assert.Equal(150f, l2.Size.Width);
    }

    [Fact]
    public void FlexShrink_TwoChildren_Overflow()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child1 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(150f), Dimension.Auto),
        });
        var child2 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(150f), Dimension.Auto),
        });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.ComputeRoot(root, 200f, 200f);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);
        Assert.Equal(100f, l1.Size.Width, 1f);
        Assert.Equal(100f, l2.Size.Width, 1f);
    }

    [Fact]
    public void FlexShrink_ZeroShrink_NoShrink()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child1 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(150f), Dimension.Auto),
            FlexShrink = 0f,
        });
        var child2 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(150f), Dimension.Auto),
        });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.ComputeRoot(root, 200f, 200f);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);
        Assert.Equal(150f, l1.Size.Width);
        Assert.Equal(50f, l2.Size.Width, 1f);
    }

    [Fact]
    public void FlexGrow_Column_Direction()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with { FlexDirection = FlexDirection.Column });
        var child = tree.AddNode(DefaultStyle with
        {
            FlexGrow = 1f,
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(200f, childLayout.Size.Height);
    }
}
