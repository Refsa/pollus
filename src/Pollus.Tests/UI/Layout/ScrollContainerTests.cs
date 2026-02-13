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

    [Fact]
    public void ScrollY_StretchedInRow_ContentSizeExceedsSize()
    {
        // Mimics UIRectExample: a Row container with a sidebar and a scroll panel.
        // The scroll panel gets its height from stretch (cross axis of the Row).
        // Its children overflow, so ContentSize.Height > Size.Height.
        var tree = new TestLayoutTree();

        // Root: Column, fixed viewport
        var root = tree.AddNode(LayoutStyle.Default with
        {
            FlexDirection = FlexDirection.Column,
            Size = new Size<Dimension>(Dimension.Px(800), Dimension.Px(400)),
        });

        // Header: fixed height
        var header = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Auto, Dimension.Px(40)),
        });

        // ContentRow: Row, flex-grow
        var contentRow = tree.AddNode(LayoutStyle.Default with
        {
            FlexGrow = 1f,
            FlexDirection = FlexDirection.Row,
        });

        // Sidebar: fixed width
        var sidebar = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(150), Dimension.Auto),
        });

        // ScrollPanel: flex-grow, Column, Overflow.Scroll on Y
        var scrollPanel = tree.AddNode(LayoutStyle.Default with
        {
            FlexGrow = 1f,
            FlexDirection = FlexDirection.Column,
            Overflow = new Point<Overflow>(Overflow.Hidden, Overflow.Scroll),
        });

        // Many children inside the scroll panel that exceed available height
        for (int i = 0; i < 10; i++)
        {
            var section = tree.AddNode(LayoutStyle.Default with
            {
                Size = new Size<Dimension>(Dimension.Auto, Dimension.Px(60)),
            });
            tree.AddChild(scrollPanel, section);
        }
        // Total children height = 10 * 60 = 600px, container ~360px

        tree.AddChild(root, header);
        tree.AddChild(root, contentRow);
        tree.AddChild(contentRow, sidebar);
        tree.AddChild(contentRow, scrollPanel);

        tree.ComputeRoot(root, 800, 400);

        var rootLayout = tree.GetNodeLayout(root);
        var contentRowLayout = tree.GetNodeLayout(contentRow);
        var scrollPanelLayout = tree.GetNodeLayout(scrollPanel);

        // Root is 800x400
        Assert.Equal(800f, rootLayout.Size.Width);
        Assert.Equal(400f, rootLayout.Size.Height);

        // ContentRow fills remaining height: 400 - 40 = 360
        Assert.True(contentRowLayout.Size.Height > 300f,
            $"ContentRow height ({contentRowLayout.Size.Height}) should be ~360");

        // ScrollPanel is stretched to contentRow height (constrained, not inflated by content)
        Assert.Equal(contentRowLayout.Size.Height, scrollPanelLayout.Size.Height);

        // ContentSize should exceed Size (children overflow: 10*60=600 > ~360)
        Assert.True(scrollPanelLayout.ContentSize.Height > scrollPanelLayout.Size.Height,
            $"ContentSize.Height ({scrollPanelLayout.ContentSize.Height}) should exceed Size.Height ({scrollPanelLayout.Size.Height})");
    }
}
