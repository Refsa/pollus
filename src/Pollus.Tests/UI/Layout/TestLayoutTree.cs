using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

namespace Pollus.Tests.UI.Layout;

/// A simple ILayoutTree for unit tests — stores styles and children in arrays.
public class TestLayoutTree : ILayoutTree
{
    private readonly List<LayoutStyle> _styles = [];
    private readonly List<List<int>> _children = [];
    private readonly List<NodeLayout> _layouts = [];
    private readonly List<bool> _hasMeasure = [];
    private readonly List<Func<LayoutInput, LayoutOutput>?> _measureFuncs = [];

    public int AddNode(LayoutStyle style)
    {
        int id = _styles.Count;
        _styles.Add(style);
        _children.Add([]);
        _layouts.Add(NodeLayout.Zero);
        _hasMeasure.Add(false);
        _measureFuncs.Add(null);
        return id;
    }

    public void AddChild(int parentId, int childId)
    {
        _children[parentId].Add(childId);
    }

    public void SetMeasureFunc(int nodeId, Func<LayoutInput, LayoutOutput> func)
    {
        _hasMeasure[nodeId] = true;
        _measureFuncs[nodeId] = func;
    }

    public int ChildCount(int nodeId) => _children[nodeId].Count;

    public ReadOnlySpan<int> GetChildIds(int nodeId) =>
        System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_children[nodeId]);

    public ref readonly LayoutStyle GetStyle(int nodeId) =>
        ref System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_styles)[nodeId];

    public int SetCount;
    public List<string> SetLog = [];
    public void SetUnroundedLayout(int nodeId, in NodeLayout layout)
    {
        SetCount++;
        SetLog.Add($"[{nodeId}:{layout.Size.Width}x{layout.Size.Height} at {layout.Location.X},{layout.Location.Y}]");
        _layouts[nodeId] = layout;
    }

    public ref NodeLayout GetLayout(int nodeId) =>
        ref System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_layouts)[nodeId];

    public List<string> ComputeLog = [];
    public int ComputeCallCount;
    public LayoutOutput ComputeChildLayout(int nodeId, in LayoutInput input)
    {
        ComputeCallCount++;
        var self = this;
        var result = FlexLayout.ComputeFlexbox(ref self, nodeId, input);
        ComputeLog.Add($"[{nodeId}:{input.RunMode} known={input.KnownDimensions.Width}x{input.KnownDimensions.Height} -> {result.Size.Width}x{result.Size.Height}]");
        return result;
    }

    public bool HasMeasureFunc(int nodeId) => _hasMeasure[nodeId];

    public LayoutOutput Measure(int nodeId, in LayoutInput input) =>
        _measureFuncs[nodeId]?.Invoke(input) ?? LayoutOutput.Zero;

    /// Convenience: compute layout for root and return results.
    public NodeLayout ComputeRoot(int rootId, float width, float height)
    {
        var input = new LayoutInput
        {
            RunMode = RunMode.PerformLayout,
            SizingMode = SizingMode.InherentSize,
            Axis = RequestedAxis.Both,
            KnownDimensions = new Size<float?>(width, height),
            ParentSize = new Size<float?>(width, height),
            AvailableSpace = new Size<AvailableSpace>(
                AvailableSpace.Definite(width),
                AvailableSpace.Definite(height)
            ),
        };
        var self = this;
        var output = FlexLayout.ComputeFlexbox(ref self, rootId, input);
        // Write root layout (ComputeFlexbox writes children, not itself)
        ref var rootLayout = ref GetLayout(rootId);
        rootLayout.Size = output.Size;
        rootLayout.ContentSize = output.ContentSize;
        return rootLayout;
    }

    public NodeLayout GetNodeLayout(int nodeId) => _layouts[nodeId];
}
