namespace Pollus.Benchmark;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Pollus.ECS;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class UIBenchmarks
{
    [Params(10, 100, 500)]
    public int NodeCount;

    // ── Shared state ──────────────────────────────────────────────────

    World flatWorld = null!;
    UITreeAdapter flatAdapter = null!;
    int flatRootNodeId;

    World deepWorld = null!;
    UITreeAdapter deepAdapter = null!;
    int deepRootNodeId;

    World gridWorld = null!;
    UITreeAdapter gridAdapter = null!;
    int gridRootNodeId;

    // For HitTest
    World hitFlatWorld = null!;
    Query hitFlatQuery;
    UIHitTestResult hitFlatResult = null!;
    UIFocusState hitFlatFocusState = null!;

    World hitDeepWorld = null!;
    Query hitDeepQuery;
    UIHitTestResult hitDeepResult = null!;
    UIFocusState hitDeepFocusState = null!;

    // ── Setup ─────────────────────────────────────────────────────────

    [GlobalSetup]
    public void Setup()
    {
        SetupFlatWorld();
        SetupDeepWorld();
        SetupGridWorld();
        SetupHitTestFlatWorld();
        SetupHitTestDeepWorld();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        flatWorld?.Dispose();
        deepWorld?.Dispose();
        gridWorld?.Dispose();
        hitFlatWorld?.Dispose();
        hitDeepWorld?.Dispose();
    }

    void SetupFlatWorld()
    {
        flatWorld = UIBenchmarkHelpers.CreateUIWorld();
        var commands = flatWorld.GetCommands();
        var root = UIBenchmarkHelpers.SpawnRoot(commands, 800, 600);
        UIBenchmarkHelpers.SpawnFlatChildren(commands, root, NodeCount);
        flatWorld.Update();

        flatAdapter = flatWorld.Resources.Get<UITreeAdapter>();
        flatRootNodeId = flatAdapter.GetNodeId(root);
    }

    void SetupDeepWorld()
    {
        deepWorld = UIBenchmarkHelpers.CreateUIWorld();
        var commands = deepWorld.GetCommands();
        var root = UIBenchmarkHelpers.SpawnRoot(commands, 800, 600);
        UIBenchmarkHelpers.SpawnDeepChain(commands, root, NodeCount);
        deepWorld.Update();

        deepAdapter = deepWorld.Resources.Get<UITreeAdapter>();
        deepRootNodeId = deepAdapter.GetNodeId(root);
    }

    void SetupGridWorld()
    {
        gridWorld = UIBenchmarkHelpers.CreateUIWorld();
        var commands = gridWorld.GetCommands();
        var root = UIBenchmarkHelpers.SpawnRoot(commands, 800, 600);
        int side = (int)System.Math.Ceiling(System.Math.Sqrt((double)NodeCount));
        UIBenchmarkHelpers.SpawnGrid(commands, root, side, side);
        gridWorld.Update();

        gridAdapter = gridWorld.Resources.Get<UITreeAdapter>();
        gridRootNodeId = gridAdapter.GetNodeId(root);
    }

    void SetupHitTestFlatWorld()
    {
        hitFlatWorld = UIBenchmarkHelpers.CreateUIWorld();
        var commands = hitFlatWorld.GetCommands();
        var root = UIBenchmarkHelpers.SpawnRoot(commands, 800, 600);
        UIBenchmarkHelpers.SpawnInteractiveChildren(commands, root, NodeCount);
        hitFlatWorld.Update();

        hitFlatQuery = new Query(hitFlatWorld);
        hitFlatResult = hitFlatWorld.Resources.Get<UIHitTestResult>();
        hitFlatFocusState = hitFlatWorld.Resources.Get<UIFocusState>();
    }

    void SetupHitTestDeepWorld()
    {
        hitDeepWorld = UIBenchmarkHelpers.CreateUIWorld();
        var commands = hitDeepWorld.GetCommands();
        var root = UIBenchmarkHelpers.SpawnRoot(commands, 800, 600);
        UIBenchmarkHelpers.SpawnInteractiveDeepChain(commands, root, NodeCount);
        hitDeepWorld.Update();

        hitDeepQuery = new Query(hitDeepWorld);
        hitDeepResult = hitDeepWorld.Resources.Get<UIHitTestResult>();
        hitDeepFocusState = hitDeepWorld.Resources.Get<UIFocusState>();
    }

    // ── Layout input helpers ──────────────────────────────────────────

    static LayoutInput MakeRootInput(float width, float height)
    {
        return new LayoutInput
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
    }

    // ── Layout benchmarks ─────────────────────────────────────────────

    [Benchmark]
    [BenchmarkCategory("Layout")]
    public LayoutOutput ColdLayout_FlatList()
    {
        flatAdapter.MarkSubtreeDirty(flatRootNodeId);
        var treeRef = new UITreeRef(flatAdapter);
        return FlexLayout.ComputeFlexbox(ref treeRef, flatRootNodeId, MakeRootInput(800, 600));
    }

    [Benchmark]
    [BenchmarkCategory("Layout")]
    public LayoutOutput ColdLayout_DeepNesting()
    {
        deepAdapter.MarkSubtreeDirty(deepRootNodeId);
        var treeRef = new UITreeRef(deepAdapter);
        return FlexLayout.ComputeFlexbox(ref treeRef, deepRootNodeId, MakeRootInput(800, 600));
    }

    [Benchmark]
    [BenchmarkCategory("Layout")]
    public LayoutOutput ColdLayout_Grid()
    {
        gridAdapter.MarkSubtreeDirty(gridRootNodeId);
        var treeRef = new UITreeRef(gridAdapter);
        return FlexLayout.ComputeFlexbox(ref treeRef, gridRootNodeId, MakeRootInput(800, 600));
    }

