using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class FlexAbsoluteTests
{
    static LayoutStyle DefaultStyle => LayoutStyle.Default;

    static Size<Length> FixedSize(float w, float h) =>
        new(Length.Px(w), Length.Px(h));

    [Fact]
    public void Absolute_InsetLeftTop()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child = tree.AddNode(DefaultStyle with
        {
            Position = Position.Absolute,
            Size = FixedSize(50f, 50f),
            Inset = new Rect<Length>(
                Length.Px(10f),
                Length.Auto,
                Length.Px(20f),
                Length.Auto
            ),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        Assert.Equal(50f, layout.Size.Width);
        Assert.Equal(50f, layout.Size.Height);
        Assert.Equal(10f, layout.Location.X);
        Assert.Equal(20f, layout.Location.Y);
    }

    [Fact]
    public void Absolute_InsetRightBottom()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child = tree.AddNode(DefaultStyle with
        {
            Position = Position.Absolute,
            Size = FixedSize(50f, 50f),
            Inset = new Rect<Length>(
                Length.Auto,
                Length.Px(10f),
                Length.Auto,
                Length.Px(20f)
            ),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        Assert.Equal(50f, layout.Size.Width);
        Assert.Equal(50f, layout.Size.Height);
        Assert.Equal(140f, layout.Location.X);
        Assert.Equal(130f, layout.Location.Y);
    }

    [Fact]
    public void Absolute_StretchFromInset()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child = tree.AddNode(DefaultStyle with
        {
            Position = Position.Absolute,
            Inset = new Rect<Length>(
                Length.Px(10f),
                Length.Px(10f),
                Length.Auto,
                Length.Auto
            ),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        Assert.Equal(180f, layout.Size.Width);
        Assert.Equal(10f, layout.Location.X);
    }

    [Fact]
    public void Absolute_DoesNotAffectFlowChildren()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var absChild = tree.AddNode(DefaultStyle with
        {
            Position = Position.Absolute,
            Size = FixedSize(100f, 100f),
            Inset = new Rect<Length>(
                Length.Px(0f),
                Length.Auto,
                Length.Px(0f),
                Length.Auto
            ),
        });
        var flowChild = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(50f, 50f),
        });
        tree.AddChild(root, absChild);
        tree.AddChild(root, flowChild);
        tree.ComputeRoot(root, 200f, 200f);

        var flowLayout = tree.GetNodeLayout(flowChild);
        Assert.Equal(0f, flowLayout.Location.X);
        Assert.Equal(50f, flowLayout.Size.Width);
    }

    [Fact]
    public void Absolute_NoInset_JustifyCenter_CentersOnMainAxis()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            JustifyContent = JustifyContent.Center,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Position = Position.Absolute,
            Size = FixedSize(60f, 40f),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        Assert.Equal(60f, layout.Size.Width);
        Assert.Equal(40f, layout.Size.Height);
        // X centered: (200 - 60) / 2 = 70
        Assert.Equal(70f, layout.Location.X);
    }

    [Fact]
    public void Absolute_NoInset_AlignItemsCenter_CentersOnCrossAxis()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            AlignItems = AlignItems.Center,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Position = Position.Absolute,
            Size = FixedSize(60f, 40f),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        // Y centered: (200 - 40) / 2 = 80
        Assert.Equal(80f, layout.Location.Y);
    }

    [Fact]
    public void Absolute_NoInset_JustifyEnd_PositionsAtEnd()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            JustifyContent = JustifyContent.End,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Position = Position.Absolute,
            Size = FixedSize(60f, 40f),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        // X at end: 200 - 60 = 140
        Assert.Equal(140f, layout.Location.X);
    }

    [Fact]
    public void Absolute_NoInset_AlignSelfOverridesAlignItems()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            AlignItems = AlignItems.Start,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Position = Position.Absolute,
            Size = FixedSize(60f, 40f),
            AlignSelf = AlignSelf.End,
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        // Y at end: 200 - 40 = 160
        Assert.Equal(160f, layout.Location.Y);
    }

    [Fact]
    public void Absolute_Column_SizingUsesCorrectAxis()
    {
        // Regression: width/height available-space was swapped for absolute
        // children in column direction.
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            FlexDirection = FlexDirection.Column,
        });
        // Child has inset left+right to derive width from container
        var child = tree.AddNode(DefaultStyle with
        {
            Position = Position.Absolute,
            Size = new Size<Length>(Length.Auto, Length.Px(50f)),
            Inset = new Rect<Length>(
                Length.Px(10f),
                Length.Px(20f),
                Length.Auto,
                Length.Auto
            ),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 300f, 200f);

        var layout = tree.GetNodeLayout(child);
        // Width = 300 - 10 - 20 = 270
        Assert.Equal(270f, layout.Size.Width);
        Assert.Equal(50f, layout.Size.Height);
        Assert.Equal(10f, layout.Location.X);
    }

    [Fact]
    public void Absolute_Column_JustifyCenter_CentersOnYAxis()
    {
        // In column direction, main axis is Y - justify-content should affect Y
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            FlexDirection = FlexDirection.Column,
            JustifyContent = JustifyContent.Center,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Position = Position.Absolute,
            Size = FixedSize(60f, 40f),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        // Y centered: (200 - 40) / 2 = 80
        Assert.Equal(80f, layout.Location.Y);
        // X should default to start (0)
        Assert.Equal(0f, layout.Location.X);
    }
}
