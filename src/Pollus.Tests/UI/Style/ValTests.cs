using Pollus.UI.Layout;

namespace Pollus.Tests.UI.Style;

public class ValTests
{
    // LengthPercentage
    [Fact]
    public void LengthPercentage_Px_Resolves()
    {
        var val = LengthPercentage.Px(50f);
        Assert.Equal(50f, val.Resolve(1000f));
    }

    [Fact]
    public void LengthPercentage_Percent_Resolves()
    {
        var val = LengthPercentage.Percent(0.5f);
        Assert.Equal(500f, val.Resolve(1000f));
    }

    [Fact]
    public void LengthPercentage_Zero()
    {
        var val = LengthPercentage.Zero;
        Assert.Equal(0f, val.Resolve(1000f));
    }

    // LengthPercentageAuto
    [Fact]
    public void LengthPercentageAuto_Px_Resolves()
    {
        var val = LengthPercentageAuto.Px(50f);
        Assert.Equal(50f, val.Resolve(1000f));
    }

    [Fact]
    public void LengthPercentageAuto_Percent_Resolves()
    {
        var val = LengthPercentageAuto.Percent(0.25f);
        Assert.Equal(250f, val.Resolve(1000f));
    }

    [Fact]
    public void LengthPercentageAuto_Auto_ResolvesNull()
    {
        var val = LengthPercentageAuto.Auto;
        Assert.Null(val.Resolve(1000f));
        Assert.True(val.IsAuto());
    }

    [Fact]
    public void LengthPercentageAuto_ResolveOr()
    {
        Assert.Equal(50f, LengthPercentageAuto.Px(50f).ResolveOr(1000f, 99f));
        Assert.Equal(99f, LengthPercentageAuto.Auto.ResolveOr(1000f, 99f));
    }

    [Fact]
    public void LengthPercentageAuto_ImplicitConversion()
    {
        LengthPercentage lp = LengthPercentage.Px(42f);
        LengthPercentageAuto lpa = lp;
        Assert.Equal(42f, lpa.Resolve(0f));
    }

    // Dimension
    [Fact]
    public void Dimension_Px_Resolves()
    {
        var val = Dimension.Px(100f);
        Assert.Equal(100f, val.Resolve(500f));
    }

    [Fact]
    public void Dimension_Percent_Resolves()
    {
        var val = Dimension.Percent(0.5f);
        Assert.Equal(250f, val.Resolve(500f));
    }

    [Fact]
    public void Dimension_Auto_ResolvesNull()
    {
        var val = Dimension.Auto;
        Assert.Null(val.Resolve(500f));
        Assert.True(val.IsAuto());
    }

    [Fact]
    public void Dimension_ResolveOr()
    {
        Assert.Equal(100f, Dimension.Px(100f).ResolveOr(500f, 0f));
        Assert.Equal(0f, Dimension.Auto.ResolveOr(500f, 0f));
    }
}
