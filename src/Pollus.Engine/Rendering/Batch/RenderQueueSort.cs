namespace Pollus.Engine.Rendering;

internal readonly record struct RenderQueueSortKey(
    int RenderStep,
    ulong SortKey,
    int SortIndex,
    int RunOrdinal
);

internal sealed class RenderQueueSortKeyComparer : IComparer<RenderQueueSortKey>
{
    public static readonly RenderQueueSortKeyComparer Instance = new();

    public int Compare(RenderQueueSortKey left, RenderQueueSortKey right)
    {
        var result = left.RenderStep.CompareTo(right.RenderStep);
        if (result != 0) return result;

        result = left.SortKey.CompareTo(right.SortKey);
        if (result != 0) return result;

        result = left.SortIndex.CompareTo(right.SortIndex);
        if (result != 0) return result;

        result = left.RunOrdinal.CompareTo(right.RunOrdinal);
        if (result != 0) return result;

        return result;
    }
}