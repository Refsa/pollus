using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

public class LayoutHelpersTests
{
    [Fact]
    public void ResolvePadding_Px()
    {
        var style = LayoutStyle.Default;
        style.Padding = new Rect<Length>(
            Length.Px(10f),
            Length.Px(20f),
            Length.Px(5f),
            Length.Px(15f)
        );
        var result = LayoutHelpers.ResolvePadding(style, new Size<float?>(100f, 100f));
        Assert.Equal(10f, result.Left);
        Assert.Equal(20f, result.Right);
        Assert.Equal(5f, result.Top);
        Assert.Equal(15f, result.Bottom);
    }

    [Fact]
    public void ResolvePadding_Percent_UsesParentWidth()
    {
        var style = LayoutStyle.Default;
        style.Padding = new Rect<Length>(
            Length.Percent(0.1f),
            Length.Percent(0.2f),
            Length.Percent(0.05f),
            Length.Percent(0.15f)
        );
        var result = LayoutHelpers.ResolvePadding(style, new Size<float?>(200f, 100f));
        Assert.Equal(20f, result.Left, 0.001f);
        Assert.Equal(40f, result.Right, 0.001f);
        Assert.Equal(10f, result.Top, 0.001f);
        Assert.Equal(30f, result.Bottom, 0.001f);
    }

    [Fact]
    public void ResolveBorder_Px()
    {
        var style = LayoutStyle.Default;
        style.Border = Rect<Length>.All(Length.Px(2f));
        var result = LayoutHelpers.ResolveBorder(style, new Size<float?>(100f, 100f));
        Assert.Equal(2f, result.Left);
        Assert.Equal(2f, result.Right);
        Assert.Equal(2f, result.Top);
        Assert.Equal(2f, result.Bottom);
    }

    [Fact]
    public void ResolveMargin_Px()
    {
        var style = LayoutStyle.Default;
        style.Margin = new Rect<LengthAuto>(
            LengthAuto.Px(5f),
            LengthAuto.Px(10f),
            LengthAuto.Px(15f),
            LengthAuto.Px(20f)
        );
        var result = LayoutHelpers.ResolveMargin(style, new Size<float?>(100f, 100f));
        Assert.Equal(5f, result.Left);
        Assert.Equal(10f, result.Right);
        Assert.Equal(15f, result.Top);
        Assert.Equal(20f, result.Bottom);
    }

    [Fact]
    public void ResolveMargin_Auto_ResolvesToZero()
    {
        var style = LayoutStyle.Default;
        style.Margin = Rect<LengthAuto>.All(LengthAuto.Auto);
        var result = LayoutHelpers.ResolveMargin(style, new Size<float?>(100f, 100f));
        Assert.Equal(0f, result.Left);
        Assert.Equal(0f, result.Right);
        Assert.Equal(0f, result.Top);
        Assert.Equal(0f, result.Bottom);
    }

    [Fact]
    public void ResolveInset_Px()
    {
        var style = LayoutStyle.Default;
        style.Inset = new Rect<LengthAuto>(
            LengthAuto.Px(10f),
            LengthAuto.Px(20f),
            LengthAuto.Px(30f),
            LengthAuto.Px(40f)
        );
        var result = LayoutHelpers.ResolveInset(style, new Size<float?>(100f, 200f));
        Assert.Equal(10f, result.Left);
        Assert.Equal(20f, result.Right);
        Assert.Equal(30f, result.Top);
        Assert.Equal(40f, result.Bottom);
    }

    [Fact]
    public void ResolveInset_Auto_IsNull()
    {
        var style = LayoutStyle.Default;
        // Default inset is all Auto
        var result = LayoutHelpers.ResolveInset(style, new Size<float?>(100f, 100f));
        Assert.Null(result.Left);
        Assert.Null(result.Right);
        Assert.Null(result.Top);
        Assert.Null(result.Bottom);
    }

    [Fact]
    public void MaybeClamp_WithinRange()
    {
        Assert.Equal(5f, LayoutHelpers.MaybeClamp(5f, 0f, 10f));
    }

    [Fact]
    public void MaybeClamp_BelowMin()
    {
        Assert.Equal(0f, LayoutHelpers.MaybeClamp(-5f, 0f, 10f));
    }

    [Fact]
    public void MaybeClamp_AboveMax()
    {
        Assert.Equal(10f, LayoutHelpers.MaybeClamp(15f, 0f, 10f));
    }

    [Fact]
    public void MaybeClamp_NullValue()
    {
        Assert.Null(LayoutHelpers.MaybeClamp(null, 0f, 10f));
    }

    [Fact]
    public void MaybeClamp_NullMinMax()
    {
        Assert.Equal(5f, LayoutHelpers.MaybeClamp(5f, null, null));
    }

    [Fact]
    public void MaybeMax_BothValues()
    {
        Assert.Equal(10f, LayoutHelpers.MaybeMax(5f, 10f));
    }

    [Fact]
    public void MaybeMax_OneNull()
    {
        Assert.Equal(5f, LayoutHelpers.MaybeMax(5f, null));
        Assert.Equal(5f, LayoutHelpers.MaybeMax(null, 5f));
    }

    [Fact]
    public void MaybeMax_BothNull()
    {
        Assert.Null(LayoutHelpers.MaybeMax(null, null));
    }

    [Fact]
    public void MaybeMin_BothValues()
    {
        Assert.Equal(5f, LayoutHelpers.MaybeMin(5f, 10f));
    }

    [Fact]
    public void MaybeMin_OneNull()
    {
        Assert.Equal(5f, LayoutHelpers.MaybeMin(5f, null));
        Assert.Equal(5f, LayoutHelpers.MaybeMin(null, 5f));
    }

    [Fact]
    public void ContentBoxAdjustment_BorderBox()
    {
        var padding = new Rect<float>(10f, 10f, 5f, 5f);
        var border = new Rect<float>(2f, 2f, 2f, 2f);
        var adj = LayoutHelpers.ContentBoxAdjustment(BoxSizing.BorderBox, padding, border);
        Assert.Equal(-24f, adj.Width);
        Assert.Equal(-14f, adj.Height);
    }

    [Fact]
    public void ContentBoxAdjustment_ContentBox_IsZero()
    {
        var padding = new Rect<float>(10f, 10f, 5f, 5f);
        var border = new Rect<float>(2f, 2f, 2f, 2f);
        var adj = LayoutHelpers.ContentBoxAdjustment(BoxSizing.ContentBox, padding, border);
        Assert.Equal(0f, adj.Width);
        Assert.Equal(0f, adj.Height);
    }
}
