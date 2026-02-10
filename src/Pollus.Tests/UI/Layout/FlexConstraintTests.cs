using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class FlexConstraintTests
{
    static LayoutStyle DefaultStyle => LayoutStyle.Default;

    [Fact]
    public void MinWidth_PreventsShrinking()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child1 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(150f), Dimension.Auto),
            MinSize = new Size<Dimension>(Dimension.Px(80f), Dimension.Auto),
        });
        var child2 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(150f), Dimension.Auto),
        });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.ComputeRoot(root, 200f, 200f);

        var l1 = tree.GetNodeLayout(child1);
        Assert.True(l1.Size.Width >= 80f, $"Expected width >= 80, got {l1.Size.Width}");
    }

    [Fact]
    public void MaxWidth_PreventsGrowing()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child = tree.AddNode(DefaultStyle with
        {
            FlexGrow = 1f,
            MaxSize = new Size<Dimension>(Dimension.Px(80f), Dimension.Auto),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(80f, childLayout.Size.Width);
    }

    [Fact]
    public void MinHeight_InColumn()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with { FlexDirection = FlexDirection.Column });
        var child = tree.AddNode(DefaultStyle with
        {
            MinSize = new Size<Dimension>(Dimension.Auto, Dimension.Px(100f)),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.True(childLayout.Size.Height >= 100f, $"Expected height >= 100, got {childLayout.Size.Height}");
    }

    [Fact]
    public void MaxHeight_InColumn()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with { FlexDirection = FlexDirection.Column });
        var child = tree.AddNode(DefaultStyle with
        {
            FlexGrow = 1f,
            MaxSize = new Size<Dimension>(Dimension.Auto, Dimension.Px(30f)),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(30f, childLayout.Size.Height);
    }

    [Fact]
    public void FlexBasis_Px()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child = tree.AddNode(DefaultStyle with
        {
            FlexBasis = Dimension.Px(100f),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(100f, childLayout.Size.Width);
    }

    [Fact]
    public void FlexBasis_Percent()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child = tree.AddNode(DefaultStyle with
        {
            FlexBasis = Dimension.Percent(0.5f),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(100f, childLayout.Size.Width);
    }
}
