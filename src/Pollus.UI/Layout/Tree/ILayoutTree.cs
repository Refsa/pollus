namespace Pollus.UI.Layout;

public interface ILayoutTree
{
    int ChildCount(int nodeId);
    ReadOnlySpan<int> GetChildIds(int nodeId);
    ref readonly Style GetStyle(int nodeId);
    void SetUnroundedLayout(int nodeId, in NodeLayout layout);
    ref NodeLayout GetLayout(int nodeId);
    LayoutOutput ComputeChildLayout(int nodeId, in LayoutInput input);
    bool HasMeasureFunc(int nodeId);
    LayoutOutput Measure(int nodeId, in LayoutInput input);
    bool TryCacheGet(int nodeId, in LayoutInput input, out LayoutOutput output);
    void CacheStore(int nodeId, in LayoutInput input, in LayoutOutput output);
    void MarkDirty(int nodeId);
}
