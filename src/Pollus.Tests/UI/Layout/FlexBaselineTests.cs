using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class FlexBaselineTests
{
    [Fact]
    public void Baseline_ItemsAlignOnBaseline()
    {
        // Two children with measure funcs providing different baselines.
        // Child1: 100x30, baseline at 20 (above=20, below=10)
        // Child2: 100x50, baseline at 40 (above=40, below=10)
        // With baseline alignment in a row container:
        // Max above = 40, max below = 10, line cross = 50
        // Child1 offset = 40 - 20 = 20 (shifted down)
        // Child2 offset = 40 - 40 = 0

        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            AlignItems = AlignItems.Baseline,
        });
        var child1 = tree.AddNode(LayoutStyle.Default);
        var child2 = tree.AddNode(LayoutStyle.Default);
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);

        tree.SetMeasureFunc(child1, _ => new LayoutOutput
        {
            Size = new Size<float>(100, 30),
            FirstBaselines = new Point<float?>(null, 20f), // Y = 20 (vertical baseline for row)
        });
        tree.SetMeasureFunc(child2, _ => new LayoutOutput
        {
            Size = new Size<float>(100, 50),
            FirstBaselines = new Point<float?>(null, 40f), // Y = 40
        });

        tree.ComputeRoot(root, 400, 200);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);

        // Child1 top = 40 - 20 = 20, Child2 top = 40 - 40 = 0
        Assert.Equal(20f, l1.Location.Y);
        Assert.Equal(0f, l2.Location.Y);
    }

    [Fact]
    public void Baseline_MixedWithStretch()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            AlignItems = AlignItems.Baseline,
        });
        var baselineChild = tree.AddNode(LayoutStyle.Default);
        var stretchChild = tree.AddNode(LayoutStyle.Default with
        {
            AlignSelf = AlignSelf.Stretch,
            Size = new Size<Dimension>(Dimension.Px(80), Dimension.Auto),
        });
        tree.AddChild(root, baselineChild);
        tree.AddChild(root, stretchChild);

        tree.SetMeasureFunc(baselineChild, _ => new LayoutOutput
        {
            Size = new Size<float>(100, 40),
            FirstBaselines = new Point<float?>(null, 30f),
        });

        tree.ComputeRoot(root, 400, 200);

        var bl = tree.GetNodeLayout(baselineChild);
        var sl = tree.GetNodeLayout(stretchChild);

        Assert.Equal(100f, bl.Size.Width);
        Assert.Equal(40f, bl.Size.Height);
        Assert.True(sl.Size.Height > 0f);
    }

    [Fact]
    public void Baseline_NoBaselineFallsBackToBottom()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            AlignItems = AlignItems.Baseline,
        });
        var withBaseline = tree.AddNode(LayoutStyle.Default);
        var withoutBaseline = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Dimension>(Dimension.Px(80), Dimension.Px(30)),
        });
        tree.AddChild(root, withBaseline);
        tree.AddChild(root, withoutBaseline);

        tree.SetMeasureFunc(withBaseline, _ => new LayoutOutput
        {
            Size = new Size<float>(100, 50),
            FirstBaselines = new Point<float?>(null, 35f),
        });

        tree.ComputeRoot(root, 400, 200);

        var l1 = tree.GetNodeLayout(withBaseline);
        var l2 = tree.GetNodeLayout(withoutBaseline);

        // withBaseline: above=35, below=15
        // withoutBaseline: no baseline, fallback = height (30), above=30, below=0
        // max above = 35, max below = 15
        // Line cross = 50
        // l1 offset = 35 - 35 = 0
        // l2 offset = 35 - 30 = 5
        Assert.Equal(0f, l1.Location.Y);
        Assert.Equal(5f, l2.Location.Y);
    }

    [Fact]
    public void Baseline_ContainerPropagatesFirstChildBaseline()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            AlignItems = AlignItems.Baseline,
        });
        var container = tree.AddNode(LayoutStyle.Default with
        {
            FlexDirection = FlexDirection.Column,
        });
        var leaf = tree.AddNode(LayoutStyle.Default);
        var directChild = tree.AddNode(LayoutStyle.Default);
        tree.AddChild(root, container);
        tree.AddChild(root, directChild);
        tree.AddChild(container, leaf);

        tree.SetMeasureFunc(leaf, _ => new LayoutOutput
        {
            Size = new Size<float>(80, 30),
            FirstBaselines = new Point<float?>(null, 20f),
        });
        tree.SetMeasureFunc(directChild, _ => new LayoutOutput
        {
            Size = new Size<float>(80, 50),
            FirstBaselines = new Point<float?>(null, 40f),
        });

        var rootOutput = tree.ComputeRoot(root, 400, 200);

        var containerLayout = tree.GetNodeLayout(container);
        var directLayout = tree.GetNodeLayout(directChild);

        // container baseline = leaf.Y + leaf.baseline = 0 + 20 = 20
        // directChild baseline = 40
        // max above = max(20, 40) = 40
        // container offset = 40 - 20 = 20
        // directChild offset = 40 - 40 = 0
        Assert.Equal(20f, containerLayout.Location.Y);
        Assert.Equal(0f, directLayout.Location.Y);
    }
}
