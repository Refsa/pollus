namespace Pollus.UI.Layout;

using System.Runtime.CompilerServices;

public struct UITreeRef(UITreeAdapter adapter) : ILayoutTree
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int ChildCount(int nodeId) => adapter.ChildCount(nodeId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<int> GetChildIds(int nodeId) => adapter.GetChildIds(nodeId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref readonly Style GetStyle(int nodeId) => ref adapter.GetStyle(nodeId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void SetUnroundedLayout(int nodeId, in NodeLayout layout) =>
        adapter.SetUnroundedLayout(nodeId, in layout);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref NodeLayout GetLayout(int nodeId) => ref adapter.GetLayout(nodeId);

    public LayoutOutput ComputeChildLayout(int nodeId, in LayoutInput input)
    {
        var self = this;
        return FlexLayout.ComputeFlexbox(ref self, nodeId, input);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool HasMeasureFunc(int nodeId) => adapter.HasMeasureFunc(nodeId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly LayoutOutput Measure(int nodeId, in LayoutInput input) =>
        adapter.Measure(nodeId, in input);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryCacheGet(int nodeId, in LayoutInput input, out LayoutOutput output) =>
        adapter.TryCacheGet(nodeId, in input, out output);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CacheStore(int nodeId, in LayoutInput input, in LayoutOutput output) =>
        adapter.CacheStore(nodeId, in input, in output);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void MarkDirty(int nodeId) => adapter.MarkDirty(nodeId);
}
