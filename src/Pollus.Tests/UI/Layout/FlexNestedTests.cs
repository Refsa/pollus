using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class FlexNestedTests
{
    static LayoutStyle DefaultStyle => LayoutStyle.Default;

    [Fact]
    public void NestedFlex_ChildContainer()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle); // row
        var child1 = tree.AddNode(DefaultStyle with
        {
            FlexDirection = FlexDirection.Column,
            Size = new Size<Length>(Length.Px(100f), Length.Auto),
        });
        var grandchild = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Length>(Length.Px(50f), Length.Px(50f)),
        });
        tree.AddChild(root, child1);
        tree.AddChild(child1, grandchild);
        tree.ComputeRoot(root, 200f, 200f);

        var grandchildLayout = tree.GetNodeLayout(grandchild);
        Assert.Equal(0f, grandchildLayout.Location.Y);
        Assert.Equal(50f, grandchildLayout.Size.Width);
        Assert.Equal(50f, grandchildLayout.Size.Height);
    }

    [Fact]
    public void NestedFlex_InnerFlexGrow()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle); // row, 200px wide
        var child1 = tree.AddNode(DefaultStyle with
        {
            FlexDirection = FlexDirection.Column,
            FlexGrow = 1f,
        });
        var grandchild1 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Length>(Length.Auto, Length.Px(30f)),
        });
        var grandchild2 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Length>(Length.Auto, Length.Px(30f)),
        });
        tree.AddChild(root, child1);
        tree.AddChild(child1, grandchild1);
        tree.AddChild(child1, grandchild2);
        tree.ComputeRoot(root, 200f, 200f);

        var child1Layout = tree.GetNodeLayout(child1);
        Assert.Equal(200f, child1Layout.Size.Width);

        var gc1Layout = tree.GetNodeLayout(grandchild1);
        var gc2Layout = tree.GetNodeLayout(grandchild2);
        Assert.Equal(0f, gc1Layout.Location.Y);
        Assert.Equal(30f, gc2Layout.Location.Y);
    }

    [Fact]
    public void MeasureFunc_LeafNode()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            AlignItems = AlignItems.FlexStart,
        });
        var child = tree.AddNode(DefaultStyle);
        tree.AddChild(root, child);
        tree.SetMeasureFunc(child, input => new LayoutOutput
        {
            Size = new Size<float>(75f, 25f),
        });
        tree.ComputeRoot(root, 200f, 200f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(75f, childLayout.Size.Width);
        Assert.Equal(25f, childLayout.Size.Height);
    }
}
