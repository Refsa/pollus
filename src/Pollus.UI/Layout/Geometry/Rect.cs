using System.Runtime.CompilerServices;

namespace Pollus.UI.Layout;

public record struct Rect<T>
{
    public T Left;
    public T Right;
    public T Top;
    public T Bottom;

    public Rect(T left, T right, T top, T bottom)
    {
        Left = left;
        Right = right;
        Top = top;
        Bottom = bottom;
    }

    public static Rect<T> All(T value) => new(value, value, value, value);
    public static readonly Rect<T> Zero = default;
}

public static class RectFloatExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float HorizontalAxisSum(this Rect<float> self) => self.Left + self.Right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float VerticalAxisSum(this Rect<float> self) => self.Top + self.Bottom;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size<float> SumAxes(this Rect<float> self) =>
        new(self.Left + self.Right, self.Top + self.Bottom);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MainAxisSum(this Rect<float> self, FlexDirection direction) =>
        direction.IsRow() ? self.HorizontalAxisSum() : self.VerticalAxisSum();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CrossAxisSum(this Rect<float> self, FlexDirection direction) =>
        direction.IsRow() ? self.VerticalAxisSum() : self.HorizontalAxisSum();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MainStart(this Rect<float> self, FlexDirection direction) =>
        direction switch
        {
            FlexDirection.Row => self.Left,
            FlexDirection.RowReverse => self.Right,
            FlexDirection.Column => self.Top,
            FlexDirection.ColumnReverse => self.Bottom,
            _ => self.Left,
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MainEnd(this Rect<float> self, FlexDirection direction) =>
        direction switch
        {
            FlexDirection.Row => self.Right,
            FlexDirection.RowReverse => self.Left,
            FlexDirection.Column => self.Bottom,
            FlexDirection.ColumnReverse => self.Top,
            _ => self.Right,
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CrossStart(this Rect<float> self, FlexDirection direction) =>
        direction.IsRow() ? self.Top : self.Left;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CrossEnd(this Rect<float> self, FlexDirection direction) =>
        direction.IsRow() ? self.Bottom : self.Right;
}
