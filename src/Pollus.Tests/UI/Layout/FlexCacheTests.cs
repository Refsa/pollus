using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class FlexCacheTests
{
    [Fact]
    public void SameInputTwice_SecondCallHitsCache()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(200), Length.Px(200)),
        });
        var child = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(100), Length.Px(50)),
        });
        tree.AddChild(root, child);

        var input = new LayoutInput
        {
            RunMode = RunMode.ComputeSize,
            SizingMode = SizingMode.ContentSize,
            Axis = RequestedAxis.Both,
            KnownDimensions = Size<float?>.Zero,
            ParentSize = new Size<float?>(200f, 200f),
            AvailableSpace = new Size<AvailableSpace>(
                AvailableSpace.Definite(200f),
                AvailableSpace.Definite(200f)),
        };

        var self1 = tree;
        var result1 = FlexLayout.ComputeFlexbox(ref self1, root, input);
        int callsAfterFirst = tree.ComputeCallCount;

        var self2 = tree;
        var result2 = FlexLayout.ComputeFlexbox(ref self2, root, input);
        int callsAfterSecond = tree.ComputeCallCount;

        Assert.Equal(callsAfterFirst, callsAfterSecond);
        Assert.Equal(result1.Size, result2.Size);
    }

    [Fact]
    public void DifferentRunMode_CacheMiss()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(200), Length.Px(200)),
        });
        var child = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(100), Length.Px(50)),
        });
        tree.AddChild(root, child);

        var input1 = new LayoutInput
        {
            RunMode = RunMode.ComputeSize,
            KnownDimensions = new Size<float?>(200f, 200f),
            ParentSize = new Size<float?>(200f, 200f),
            AvailableSpace = new Size<AvailableSpace>(
                AvailableSpace.Definite(200f),
                AvailableSpace.Definite(200f)),
        };

        var self1 = tree;
        FlexLayout.ComputeFlexbox(ref self1, root, input1);
        int callsAfterFirst = tree.ComputeCallCount;

        var input2 = input1 with { RunMode = RunMode.PerformLayout };
        var self2 = tree;
        FlexLayout.ComputeFlexbox(ref self2, root, input2);
        int callsAfterSecond = tree.ComputeCallCount;

        Assert.True(callsAfterSecond > callsAfterFirst);
    }

    [Fact]
    public void MarkDirty_ClearsCache()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(200), Length.Px(200)),
        });
        var child = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(100), Length.Px(50)),
        });
        tree.AddChild(root, child);

        var input = new LayoutInput
        {
            RunMode = RunMode.ComputeSize,
            KnownDimensions = Size<float?>.Zero,
            ParentSize = new Size<float?>(200f, 200f),
            AvailableSpace = new Size<AvailableSpace>(
                AvailableSpace.Definite(200f),
                AvailableSpace.Definite(200f)),
        };

        var self1 = tree;
        FlexLayout.ComputeFlexbox(ref self1, root, input);
        int callsAfterFirst = tree.ComputeCallCount;

        tree.MarkDirty(root);

        var self2 = tree;
        FlexLayout.ComputeFlexbox(ref self2, root, input);
        int callsAfterSecond = tree.ComputeCallCount;

        Assert.True(callsAfterSecond > callsAfterFirst);
    }

    [Fact]
    public void CacheTransparent_ResultsIdentical()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(400), Length.Px(300)),
            FlexDirection = FlexDirection.Row,
        });
        var a = tree.AddNode(LayoutStyle.Default with { FlexGrow = 1 });
        var b = tree.AddNode(LayoutStyle.Default with { FlexGrow = 2 });
        var c = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(50), Length.Auto),
        });
        tree.AddChild(root, a);
        tree.AddChild(root, b);
        tree.AddChild(root, c);

        tree.ComputeRoot(root, 400, 300);
        var layoutA1 = tree.GetNodeLayout(a);
        var layoutB1 = tree.GetNodeLayout(b);
        var layoutC1 = tree.GetNodeLayout(c);

        tree.MarkDirty(root);
        tree.MarkDirty(a);
        tree.MarkDirty(b);
        tree.MarkDirty(c);
        tree.ComputeRoot(root, 400, 300);
        var layoutA2 = tree.GetNodeLayout(a);
        var layoutB2 = tree.GetNodeLayout(b);
        var layoutC2 = tree.GetNodeLayout(c);

        Assert.Equal(layoutA1.Size, layoutA2.Size);
        Assert.Equal(layoutA1.Location, layoutA2.Location);
        Assert.Equal(layoutB1.Size, layoutB2.Size);
        Assert.Equal(layoutB1.Location, layoutB2.Location);
        Assert.Equal(layoutC1.Size, layoutC2.Size);
        Assert.Equal(layoutC1.Location, layoutC2.Location);
    }

    [Fact]
    public void NestedLayout_CacheReducesChildComputeCalls()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(LayoutStyle.Default);
        var mid = tree.AddNode(LayoutStyle.Default);
        var leaf = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(50), Length.Px(30)),
        });
        tree.AddChild(root, mid);
        tree.AddChild(mid, leaf);

        tree.ComputeRoot(root, 200, 200);
        int callsFirst = tree.ComputeCallCount;

        tree.ComputeRoot(root, 200, 200);
        int callsSecond = tree.ComputeCallCount;

        Assert.Equal(callsFirst, callsSecond);
    }
}