    [Benchmark]
    [BenchmarkCategory("Layout")]
    public LayoutOutput WarmLayout_NoChange()
    {
        // Caches are populated from Setup — measure cache lookup overhead
        var treeRef = new UITreeRef(flatAdapter);
        return FlexLayout.ComputeFlexbox(ref treeRef, flatRootNodeId, MakeRootInput(800, 600));
    }

    [Benchmark]
    [BenchmarkCategory("Layout")]
    public LayoutOutput DirtyLeaf_Relayout()
    {
        // Dirty a single leaf node, then recompute the full tree
        var children = flatAdapter.GetChildIds(flatRootNodeId);
        if (children.Length > 0)
        {
            int leafNodeId = children[children.Length / 2];
            flatAdapter.MarkDirty(leafNodeId);
        }
        var treeRef = new UITreeRef(flatAdapter);
        return FlexLayout.ComputeFlexbox(ref treeRef, flatRootNodeId, MakeRootInput(800, 600));
    }

    [Benchmark]
    [BenchmarkCategory("Layout")]
    public LayoutOutput ViewportResize()
    {
        // Full tree invalidation from viewport size change
        flatAdapter.MarkSubtreeDirty(flatRootNodeId);
        var treeRef = new UITreeRef(flatAdapter);
        return FlexLayout.ComputeFlexbox(ref treeRef, flatRootNodeId, MakeRootInput(1024, 768));
    }

    // ── Tree sync benchmarks ──────────────────────────────────────────

    [Benchmark]
    [BenchmarkCategory("Sync")]
    public void SyncFull_NoChange()
    {
        var uiNodeQuery = new Query<UINode>(flatWorld);
        var query = new Query(flatWorld);
        flatAdapter.SyncFull(uiNodeQuery, query);
    }

    // ── WriteBack benchmarks ──────────────────────────────────────────

    [Benchmark]
    [BenchmarkCategory("WriteBack")]
    public void WriteBack_AllNodes()
    {
        // Simulate what WriteBack does: iterate active entities, copy layout data
        var query = new Query(flatWorld);
        foreach (var entity in flatAdapter.ActiveEntities)
        {
            int nodeId = flatAdapter.GetNodeId(entity);
            if (nodeId < 0) continue;
            if (!query.Has<ComputedNode>(entity)) continue;

            ref readonly var rounded = ref flatAdapter.GetRoundedLayout(nodeId);
            ref var computed = ref query.GetTracked<ComputedNode>(entity);
            computed.Size = new Vec2f(rounded.Size.Width, rounded.Size.Height);
            computed.ContentSize = new Vec2f(rounded.ContentSize.Width, rounded.ContentSize.Height);
            computed.Position = new Vec2f(rounded.Location.X, rounded.Location.Y);
            computed.BorderLeft = rounded.Border.Left;
            computed.BorderRight = rounded.Border.Right;
            computed.BorderTop = rounded.Border.Top;
            computed.BorderBottom = rounded.Border.Bottom;
            computed.PaddingLeft = rounded.Padding.Left;
            computed.PaddingRight = rounded.Padding.Right;
            computed.PaddingTop = rounded.Padding.Top;
            computed.PaddingBottom = rounded.Padding.Bottom;
            computed.MarginLeft = rounded.Margin.Left;
            computed.MarginRight = rounded.Margin.Right;
            computed.MarginTop = rounded.Margin.Top;
            computed.MarginBottom = rounded.Margin.Bottom;
            computed.UnroundedSize = new Vec2f(rounded.Size.Width, rounded.Size.Height);
            computed.UnroundedPosition = new Vec2f(rounded.Location.X, rounded.Location.Y);
        }
    }

    // ── HitTest benchmarks ────────────────────────────────────────────

    [Benchmark]
    [BenchmarkCategory("HitTest")]
    public void HitTest_FlatList()
    {
        // Mouse positioned at middle child
        var mousePos = new Vec2f(400, NodeCount / 2 * 40 + 20);
        UIInteractionSystem.PerformHitTest(hitFlatQuery, hitFlatResult, hitFlatFocusState, mousePos);
    }

    [Benchmark]
    [BenchmarkCategory("HitTest")]
    public void HitTest_DeepNesting()
    {
        // Mouse positioned to hit the deepest leaf
        var mousePos = new Vec2f(10, 10);
        UIInteractionSystem.PerformHitTest(hitDeepQuery, hitDeepResult, hitDeepFocusState, mousePos);
    }

    // ── FullFrame benchmarks ──────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("FullFrame")]
    public void FullFrame_FlatList()
    {
        flatWorld.Update();
    }

    // ── Cache micro-benchmarks ────────────────────────────────────────

    [Benchmark]
    [BenchmarkCategory("Cache")]
    public bool LayoutCache_Hit()
    {
        // Populate cache with known input, then look it up
        var cache = new LayoutCache();
        var input = MakeRootInput(800, 600);
        var output = new LayoutOutput { Size = new Size<float>(800, 600) };
        cache.Store(in input, in output);

        return cache.TryGet(in input, out _);
    }

    [Benchmark]
    [BenchmarkCategory("Cache")]
    public bool LayoutCache_Miss()
    {
        // Populate cache with different inputs, then miss
        var cache = new LayoutCache();
        var storeInput = MakeRootInput(800, 600);
        var storeOutput = new LayoutOutput { Size = new Size<float>(800, 600) };

        // Fill all 9 slots with one pattern
        for (int i = 0; i < 9; i++)
        {
            var fillInput = MakeRootInput(100 + i, 100 + i);
            cache.Store(in fillInput, in storeOutput);
        }

        // Look up a pattern that doesn't exist
        var missInput = MakeRootInput(999, 999);
        return cache.TryGet(in missInput, out _);
    }
}
