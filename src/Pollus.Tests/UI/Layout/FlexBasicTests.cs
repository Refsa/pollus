using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class FlexBasicTests
{
    static LayoutStyle DefaultStyle => LayoutStyle.Default;

    [Fact]
    public void SingleChild_FixedSize_Row()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(50f)),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(50f, childLayout.Size.Width);
        Assert.Equal(50f, childLayout.Size.Height);
        Assert.Equal(0f, childLayout.Location.X);
        Assert.Equal(0f, childLayout.Location.Y);
    }

    [Fact]
    public void TwoChildren_FixedSize_Row()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child1 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(30f)),
        });
        var child2 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(80f), Dimension.Px(30f)),
        });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.ComputeRoot(root, 200f, 200f);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);

        Assert.Equal(50f, l1.Size.Width);
        Assert.Equal(0f, l1.Location.X);

        Assert.Equal(80f, l2.Size.Width);
        Assert.Equal(50f, l2.Location.X); // positioned after child1
    }

    [Fact]
    public void Column_Direction()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with { FlexDirection = FlexDirection.Column });
        var child1 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(30f)),
        });
        var child2 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(40f)),
        });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.ComputeRoot(root, 200f, 200f);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);

        Assert.Equal(0f, l1.Location.Y);
        Assert.Equal(30f, l2.Location.Y); // positioned after child1
    }

    [Fact]
    public void DisplayNone_ExcludedFromLayout()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child1 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(50f)),
        });
        var child2 = tree.AddNode(DefaultStyle with
        {
            Display = Display.None,
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(50f)),
        });
        var child3 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(50f)),
        });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.AddChild(root, child3);
        tree.ComputeRoot(root, 200f, 200f);

        var l2 = tree.GetNodeLayout(child2);
        Assert.Equal(0f, l2.Size.Width);
        Assert.Equal(0f, l2.Size.Height);

        var l3 = tree.GetNodeLayout(child3);
        Assert.Equal(50f, l3.Location.X); // right after child1, child2 is skipped
    }

    [Fact]
    public void Root_FixedSize()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        tree.ComputeRoot(root, 300f, 400f);

        var rootLayout = tree.GetNodeLayout(root);
        Assert.Equal(300f, rootLayout.Size.Width);
        Assert.Equal(400f, rootLayout.Size.Height);
    }

    [Fact]
    public void Children_StretchCrossAxis_InRow()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            AlignItems = AlignItems.Stretch,
            Size = new Size<Dimension>(Dimension.Px(200f), Dimension.Px(100f)),
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Auto),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 100f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(50f, childLayout.Size.Width);
        Assert.Equal(100f, childLayout.Size.Height); // stretched to container cross axis
    }
}
