using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class FlexAlignmentTests
{
    static LayoutStyle DefaultStyle => LayoutStyle.Default;

    static Size<Length> FixedSize(float w, float h) =>
        new(Length.Px(w), Length.Px(h));

    [Fact]
    public void JustifyContent_Center()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            JustifyContent = JustifyContent.Center,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(50f, 50f),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        Assert.Equal(50f, layout.Size.Width);
        Assert.Equal(75f, layout.Location.X);
    }

    [Fact]
    public void JustifyContent_FlexEnd()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            JustifyContent = JustifyContent.FlexEnd,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(50f, 50f),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        Assert.Equal(50f, layout.Size.Width);
        Assert.Equal(150f, layout.Location.X);
    }

    [Fact]
    public void JustifyContent_SpaceBetween()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            JustifyContent = JustifyContent.SpaceBetween,
        });
        var child1 = tree.AddNode(DefaultStyle with { Size = FixedSize(50f, 50f) });
        var child2 = tree.AddNode(DefaultStyle with { Size = FixedSize(50f, 50f) });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.ComputeRoot(root, 200f, 200f);

        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);
        Assert.Equal(0f, l1.Location.X);
        Assert.Equal(150f, l2.Location.X);
    }

    [Fact]
    public void JustifyContent_SpaceAround()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            JustifyContent = JustifyContent.SpaceAround,
        });
        var child1 = tree.AddNode(DefaultStyle with { Size = FixedSize(50f, 50f) });
        var child2 = tree.AddNode(DefaultStyle with { Size = FixedSize(50f, 50f) });
        tree.AddChild(root, child1);
        tree.AddChild(root, child2);
        tree.ComputeRoot(root, 200f, 200f);

        // 200 - 50 - 50 = 100 free space. SpaceAround: 100/2 = 50 per item, 25 on each side.
        // First child at X=25, second at X=125.
        var l1 = tree.GetNodeLayout(child1);
        var l2 = tree.GetNodeLayout(child2);
        Assert.Equal(25f, l1.Location.X);
        Assert.Equal(125f, l2.Location.X);
    }

    [Fact]
    public void AlignItems_Center()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            AlignItems = AlignItems.Center,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(50f, 50f),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        Assert.Equal(50f, layout.Size.Height);
        Assert.Equal(75f, layout.Location.Y);
    }

    [Fact]
    public void AlignItems_FlexEnd()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            AlignItems = AlignItems.FlexEnd,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(50f, 50f),
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        Assert.Equal(50f, layout.Size.Height);
        Assert.Equal(150f, layout.Location.Y);
    }

    [Fact]
    public void AlignSelf_Override()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            AlignItems = AlignItems.FlexStart,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(50f, 50f),
            AlignSelf = AlignSelf.FlexEnd,
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        Assert.Equal(50f, layout.Size.Height);
        Assert.Equal(150f, layout.Location.Y);
    }

    [Fact]
    public void AlignItems_Center_WithPaddingAndMeasuredChild()
    {
        // Simulates the UI header: row, height 60, padding 16, AlignItems.Center
        // with a text child that measures at ~32px height (line-height of 20px font)
        var tree = new TestLayoutTree();
        var header = tree.AddNode(DefaultStyle with
        {
            FlexDirection = FlexDirection.Row,
            Size = new Size<Length>(Length.Auto, Length.Px(60)),
            Padding = Rect<Length>.All(Length.Px(16)),
            AlignItems = AlignItems.Center,
            JustifyContent = JustifyContent.SpaceBetween,
            BoxSizing = BoxSizing.BorderBox,
        });

        // Text child measured at 120x32 (simulating 20px font line height)
        var text1 = tree.AddNode(DefaultStyle);
        tree.SetMeasureFunc(text1, input =>
        {
            float w = input.KnownDimensions.Width ?? 120f;
            float h = input.KnownDimensions.Height ?? 32f;
            return new LayoutOutput { Size = new Size<float>(w, h) };
        });

        // Second text child measured at 200x19 (simulating 12px font)
        var text2 = tree.AddNode(DefaultStyle);
        tree.SetMeasureFunc(text2, input =>
        {
            float w = input.KnownDimensions.Width ?? 200f;
            float h = input.KnownDimensions.Height ?? 19f;
            return new LayoutOutput { Size = new Size<float>(w, h) };
        });

        tree.AddChild(header, text1);
        tree.AddChild(header, text2);

        // Root wrapping the header
        var root = tree.AddNode(DefaultStyle with
        {
            FlexDirection = FlexDirection.Column,
            Size = new Size<Length>(Length.Px(800), Length.Px(600)),
        });
        tree.AddChild(root, header);
        tree.ComputeRoot(root, 800f, 600f);

        var headerLayout = tree.GetNodeLayout(header);
        var text1Layout = tree.GetNodeLayout(text1);
        var text2Layout = tree.GetNodeLayout(text2);

        // Header: border-box height 60, padding 16 each side => content area = 28
        Assert.Equal(60f, headerLayout.Size.Height);

        // Text1 (height 32): center = Y + height/2 should equal header center (30)
        // offset = (28 - 32) / 2 = -2, Y = padding(16) + (-2) = 14
        Assert.Equal(14f, text1Layout.Location.Y);
        Assert.Equal(32f, text1Layout.Size.Height);
        Assert.Equal(30f, text1Layout.Location.Y + text1Layout.Size.Height / 2f); // centered

        // Text2 (height 19): center = Y + height/2 should equal header center (30)
        // offset = (28 - 19) / 2 = 4.5, Y = padding(16) + 4.5 = 20.5
        Assert.Equal(20.5f, text2Layout.Location.Y);
        Assert.Equal(19f, text2Layout.Size.Height);
        Assert.Equal(30f, text2Layout.Location.Y + text2Layout.Size.Height / 2f); // centered
    }

    [Fact]
    public void AlignItems_Center_ButtonWithMeasuredText()
    {
        // Simulates: Button(height=34, padding 6/10/6/10, AlignItems.Center, BoxSizing.BorderBox)
        // with a text child measured at 80x18 (simulating 12px font natural line height)
        var tree = new TestLayoutTree();

        var button = tree.AddNode(DefaultStyle with
        {
            FlexDirection = FlexDirection.Row,
            Size = new Size<Length>(Length.Px(200), Length.Px(34)),
            // Rect<Length> ctor is (left, right, top, bottom), NOT CSS order
            Padding = new Rect<Length>(Length.Px(10), Length.Px(10), Length.Px(6), Length.Px(6)),
            AlignItems = AlignItems.Center,
            BoxSizing = BoxSizing.BorderBox,
        });

        var text = tree.AddNode(DefaultStyle);
        tree.SetMeasureFunc(text, input =>
        {
            float w = input.KnownDimensions.Width ?? 80f;
            float h = input.KnownDimensions.Height ?? 18f;
            return new LayoutOutput { Size = new Size<float>(w, h) };
        });

        tree.AddChild(button, text);

        var root = tree.AddNode(DefaultStyle with
        {
            FlexDirection = FlexDirection.Column,
            Size = new Size<Length>(Length.Px(400), Length.Px(400)),
        });
        tree.AddChild(root, button);
        tree.ComputeRoot(root, 400f, 400f);

        var buttonLayout = tree.GetNodeLayout(button);
        var textLayout = tree.GetNodeLayout(text);

        // Button: border-box height 34, padding 6 top + 6 bottom => content area = 22
        Assert.Equal(34f, buttonLayout.Size.Height);

        // Text (height 18): center should be at button center (17)
        // offset = (22 - 18) / 2 = 2, Y = padding(6) + 2 = 8
        Assert.Equal(18f, textLayout.Size.Height);
        Assert.Equal(8f, textLayout.Location.Y);
        Assert.Equal(17f, textLayout.Location.Y + textLayout.Size.Height / 2f); // centered at button midpoint
    }

    [Fact]
    public void AlignItems_Center_WithLineHeight1_GivesBetterCentering()
    {
        // Same header setup but text uses line-height: 1.0 (height = fontSize)
        // instead of font metrics (height = ascent+descent ≈ 1.48 * fontSize).
        // With lineHeight=1.0, measured height = 20px for 20px font,
        // giving 8px free space in the 28px content area.
        var tree = new TestLayoutTree();
        var header = tree.AddNode(DefaultStyle with
        {
            FlexDirection = FlexDirection.Row,
            Size = new Size<Length>(Length.Auto, Length.Px(60)),
            Padding = Rect<Length>.All(Length.Px(16)),
            AlignItems = AlignItems.Center,
            JustifyContent = JustifyContent.SpaceBetween,
            BoxSizing = BoxSizing.BorderBox,
        });

        // Text child with lineHeight=1.0: measured height = fontSize = 20
        var text1 = tree.AddNode(DefaultStyle);
        tree.SetMeasureFunc(text1, input =>
        {
            float w = input.KnownDimensions.Width ?? 120f;
            float h = input.KnownDimensions.Height ?? 20f; // fontSize * 1.0
            return new LayoutOutput { Size = new Size<float>(w, h) };
        });

        // Second text child with lineHeight=1.0: measured height = 12
        var text2 = tree.AddNode(DefaultStyle);
        tree.SetMeasureFunc(text2, input =>
        {
            float w = input.KnownDimensions.Width ?? 200f;
            float h = input.KnownDimensions.Height ?? 12f; // fontSize * 1.0
            return new LayoutOutput { Size = new Size<float>(w, h) };
        });

        tree.AddChild(header, text1);
        tree.AddChild(header, text2);

        var root = tree.AddNode(DefaultStyle with
        {
            FlexDirection = FlexDirection.Column,
            Size = new Size<Length>(Length.Px(800), Length.Px(600)),
        });
        tree.AddChild(root, header);
        tree.ComputeRoot(root, 800f, 600f);

        var text1Layout = tree.GetNodeLayout(text1);
        var text2Layout = tree.GetNodeLayout(text2);

        // Text1 (height 20): offset = (28 - 20) / 2 = 4, Y = 16 + 4 = 20
        Assert.Equal(20f, text1Layout.Location.Y);
        Assert.Equal(20f, text1Layout.Size.Height);
        Assert.Equal(30f, text1Layout.Location.Y + text1Layout.Size.Height / 2f);

        // Text2 (height 12): offset = (28 - 12) / 2 = 8, Y = 16 + 8 = 24
        Assert.Equal(24f, text2Layout.Location.Y);
        Assert.Equal(12f, text2Layout.Size.Height);
        Assert.Equal(30f, text2Layout.Location.Y + text2Layout.Size.Height / 2f);
    }

    [Fact]
    public void JustifySelf_Center_Absolute()
    {
        // Absolute child with JustifySelf.Center should center on main axis
        // regardless of parent's JustifyContent
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(200f, 200f),
            JustifyContent = JustifyContent.FlexStart,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(50f, 50f),
            Position = Position.Absolute,
            JustifySelf = JustifySelf.Center,
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        // 200 - 50 = 150 free space, centered = 75
        Assert.Equal(75f, layout.Location.X);
    }

    [Fact]
    public void JustifySelf_FlexEnd_Absolute()
    {
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(200f, 200f),
            JustifyContent = JustifyContent.FlexStart,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(50f, 50f),
            Position = Position.Absolute,
            JustifySelf = JustifySelf.FlexEnd,
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        // 200 - 50 = 150 free space, flex-end = 150
        Assert.Equal(150f, layout.Location.X);
    }

    [Fact]
    public void JustifySelf_Center_Absolute_Column()
    {
        // In column direction, main axis is vertical, so JustifySelf affects Y
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            FlexDirection = FlexDirection.Column,
            Size = FixedSize(200f, 200f),
            JustifyContent = JustifyContent.FlexStart,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(50f, 50f),
            Position = Position.Absolute,
            JustifySelf = JustifySelf.Center,
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        // Column: main axis = Y. 200 - 50 = 150 free, centered = 75
        Assert.Equal(75f, layout.Location.Y);
    }

    [Fact]
    public void JustifySelf_Null_FallsBackToJustifyContent()
    {
        // When JustifySelf is null, should fall back to parent's JustifyContent
        var tree = new TestLayoutTree();
        var root = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(200f, 200f),
            JustifyContent = JustifyContent.Center,
        });
        var child = tree.AddNode(DefaultStyle with
        {
            Size = FixedSize(50f, 50f),
            Position = Position.Absolute,
            // JustifySelf not set - should use parent's JustifyContent.Center
        });
        tree.AddChild(root, child);
        tree.ComputeRoot(root, 200f, 200f);

        var layout = tree.GetNodeLayout(child);
        Assert.Equal(75f, layout.Location.X);
    }

}
