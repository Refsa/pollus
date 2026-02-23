using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class RoundLayoutTests
{
    static (TestLayoutTree tree, int root, int container, int leaf) BuildCenteredLeaf(
        float containerWidth, float rootWidth)
    {
        var tree = new TestLayoutTree();

        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Percent(1f), Length.Percent(1f)),
        });

        var container = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(containerWidth), Length.Auto),
            FlexDirection = FlexDirection.Column,
            AlignItems = AlignItems.Center,
            AlignSelf = AlignSelf.Center,
        });
        tree.AddChild(root, container);

        var leaf = tree.AddNode(LayoutStyle.Default);
        tree.AddChild(container, leaf);

        return (tree, root, container, leaf);
    }

    [Fact]
    public void RoundSize_OddWidthAtHalfPixel_CanReduceBy1()
    {
        // Container 500px centered in 800px root -> container x = 150
        // Leaf 201px centered in 500px -> leaf offset = 149.5, absX = 299.5
        // Round(299.5 + 201) - Round(299.5) = 500 - 300 = 200
        var (tree, root, _, leaf) = BuildCenteredLeaf(500, 800);

        tree.SetMeasureFunc(leaf, input =>
        {
            float w = input.KnownDimensions.Width ?? 201f;
            return new LayoutOutput { Size = new Size<float>(w, 20) };
        });

        tree.ComputeRoot(root, 800, 600);
        Assert.Equal(201f, tree.GetNodeLayout(leaf).Size.Width);

        var self = tree;
        RoundLayout.Round(ref self, root);
        Assert.Equal(200f, tree.GetNodeLayout(leaf).Size.Width);
    }

    [Fact]
    public void RoundSize_EvenWidth_Preserved()
    {
        // Even widths centered in even containers produce integer offsets,
        // so rounding preserves the width exactly.
        var (tree, root, _, leaf) = BuildCenteredLeaf(500, 800);

        tree.SetMeasureFunc(leaf, input =>
        {
            float w = input.KnownDimensions.Width ?? 200f;
            return new LayoutOutput { Size = new Size<float>(w, 20) };
        });

        tree.ComputeRoot(root, 800, 600);

        var self = tree;
        RoundLayout.Round(ref self, root);
        Assert.Equal(200f, tree.GetNodeLayout(leaf).Size.Width);
    }
}
