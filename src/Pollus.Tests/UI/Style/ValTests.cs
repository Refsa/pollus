using Pollus.UI.Layout;

namespace Pollus.Tests.UI.Style;

public class ValTests
{
    // Length
    [Fact]
    public void Length_Px_Resolves()
    {
        var val = Length.Px(50f);
        Assert.Equal(50f, val.Resolve(1000f));
    }

    [Fact]
    public void Length_Percent_Resolves()
    {
        var val = Length.Percent(0.5f);
        Assert.Equal(500f, val.Resolve(1000f));
    }

    [Fact]
    public void Length_Zero()
    {
        var val = Length.Zero;
        Assert.Equal(0f, val.Resolve(1000f));
    }

    [Fact]
    public void Length_Auto_ResolvesNull()
    {
        var val = Length.Auto;
        Assert.Null(val.Resolve(1000f));
        Assert.True(val.IsAuto());
    }

    [Fact]
    public void Length_ResolveOr()
    {
        Assert.Equal(50f, Length.Px(50f).ResolveOr(1000f, 99f));
        Assert.Equal(99f, Length.Auto.ResolveOr(1000f, 99f));
    }

    [Fact]
    public void Length_Px_ResolveAgainstParent()
    {
        var val = Length.Px(100f);
        Assert.Equal(100f, val.Resolve(500f));
    }

    [Fact]
    public void Length_Percent_ResolveAgainstParent()
    {
        var val = Length.Percent(0.5f);
        Assert.Equal(250f, val.Resolve(500f));
    }

    [Fact]
    public void Length_Auto_ResolveOrFallback()
    {
        Assert.Equal(100f, Length.Px(100f).ResolveOr(500f, 0f));
        Assert.Equal(0f, Length.Auto.ResolveOr(500f, 0f));
    }
}
