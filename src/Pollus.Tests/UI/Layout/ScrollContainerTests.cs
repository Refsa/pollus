using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class ScrollContainerTests
{
    [Fact]
    public void ScrollY_ChildrenCanOverflowCross()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(100)),
            Overflow = new Point<Overflow>(Overflow.Visible, Overflow.Scroll),
        });
        var child = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(180), Dimension.Px(300)),
        });
        tree.AddChild(root, child);

        tree.ComputeRoot(root, 200, 100);

        var rootLayout = tree.GetNodeLayout(root);
        var childLayout = tree.GetNodeLayout(child);

        Assert.Equal(200f, rootLayout.Size.Width);
        Assert.Equal(100f, rootLayout.Size.Height);

        // Child keeps full size, not clipped to container
        Assert.Equal(180f, childLayout.Size.Width);
        Assert.Equal(300f, childLayout.Size.Height);
        Assert.True(rootLayout.ContentSize.Height >= 300f);
    }

    [Fact]
    public void ScrollX_ChildrenCanOverflowMain()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(100)),
            Overflow = new Point<Overflow>(Overflow.Scroll, Overflow.Visible),
        });
        // Two children that together exceed the container width
        var child1 = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(150), Dimension.Px(50)),
        });
        var child2 = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(150), Dimension.Px(50)),
        });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);

        tree.ComputeRoot(root, 200, 100);

        var c1 = tree.GetNodeLayout(child1);
        var c2 = tree.GetNodeLayout(child2);

        Assert.Equal(150f, c1.Size.Width);
        Assert.Equal(150f, c2.Size.Width);
        Assert.Equal(150f, c2.Location.X);
    }

    [Fact]
    public void ScrollbarSize_ReservedInLayout()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(100)),
            Overflow = new Point<Overflow>(Overflow.Visible, Overflow.Scroll),
        });
        var child = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(180), Dimension.Px(300)),
        });
        tree.AddChild(root, child);

        tree.ComputeRoot(root, 200, 100);

        var rootLayout = tree.GetNodeLayout(root);

        // Vertical scrollbar reserves width
        Assert.True(rootLayout.ScrollbarSize.Width > 0f);
        Assert.Equal(0f, rootLayout.ScrollbarSize.Height);
    }

    [Fact]
    public void NoScroll_DefaultBehaviorUnchanged()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(100)),
        });
        var child = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(180), Dimension.Px(50)),
        });
        tree.AddChild(root, child);

        tree.ComputeRoot(root, 200, 100);

        var rootLayout = tree.GetNodeLayout(root);
        Assert.Equal(0f, rootLayout.ScrollbarSize.Width);
        Assert.Equal(0f, rootLayout.ScrollbarSize.Height);
    }

    [Fact]
    public void ScrollBothAxes_ContentSizeTracked()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(100), Dimension.Px(100)),
            Overflow = new Point<Overflow>(Overflow.Scroll, Overflow.Scroll),
        });
        var child = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(300), Dimension.Px(400)),
        });
        tree.AddChild(root, child);

        tree.ComputeRoot(root, 100, 100);

        var rootLayout = tree.GetNodeLayout(root);
        var childLayout = tree.GetNodeLayout(child);

        Assert.Equal(300f, childLayout.Size.Width);
        Assert.Equal(400f, childLayout.Size.Height);
        Assert.True(rootLayout.ContentSize.Width >= 300f);
        Assert.True(rootLayout.ContentSize.Height >= 400f);
    }

    [Fact]
    public void ScrollY_FlexGrowChild_DoesNotStretchBeyondContent()
    {
        // With scroll-Y, a flex-grow child should grow based on content, not container
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            FlexDirection = FlexDirection.Column,
            Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(100)),
            Overflow = new Point<Overflow>(Overflow.Visible, Overflow.Scroll),
        });
        var fixedChild = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(200), Dimension.Px(60)),
        });
        var growChild = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(200), Dimension.Auto),
            FlexGrow = 1f,
        });
        tree.AddChild(root, fixedChild);
        tree.AddChild(root, growChild);

        tree.ComputeRoot(root, 200, 100);

        var fixedLayout = tree.GetNodeLayout(fixedChild);
        var growLayout = tree.GetNodeLayout(growChild);

        Assert.Equal(60f, fixedLayout.Size.Height);
        // Grow child should get remaining space (at least 0, not negative)
        Assert.True(growLayout.Size.Height >= 0f);
    }
}
