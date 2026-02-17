using System.Runtime.CompilerServices;

namespace Pollus.UI.Layout;

public enum Display : byte
{
    Flex = 0,
    None = 1,
    Block = 2,
    Grid = 3,
}

public enum FlexDirection : byte
{
    Row = 0,
    Column = 1,
    RowReverse = 2,
    ColumnReverse = 3,
}

public enum FlexWrap : byte
{
    NoWrap = 0,
    Wrap = 1,
    WrapReverse = 2,
}

public enum Position : byte
{
    Relative = 0,
    Absolute = 1,
}

public enum Overflow : byte
{
    Visible = 0,
    Clip = 1,
    Hidden = 2,
    Scroll = 3, // no-op milestone 1
}

public enum BoxSizing : byte
{
    BorderBox = 0,
    ContentBox = 1,
}

public static class FlexDirectionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRow(this FlexDirection dir) =>
        dir == FlexDirection.Row || dir == FlexDirection.RowReverse;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsColumn(this FlexDirection dir) =>
        dir == FlexDirection.Column || dir == FlexDirection.ColumnReverse;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReverse(this FlexDirection dir) =>
        dir == FlexDirection.RowReverse || dir == FlexDirection.ColumnReverse;
}
