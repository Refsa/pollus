using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class FlexGapTests
{
    static LayoutStyle DefaultStyle => LayoutStyle.Default;

    [Fact]
    public void Gap_Row_BetweenItems()
    {
        // 2 children 50px each, gap 10px -> child2 at X=60 (50+10).
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            Gap = new Size<LengthPercentage>(LengthPercentage.Px(10f), LengthPercentage.Zero),
            Size = new Size<Dimension>(Dimension.Px(200f), Dimension.Px(100f)),
        });
        var child1 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(30f)),
        });
        var child2 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(30f)),
        });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.ComputeRoot(root, 200f, 100f);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);

        Assert.Equal(0f, l1.Location.X);
        Assert.Equal(60f, l2.Location.X); // 50 + 10 gap
    }

    [Fact]
    public void Gap_Column_BetweenItems()
    {
        // Column direction, 2 children 30px each, gap 10px -> child2 at Y=40 (30+10).
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            FlexDirection = FlexDirection.Column,
            Gap = new Size<LengthPercentage>(LengthPercentage.Zero, LengthPercentage.Px(10f)),
            Size = new Size<Dimension>(Dimension.Px(200f), Dimension.Px(200f)),
        });
        var child1 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(30f)),
        });
        var child2 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(30f)),
        });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.ComputeRoot(root, 200f, 200f);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);

        Assert.Equal(0f, l1.Location.Y);
        Assert.Equal(40f, l2.Location.Y); // 30 + 10 gap
    }

    [Fact]
    public void Gap_DoesNotAffectEdges()
    {
        // Gap only applies between items, not before the first or after the last.
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            Gap = new Size<LengthPercentage>(LengthPercentage.Px(20f), LengthPercentage.Zero),
            Size = new Size<Dimension>(Dimension.Px(300f), Dimension.Px(100f)),
        });
        var child1 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(30f)),
        });
        var child2 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(30f)),
        });
        var child3 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(30f)),
        });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.AddChild(root, child3);
        tree.ComputeRoot(root, 300f, 100f);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);
        var l3 = tree.GetNodeLayout(child3);

        // First item starts at X=0 (no gap before first item)
        Assert.Equal(0f, l1.Location.X);
        // Second item at 50 + 20 gap = 70
        Assert.Equal(70f, l2.Location.X);
        // Third item at 70 + 50 + 20 gap = 140
        Assert.Equal(140f, l3.Location.X);
    }

    [Fact]
    public void Gap_WithWrap_CrossGap()
    {
        // Wrapped items should have a cross gap between lines.
        // Container 200x200, children 150px wide (forces wrap), cross gap 20px.
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            FlexWrap = FlexWrap.Wrap,
            Gap = new Size<LengthPercentage>(LengthPercentage.Zero, LengthPercentage.Px(20f)),
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

        // First line at Y=0
        Assert.Equal(0f, l1.Location.Y);

        // Second line should be at first line's cross size + cross gap.
        // Line 1 cross size is 40px (from child1), so child2 Y = 40 + 20 = 60.
        Assert.Equal(60f, l2.Location.Y);
    }
}
