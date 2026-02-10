using Pollus.UI.Layout;

namespace Pollus.Tests.UI.Geometry;

public class RectTests
{
    [Fact]
    public void HorizontalAxisSum()
    {
        var rect = new Rect<float>(10f, 20f, 5f, 15f);
        Assert.Equal(30f, rect.HorizontalAxisSum());
    }

    [Fact]
    public void VerticalAxisSum()
    {
        var rect = new Rect<float>(10f, 20f, 5f, 15f);
        Assert.Equal(20f, rect.VerticalAxisSum());
    }

    [Fact]
    public void SumAxes()
    {
        var rect = new Rect<float>(10f, 20f, 5f, 15f);
        var sum = rect.SumAxes();
        Assert.Equal(30f, sum.Width);
        Assert.Equal(20f, sum.Height);
    }

    [Fact]
    public void MainAxisSum_Row()
    {
        var rect = new Rect<float>(10f, 20f, 5f, 15f);
        Assert.Equal(30f, rect.MainAxisSum(FlexDirection.Row));
    }

    [Fact]
    public void MainAxisSum_Column()
    {
        var rect = new Rect<float>(10f, 20f, 5f, 15f);
        Assert.Equal(20f, rect.MainAxisSum(FlexDirection.Column));
    }

    [Fact]
    public void CrossAxisSum_Row()
    {
        var rect = new Rect<float>(10f, 20f, 5f, 15f);
        Assert.Equal(20f, rect.CrossAxisSum(FlexDirection.Row));
    }

    [Fact]
    public void CrossAxisSum_Column()
    {
        var rect = new Rect<float>(10f, 20f, 5f, 15f);
        Assert.Equal(30f, rect.CrossAxisSum(FlexDirection.Column));
    }

    [Fact]
    public void MainStart_Row()
    {
        var rect = new Rect<float>(10f, 20f, 5f, 15f);
        Assert.Equal(10f, rect.MainStart(FlexDirection.Row));
    }

    [Fact]
    public void MainStart_RowReverse()
    {
        var rect = new Rect<float>(10f, 20f, 5f, 15f);
        Assert.Equal(20f, rect.MainStart(FlexDirection.RowReverse));
    }

    [Fact]
    public void MainStart_Column()
    {
        var rect = new Rect<float>(10f, 20f, 5f, 15f);
        Assert.Equal(5f, rect.MainStart(FlexDirection.Column));
    }

    [Fact]
    public void MainStart_ColumnReverse()
    {
        var rect = new Rect<float>(10f, 20f, 5f, 15f);
        Assert.Equal(15f, rect.MainStart(FlexDirection.ColumnReverse));
    }

    [Fact]
    public void MainEnd_Row()
    {
        var rect = new Rect<float>(10f, 20f, 5f, 15f);
        Assert.Equal(20f, rect.MainEnd(FlexDirection.Row));
    }

    [Fact]
    public void MainEnd_RowReverse()
    {
        var rect = new Rect<float>(10f, 20f, 5f, 15f);
        Assert.Equal(10f, rect.MainEnd(FlexDirection.RowReverse));
    }

    [Fact]
    public void CrossStart_Row()
    {
        var rect = new Rect<float>(10f, 20f, 5f, 15f);
        Assert.Equal(5f, rect.CrossStart(FlexDirection.Row));
    }

    [Fact]
    public void CrossStart_Column()
    {
        var rect = new Rect<float>(10f, 20f, 5f, 15f);
        Assert.Equal(10f, rect.CrossStart(FlexDirection.Column));
    }

    [Fact]
    public void CrossEnd_Row()
    {
        var rect = new Rect<float>(10f, 20f, 5f, 15f);
        Assert.Equal(15f, rect.CrossEnd(FlexDirection.Row));
    }

    [Fact]
    public void CrossEnd_Column()
    {
        var rect = new Rect<float>(10f, 20f, 5f, 15f);
        Assert.Equal(20f, rect.CrossEnd(FlexDirection.Column));
    }

    [Fact]
    public void All_SetsAllEdges()
    {
        var rect = Rect<float>.All(5f);
        Assert.Equal(5f, rect.Left);
        Assert.Equal(5f, rect.Right);
        Assert.Equal(5f, rect.Top);
        Assert.Equal(5f, rect.Bottom);
    }

    [Fact]
    public void Zero_IsDefault()
    {
        var zero = Rect<float>.Zero;
        Assert.Equal(0f, zero.Left);
        Assert.Equal(0f, zero.Right);
        Assert.Equal(0f, zero.Top);
        Assert.Equal(0f, zero.Bottom);
    }
}
