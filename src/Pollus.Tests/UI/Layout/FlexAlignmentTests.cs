using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class FlexAlignmentTests
{
    static LayoutStyle DefaultStyle => LayoutStyle.Default;

    static Size<Dimension> FixedSize(float w, float h) =>
        new(Dimension.Px(w), Dimension.Px(h));

    [Fact]
    public void JustifyContent_Center()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            JustifyContent = JustifyContent.Center,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(50f, 50f),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        Assert.Equal(50f, layout.Size.Width);
        Assert.Equal(75f, layout.Location.X);
    }

    [Fact]
    public void JustifyContent_FlexEnd()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            JustifyContent = JustifyContent.FlexEnd,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(50f, 50f),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        Assert.Equal(50f, layout.Size.Width);
        Assert.Equal(150f, layout.Location.X);
    }

    [Fact]
    public void JustifyContent_SpaceBetween()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            JustifyContent = JustifyContent.SpaceBetween,
        });
        var child1 = tree.AddNode(DefaultStyle with { Size = FixedSize(50f, 50f) });
        var child2 = tree.AddNode(DefaultStyle with { Size = FixedSize(50f, 50f) });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.ComputeRoot(root, 200f, 200f);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);
        Assert.Equal(0f, l1.Location.X);
        Assert.Equal(150f, l2.Location.X);
    }

    [Fact]
    public void JustifyContent_SpaceAround()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            JustifyContent = JustifyContent.SpaceAround,
        });
        var child1 = tree.AddNode(DefaultStyle with { Size = FixedSize(50f, 50f) });
        var child2 = tree.AddNode(DefaultStyle with { Size = FixedSize(50f, 50f) });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.ComputeRoot(root, 200f, 200f);

        // 200 - 50 - 50 = 100 free space. SpaceAround: 100/2 = 50 per item, 25 on each side.
        // First child at X=25, second at X=125.
        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);
        Assert.Equal(25f, l1.Location.X);
        Assert.Equal(125f, l2.Location.X);
    }

    [Fact]
    public void AlignItems_Center()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            AlignItems = AlignItems.Center,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(50f, 50f),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        Assert.Equal(50f, layout.Size.Height);
        Assert.Equal(75f, layout.Location.Y);
    }

    [Fact]
    public void AlignItems_FlexEnd()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            AlignItems = AlignItems.FlexEnd,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(50f, 50f),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        Assert.Equal(50f, layout.Size.Height);
        Assert.Equal(150f, layout.Location.Y);
    }

    [Fact]
    public void AlignSelf_Override()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            AlignItems = AlignItems.FlexStart,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(50f, 50f),
            AlignSelf = AlignSelf.FlexEnd,
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        Assert.Equal(50f, layout.Size.Height);
        Assert.Equal(150f, layout.Location.Y);
    }
}
