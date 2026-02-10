using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Style;

public class StyleDefaultTests
{
    [Fact]
    public void Default_Display_IsFlex()
    {
        Assert.Equal(Display.Flex, LayoutStyle.Default.Display);
    }

    [Fact]
    public void Default_Position_IsRelative()
    {
        Assert.Equal(Position.Relative, LayoutStyle.Default.Position);
    }

    [Fact]
    public void Default_BoxSizing_IsBorderBox()
    {
        Assert.Equal(BoxSizing.BorderBox, LayoutStyle.Default.BoxSizing);
    }

    [Fact]
    public void Default_FlexDirection_IsRow()
    {
        Assert.Equal(FlexDirection.Row, LayoutStyle.Default.FlexDirection);
    }

    [Fact]
    public void Default_FlexWrap_IsNoWrap()
    {
        Assert.Equal(FlexWrap.NoWrap, LayoutStyle.Default.FlexWrap);
    }

    [Fact]
    public void Default_FlexGrow_IsZero()
    {
        Assert.Equal(0f, LayoutStyle.Default.FlexGrow);
    }

    [Fact]
    public void Default_FlexShrink_IsOne()
    {
        Assert.Equal(1f, LayoutStyle.Default.FlexShrink);
    }

    [Fact]
    public void Default_FlexBasis_IsAuto()
    {
        Assert.True(LayoutStyle.Default.FlexBasis.IsAuto());
    }

    [Fact]
    public void Default_Size_IsAuto()
    {
        Assert.True(LayoutStyle.Default.Size.Width.IsAuto());
        Assert.True(LayoutStyle.Default.Size.Height.IsAuto());
    }

    [Fact]
    public void Default_Alignment_IsNull()
    {
        var style = LayoutStyle.Default;
        Assert.Null(style.AlignItems);
        Assert.Null(style.AlignSelf);
        Assert.Null(style.AlignContent);
        Assert.Null(style.JustifyContent);
    }

    [Fact]
    public void Default_Margin_IsZero()
    {
        var m = LayoutStyle.Default.Margin;
        Assert.False(m.Left.IsAuto());
        Assert.Equal(0f, m.Left.ResolveOr(100f, -1f));
    }

    [Fact]
    public void Default_Inset_IsAuto()
    {
        var inset = LayoutStyle.Default.Inset;
        Assert.True(inset.Left.IsAuto());
        Assert.True(inset.Right.IsAuto());
        Assert.True(inset.Top.IsAuto());
        Assert.True(inset.Bottom.IsAuto());
    }

    [Fact]
    public void Default_Order_IsZero()
    {
        Assert.Equal(0, LayoutStyle.Default.Order);
    }

    [Fact]
    public void Default_AspectRatio_IsNull()
    {
        Assert.Null(LayoutStyle.Default.AspectRatio);
    }

    [Fact]
    public void Default_Overflow_IsVisible()
    {
        var style = LayoutStyle.Default;
        Assert.Equal(Overflow.Visible, style.Overflow.X);
        Assert.Equal(Overflow.Visible, style.Overflow.Y);
    }
}
