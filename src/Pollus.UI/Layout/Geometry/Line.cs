namespace Pollus.UI.Layout;

public record struct Line<T>
{
    public T Start;
    public T End;

    public Line(T start, T end)
    {
        Start = start;
        End = end;
    }

    public static readonly Line<T> Zero = default;
}

public static class LineFloatExtensions
{
    public static float Sum(this Line<float> self) => self.Start + self.End;
}
