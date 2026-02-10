using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class FlexPaddingBorderTests
{
    static LayoutStyle DefaultStyle => LayoutStyle.Default;

    [Fact]
    public void Padding_ShiftsChildren()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            Padding = Rect<LengthPercentage>.All(LengthPercentage.Px(10f)),
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(50f)),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(10f, childLayout.Location.X);
        Assert.Equal(10f, childLayout.Location.Y);
        Assert.Equal(50f, childLayout.Size.Width);
        Assert.Equal(50f, childLayout.Size.Height);
    }

    [Fact]
    public void Border_ShiftsChildren()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            Border = Rect<LengthPercentage>.All(LengthPercentage.Px(5f)),
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(50f)),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(5f, childLayout.Location.X);
        Assert.Equal(5f, childLayout.Location.Y);
        Assert.Equal(50f, childLayout.Size.Width);
        Assert.Equal(50f, childLayout.Size.Height);
    }

    [Fact]
    public void PaddingAndBorder_Combined()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            Padding = Rect<LengthPercentage>.All(LengthPercentage.Px(10f)),
            Border = Rect<LengthPercentage>.All(LengthPercentage.Px(5f)),
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Dimension>(Dimension.Px(50f), Dimension.Px(50f)),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(15f, childLayout.Location.X);
        Assert.Equal(15f, childLayout.Location.Y);
    }

    [Fact]
    public void BorderBox_ChildSizeIncludesPaddingBorder()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child = tree.AddNode(DefaultStyle with
        {
            BoxSizing = BoxSizing.BorderBox,
            Size = new Size<Dimension>(Dimension.Px(100f), Dimension.Px(100f)),
            Padding = Rect<LengthPercentage>.All(LengthPercentage.Px(10f)),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(100f, childLayout.Size.Width);
        Assert.Equal(100f, childLayout.Size.Height);
    }

    [Fact]
    public void ContentBox_ExplicitDimensionsPreserved()
    {
        // Regression: ContentBoxAdjustment returned (null,null) for ContentBox,
        // which caused MaybeAdd to erase explicit dimensions.
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child = tree.AddNode(DefaultStyle with
        {
            BoxSizing = BoxSizing.ContentBox,
            Size = new Size<Dimension>(Dimension.Px(80f), Dimension.Px(60f)),
            Padding = Rect<LengthPercentage>.All(LengthPercentage.Px(10f)),
            Border = Rect<LengthPercentage>.All(LengthPercentage.Px(2f)),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 300f, 300f);

        var childLayout = tree.GetNodeLayout(child);
        // ContentBox: outer = content + padding + border = 80+24=104, 60+24=84
        Assert.Equal(104f, childLayout.Size.Width);
        Assert.Equal(84f, childLayout.Size.Height);
    }
}
