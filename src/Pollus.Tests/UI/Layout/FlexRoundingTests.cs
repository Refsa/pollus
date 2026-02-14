using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class FlexRoundingTests
{
    static LayoutStyle DefaultStyle => LayoutStyle.Default;

    [Fact]
    public void Round_IntegerValues_Unchanged()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(30f)),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var self = tree;
        RoundLayout.Round(ref self, root);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(50f, childLayout.Size.Width);
        Assert.Equal(30f, childLayout.Size.Height);
        Assert.Equal(0f, childLayout.Location.X);
        Assert.Equal(0f, childLayout.Location.Y);
    }

    [Fact]
    public void Round_SubPixelPosition_RoundedToNearest()
    {
        // Create a layout with sub-pixel padding that shifts children
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            Padding = new Rect<Length>(
                Length.Px(10.3f), Length.Px(10.3f),
                Length.Px(10.7f), Length.Px(10.7f)
            ),
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(30f)),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var self = tree;
        RoundLayout.Round(ref self, root);

        var childLayout = tree.GetNodeLayout(child);
        // Child position is relative to parent, rounded cumulatively
        // Before rounding: child at (10.3, 10.7) absolutely
        // round(10.3) - round(0) = 10 - 0 = 10
        // round(10.7) - round(0) = 11 - 0 = 11
        Assert.Equal(10f, childLayout.Location.X);
        Assert.Equal(11f, childLayout.Location.Y);

        // Child border/padding on the child node itself should be rounded
        // (child has no padding/border, so they stay zero)
        Assert.Equal(0f, childLayout.Padding.Left);
    }

    [Fact]
    public void Round_CumulativePosition_NoGapAccumulation()
    {
        // Three children each at x.333... positions
        // Without cumulative rounding, gaps accumulate
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);

        // Three children each 33.33px wide in a 100px container
        var child1 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(33.33f), Dimension.Px(20f)),
        });
        var child2 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(33.33f), Dimension.Px(20f)),
        });
        var child3 = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(33.34f), Dimension.Px(20f)),
        });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.AddChild(root, child3);
        tree.ComputeRoot(root, 100f, 100f);

        var self = tree;
        RoundLayout.Round(ref self, root);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);
        var l3 = tree.GetNodeLayout(child3);

        // All positions and sizes should be integer
        Assert.Equal(MathF.Round(l1.Location.X), l1.Location.X);
        Assert.Equal(MathF.Round(l2.Location.X), l2.Location.X);
        Assert.Equal(MathF.Round(l3.Location.X), l3.Location.X);
        Assert.Equal(MathF.Round(l1.Size.Width), l1.Size.Width);
        Assert.Equal(MathF.Round(l2.Size.Width), l2.Size.Width);
        Assert.Equal(MathF.Round(l3.Size.Width), l3.Size.Width);

        // No pixel gaps between items:
        // child1 end = child1.X + child1.Width should equal child2.X
        Assert.Equal(l1.Location.X + l1.Size.Width, l2.Location.X);
        Assert.Equal(l2.Location.X + l2.Size.Width, l3.Location.X);
    }

    [Fact]
    public void Round_NestedNodes_CumulativeFromRoot()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var parent = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(100.5f), Dimension.Px(100.5f)),
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50.5f), Dimension.Px(50.5f)),
        });
        tree.AddChild(root, parent);
        tree.AddChild(parent, child);
        tree.ComputeRoot(root, 200f, 200f);

        var self = tree;
        RoundLayout.Round(ref self, root);

        var parentLayout = tree.GetNodeLayout(parent);
        var childLayout = tree.GetNodeLayout(child);

        // Parent at (0,0) with sub-pixel size gets rounded
        Assert.Equal(0f, parentLayout.Location.X);
        Assert.Equal(0f, parentLayout.Location.Y);

        // All sizes and positions are integers
        Assert.Equal(MathF.Round(parentLayout.Size.Width), parentLayout.Size.Width);
        Assert.Equal(MathF.Round(childLayout.Size.Width), childLayout.Size.Width);
        Assert.Equal(MathF.Round(childLayout.Location.X), childLayout.Location.X);
    }

    [Fact]
    public void Round_DisplayNone_Skipped()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var hidden = tree.AddNode(DefaultStyle with
        {
            Display = Display.None,
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(50f)),
        });
        var visible = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(50f)),
        });
        tree.AddChild(root, hidden);
        tree.AddChild(root, visible);
        tree.ComputeRoot(root, 200f, 200f);

        var self = tree;
        RoundLayout.Round(ref self, root);

        var hiddenLayout = tree.GetNodeLayout(hidden);
        Assert.Equal(0f, hiddenLayout.Size.Width);
        Assert.Equal(0f, hiddenLayout.Size.Height);

        var visibleLayout = tree.GetNodeLayout(visible);
        Assert.Equal(50f, visibleLayout.Size.Width);
    }
}
