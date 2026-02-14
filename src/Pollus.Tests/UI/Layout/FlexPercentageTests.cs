using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class FlexPercentageTests
{
    static LayoutStyle DefaultStyle => LayoutStyle.Default;

    [Fact]
    public void Percent_Width()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Length>(Length.Percent(0.5f), Length.Auto),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(100f, childLayout.Size.Width, 0.01f);
    }

    [Fact]
    public void Percent_Height()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            FlexDirection = FlexDirection.Column,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Length>(Length.Auto, Length.Percent(0.25f)),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(50f, childLayout.Size.Height, 0.01f);
    }

    [Fact]
    public void Percent_Padding()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            Padding = new Rect<Length>(
                Length.Percent(0.1f), Length.Zero,
                Length.Zero, Length.Zero
            ),
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Length>(Length.Px(50f), Length.Px(50f)),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(20f, childLayout.Location.X, 0.01f);
    }

    [Fact]
    public void Percent_Margin()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle);
        var child = tree.AddNode(DefaultStyle with
        {
            Size = new Size<Length>(Length.Px(50f), Length.Px(50f)),
            Margin = new Rect<Length>(
                Length.Percent(0.1f), Length.Zero,
                Length.Zero, Length.Zero
            ),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var childLayout = tree.GetNodeLayout(child);
        Assert.Equal(20f, childLayout.Location.X, 0.01f);
    }
}
