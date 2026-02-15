using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

/// Reproduces the checkbox group text wrapping issue.
/// Layout hierarchy mirrors UICheckBoxGroupBuilder output:
///   Root (800x600, column)
///     └─ Container (auto, column) ← checkbox group
///         └─ Row (auto, row, gap=4, align-items=center)
///             ├─ Checkbox (18x18, min=18x18)
///             └─ Text (auto, measure=120x20, flex-shrink=0)
public class CheckBoxGroupLayoutTests
{
    [Fact]
    public void TextLabel_GetsFullMeasuredWidth()
    {
        var tree = new TestLayoutTree();

        // Root layout container (like a panel the user places the checkbox group in)
        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(130), Length.Px(400)),
            FlexDirection = FlexDirection.Column,
        });

        // Checkbox group container (auto width, column)
        var container = tree.AddNode(LayoutStyle.Default with
        {
            FlexDirection = FlexDirection.Column,
        });
        tree.AddChild(root, container);

        // Row (flex row, gap 4, align-items center)
        var row = tree.AddNode(LayoutStyle.Default with
        {
            FlexDirection = FlexDirection.Row,
            AlignItems = AlignItems.Center,
            Gap = new Size<Length>(Length.Px(4), Length.Px(4)),
        });
        tree.AddChild(container, row);

        // Checkbox (18x18 fixed, min 18x18)
        var checkbox = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(18), Length.Px(18)),
            MinSize = new Size<Length>(Length.Px(18), Length.Px(18)),
        });
        tree.AddChild(row, checkbox);

        // Text leaf (auto, flex-shrink=0, measures as 120x20)
        var text = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Auto, Length.Auto),
            FlexShrink = 0f,
        });
        tree.AddChild(row, text);

        // Simulates the real UITextPlugin MeasureFunc:
        // maxWidth = knownDimensions.Width ?? availableSpace.Width.AsDefinite() ?? float.MaxValue
        tree.SetMeasureFunc(text, input =>
        {
            float maxWidth = input.KnownDimensions.Width
                ?? input.AvailableSpace.Width.AsDefinite()
                ?? float.MaxValue;
            float textWidth = 120f;
            float textHeight = 20f;
            if (maxWidth < textWidth)
            {
                float lines = MathF.Ceiling(textWidth / maxWidth);
                textWidth = maxWidth;
                textHeight = lines * 20f;
            }
            return new LayoutOutput
            {
                Size = new Size<float>(
                    input.KnownDimensions.Width ?? textWidth,
                    input.KnownDimensions.Height ?? textHeight),
            };
        });

        tree.ComputeRoot(root, 130, 400);

        var textLayout = tree.GetNodeLayout(text);
        var rowLayout = tree.GetNodeLayout(row);
        var containerLayout = tree.GetNodeLayout(container);

        // Log for debugging
        foreach (var log in tree.SetLog)
            Console.WriteLine(log);

        // Text should get its full measured width (120), not be shrunk
        Assert.True(textLayout.Size.Width >= 120f,
            $"Text width was {textLayout.Size.Width}, expected >= 120. " +
            $"Row width: {rowLayout.Size.Width}, Container width: {containerLayout.Size.Width}");

        // Text height should be single line (20), not wrapped
        Assert.Equal(20f, textLayout.Size.Height);
    }

    [Fact]
    public void TextLabel_WithoutFlexShrinkZero_GetsShrunk()
    {
        var tree = new TestLayoutTree();

        var root = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(130), Length.Px(400)),
            FlexDirection = FlexDirection.Column,
        });

        var container = tree.AddNode(LayoutStyle.Default with
        {
            FlexDirection = FlexDirection.Column,
        });
        tree.AddChild(root, container);

        var row = tree.AddNode(LayoutStyle.Default with
        {
            FlexDirection = FlexDirection.Row,
            AlignItems = AlignItems.Center,
            Gap = new Size<Length>(Length.Px(4), Length.Px(4)),
        });
        tree.AddChild(container, row);

        var checkbox = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Px(18), Length.Px(18)),
            MinSize = new Size<Length>(Length.Px(18), Length.Px(18)),
        });
        tree.AddChild(row, checkbox);

        // Text WITHOUT FlexShrink=0 (default FlexShrink=1)
        var text = tree.AddNode(LayoutStyle.Default with
        {
            Size = new Size<Length>(Length.Auto, Length.Auto),
        });
        tree.AddChild(row, text);

        tree.SetMeasureFunc(text, input =>
        {
            float maxWidth = input.KnownDimensions.Width
                ?? input.AvailableSpace.Width.AsDefinite()
                ?? float.MaxValue;
            float textWidth = 120f;
            float textHeight = 20f;
            if (maxWidth < textWidth)
            {
                float lines = MathF.Ceiling(textWidth / maxWidth);
                textWidth = maxWidth;
                textHeight = lines * 20f;
            }
            return new LayoutOutput
            {
                Size = new Size<float>(
                    input.KnownDimensions.Width ?? textWidth,
                    input.KnownDimensions.Height ?? textHeight),
            };
        });

        tree.ComputeRoot(root, 130, 400);

        var textLayout = tree.GetNodeLayout(text);

        // Without FlexShrink=0, text SHOULD get shrunk below 120
        Assert.True(textLayout.Size.Width < 120f,
            $"Expected text to be shrunk but got width {textLayout.Size.Width}");
    }
}
