namespace Pollus.UI.Layout;

using System.Runtime.CompilerServices;

public record struct LayoutRect<T>
{
    public T Left;
    public T Right;
    public T Top;
    public T Bottom;

    public LayoutRect(T left, T right, T top, T bottom)
    {
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }

    public static LayoutRect<T> All(T value) => new(value, value, value, value);
    public static readonly LayoutRect<T> Zero = default;
}

public static class LayoutRectFloatExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float HorizontalAxisSum(this LayoutRect<float> self) => self.Left + self.Right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float VerticalAxisSum(this LayoutRect<float> self) => self.Top + self.Bottom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float> SumAxes(this LayoutRect<float> self) =>
        new(self.Left + self.Right, self.Top + self.Bottom);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MainAxisSum(this LayoutRect<float> self, FlexDirection direction) =>
        direction.IsRow() ? self.HorizontalAxisSum() : self.VerticalAxisSum();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CrossAxisSum(this LayoutRect<float> self, FlexDirection direction) =>
        direction.IsRow() ? self.VerticalAxisSum() : self.HorizontalAxisSum();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MainStart(this LayoutRect<float> self, FlexDirection direction) =>
        direction switch
        {
            FlexDirection.Row => self.Left,
            FlexDirection.RowReverse => self.Right,
            FlexDirection.Column => self.Top,
            FlexDirection.ColumnReverse => self.Bottom,
            _ => self.Left,
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MainEnd(this LayoutRect<float> self, FlexDirection direction) =>
        direction switch
        {
            FlexDirection.Row => self.Right,
            FlexDirection.RowReverse => self.Left,
            FlexDirection.Column => self.Bottom,
            FlexDirection.ColumnReverse => self.Top,
            _ => self.Right,
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CrossStart(this LayoutRect<float> self, FlexDirection direction) =>
        direction.IsRow() ? self.Top : self.Left;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CrossEnd(this LayoutRect<float> self, FlexDirection direction) =>
        direction.IsRow() ? self.Bottom : self.Right;
}
