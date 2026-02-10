using Pollus.UI.Layout;

namespace Pollus.Tests.UI.Geometry;

public class SizeTests
{
    [Fact]
    public void Main_Row_ReturnsWidth()
    {
        var size = new Size<float>(100f, 200f);
        Assert.Equal(100f, size.Main(FlexDirection.Row));
        Assert.Equal(100f, size.Main(FlexDirection.RowReverse));
    }

    [Fact]
    public void Main_Column_ReturnsHeight()
    {
        var size = new Size<float>(100f, 200f);
        Assert.Equal(200f, size.Main(FlexDirection.Column));
        Assert.Equal(200f, size.Main(FlexDirection.ColumnReverse));
    }

    [Fact]
    public void Cross_Row_ReturnsHeight()
    {
        var size = new Size<float>(100f, 200f);
        Assert.Equal(200f, size.Cross(FlexDirection.Row));
    }

    [Fact]
    public void Cross_Column_ReturnsWidth()
    {
        var size = new Size<float>(100f, 200f);
        Assert.Equal(100f, size.Cross(FlexDirection.Column));
    }

    [Fact]
    public void WithMain_Row_SetsWidth()
    {
        var size = new Size<float>(100f, 200f);
        var updated = size.WithMain(FlexDirection.Row, 50f);
        Assert.Equal(50f, updated.Width);
        Assert.Equal(200f, updated.Height);
    }

    [Fact]
    public void WithMain_Column_SetsHeight()
    {
        var size = new Size<float>(100f, 200f);
        var updated = size.WithMain(FlexDirection.Column, 50f);
        Assert.Equal(100f, updated.Width);
        Assert.Equal(50f, updated.Height);
    }

    [Fact]
    public void WithCross_Row_SetsHeight()
    {
        var size = new Size<float>(100f, 200f);
        var updated = size.WithCross(FlexDirection.Row, 50f);
        Assert.Equal(100f, updated.Width);
        Assert.Equal(50f, updated.Height);
    }

    [Fact]
    public void WithCross_Column_SetsWidth()
    {
        var size = new Size<float>(100f, 200f);
        var updated = size.WithCross(FlexDirection.Column, 50f);
        Assert.Equal(50f, updated.Width);
        Assert.Equal(200f, updated.Height);
    }

    [Fact]
    public void FromMainCross_Row()
    {
        var size = Size<float>.FromMainCross(FlexDirection.Row, 100f, 200f);
        Assert.Equal(100f, size.Width);
        Assert.Equal(200f, size.Height);
    }

    [Fact]
    public void FromMainCross_Column()
    {
        var size = Size<float>.FromMainCross(FlexDirection.Column, 100f, 200f);
        Assert.Equal(200f, size.Width);
        Assert.Equal(100f, size.Height);
    }

    [Fact]
    public void Zero_IsDefault()
    {
        var zero = Size<float>.Zero;
        Assert.Equal(0f, zero.Width);
        Assert.Equal(0f, zero.Height);
    }

    [Fact]
    public void NullableFloat_MainCross()
    {
        var size = new Size<float?>(100f, null);
        Assert.Equal(100f, size.Main(FlexDirection.Row));
        Assert.Null(size.Cross(FlexDirection.Row));
        Assert.Null(size.Main(FlexDirection.Column));
        Assert.Equal(100f, size.Cross(FlexDirection.Column));
    }
}
