using System.Buffers;
using System.Runtime.InteropServices;
using Pollus.ECS;

namespace Pollus.UI.Layout;

public struct SyncStats
{
    public int StylesCopied;
    public int NodesAdded;
    public int NodesRemoved;
    public int HierarchyRebuilds;
}

public class UITreeAdapter : ILayoutTree
{
    readonly List<Style> _styles = [];
    readonly List<List<int>> _children = [];
    readonly List<NodeLayout> _layouts = [];
    readonly List<NodeLayout> _unroundedLayouts = [];
    readonly List<bool> _hasMeasure = [];
    readonly List<MeasureFunc?> _nodeMeasureFuncs = [];
    readonly List<LayoutCache> _caches = [];
    readonly List<int> _parentNode = [];

    readonly List<Entity> _nodeToEntity = [];
    readonly Dictionary<Entity, int> _entityToNode = [];
    readonly Dictionary<Entity, MeasureFunc> _entityMeasureFuncs = [];

    readonly List<int> _roots = [];
    readonly Stack<int> _freeNodes = new();

    // Reusable temp collections to avoid per-frame allocation
    readonly HashSet<Entity> _tempAlive = [];
    readonly List<Entity> _tempRemove = [];

    readonly Dictionary<Entity, Size<float>> _lastRootSizes = [];

    SyncStats _syncStats;
    public SyncStats LastSyncStats => _syncStats;

    public ReadOnlySpan<int> Roots => CollectionsMarshal.AsSpan(_roots);

    int AllocateNode()
    {
        int id = _styles.Count;
        _styles.Add(Style.Default);
        _children.Add([]);
        _layouts.Add(NodeLayout.Zero);
        _unroundedLayouts.Add(NodeLayout.Zero);
        _hasMeasure.Add(false);
        _nodeMeasureFuncs.Add(null);
        _caches.Add(new LayoutCache());
        _parentNode.Add(-1);
        _nodeToEntity.Add(Entity.Null);
        return id;
    }

    int AllocateOrReuseNode()
    {
        if (_freeNodes.TryPop(out int nodeId))
        {
            _styles[nodeId] = Style.Default;
            _children[nodeId].Clear();
            _layouts[nodeId] = NodeLayout.Zero;
            _unroundedLayouts[nodeId] = NodeLayout.Zero;
            _hasMeasure[nodeId] = false;
            _nodeMeasureFuncs[nodeId] = null;
            _caches[nodeId].Clear();
            _parentNode[nodeId] = -1;
            _nodeToEntity[nodeId] = Entity.Null;
            return nodeId;
        }
        return AllocateNode();
    }

    void FreeNode(int nodeId)
    {
        _nodeToEntity[nodeId] = Entity.Null;
        _children[nodeId].Clear();
        _caches[nodeId].Clear();
        _hasMeasure[nodeId] = false;
        _nodeMeasureFuncs[nodeId] = null;
        _parentNode[nodeId] = -1;
        _freeNodes.Push(nodeId);
    }

    public void SyncFull(Query query)
    {
        _syncStats = default;

        // Phase 1: Discover currently alive UINode entities
        _tempAlive.Clear();
        query.ForEach((in Entity entity) =>
        {
            if (query.Has<UINode>(entity))
                _tempAlive.Add(entity);
        });

        // Phase 2: Remove nodes for despawned entities
        _tempRemove.Clear();
        foreach (var (entity, _) in _entityToNode)
        {
            if (!_tempAlive.Contains(entity))
                _tempRemove.Add(entity);
        }
        foreach (var entity in _tempRemove)
        {
            int nodeId = _entityToNode[entity];

            // Mark old parent dirty — its children list is changing
            int oldParentId = _parentNode[nodeId];
            if (oldParentId >= 0)
                MarkDirtyWithAncestors(oldParentId);

            _entityToNode.Remove(entity);
            _lastRootSizes.Remove(entity);
            FreeNode(nodeId);
            _syncStats.NodesRemoved++;
        }

        // Phase 3: Add nodes for new entities
        foreach (var entity in _tempAlive)
        {
            if (_entityToNode.ContainsKey(entity)) continue;

            int nodeId = AllocateOrReuseNode();
            _nodeToEntity[nodeId] = entity;
            _entityToNode[entity] = nodeId;

            if (query.Has<UIStyle>(entity))
                _styles[nodeId] = query.Get<UIStyle>(entity).Value;

            if (query.Has<ContentSize>(entity)
                && _entityMeasureFuncs.TryGetValue(entity, out var measureFunc))
            {
                _hasMeasure[nodeId] = true;
                _nodeMeasureFuncs[nodeId] = measureFunc;
            }

            MarkDirtyWithAncestors(nodeId);
            _syncStats.NodesAdded++;
        }

        // Phase 4: Sync styles for existing entities (structural comparison)
        // New nodes already got their style in Phase 3; the comparison below
        // will see equality and skip them (no extra StylesCopied count).
        foreach (var entity in _tempAlive)
        {
            int nodeId = _entityToNode[entity];

            // Structural style comparison — only copy if actually changed
            if (query.Has<UIStyle>(entity))
            {
                var ecsStyle = query.Get<UIStyle>(entity).Value;
                if (!_styles[nodeId].Equals(ecsStyle))
                {
                    _styles[nodeId] = ecsStyle;
                    MarkDirtyWithAncestors(nodeId);
                    _syncStats.StylesCopied++;
                }
            }

            _entityMeasureFuncs.TryGetValue(entity, out var mf);
            bool shouldHaveMeasure = query.Has<ContentSize>(entity) && mf is not null;
            if (shouldHaveMeasure != _hasMeasure[nodeId])
            {
                _hasMeasure[nodeId] = shouldHaveMeasure;
                _nodeMeasureFuncs[nodeId] = shouldHaveMeasure ? mf : null;
                MarkDirtyWithAncestors(nodeId);
            }
            else if (shouldHaveMeasure && _nodeMeasureFuncs[nodeId] != mf)
            {
                _nodeMeasureFuncs[nodeId] = mf;
                MarkDirtyWithAncestors(nodeId);
            }
        }

        // Phase 5: Rebuild hierarchy from ECS and detect changes
        int capacity = NodeCapacity;
        var oldParents = ArrayPool<int>.Shared.Rent(capacity);
        try
        {
            for (int i = 0; i < capacity; i++)
                oldParents[i] = _parentNode[i];

            for (int i = 0; i < capacity; i++)
            {
                _children[i].Clear();
                _parentNode[i] = -1;
            }

            // Build children arrays from ECS hierarchy
            foreach (var (entity, nodeId) in _entityToNode)
            {
                if (!query.Has<Parent>(entity)) continue;

                ref readonly var parent = ref query.Get<Parent>(entity);
                var childEntity = parent.FirstChild;
                while (!childEntity.IsNull)
                {
                    if (_entityToNode.TryGetValue(childEntity, out int childNodeId))
                        _children[nodeId].Add(childNodeId);

                    if (!query.Has<Child>(childEntity)) break;
                    childEntity = query.Get<Child>(childEntity).NextSibling;
                }
            }

            for (int i = 0; i < capacity; i++)
            {
                foreach (int childId in _children[i])
                    _parentNode[childId] = i;
            }

            for (int i = 0; i < capacity; i++)
            {
                if (_nodeToEntity[i].IsNull) continue; // free slot
                if (_parentNode[i] != oldParents[i])
                {
                    // Parent changed — mark both old and new parents dirty
                    if (_parentNode[i] >= 0)
                        MarkDirtyWithAncestors(_parentNode[i]);
                    if (oldParents[i] >= 0 && !_nodeToEntity[oldParents[i]].IsNull)
                        MarkDirtyWithAncestors(oldParents[i]);
                    _syncStats.HierarchyRebuilds++;
                }
            }
        }
        finally
        {
            ArrayPool<int>.Shared.Return(oldParents);
        }

        // Phase 6: Detect roots
        _roots.Clear();
        foreach (var (entity, nodeId) in _entityToNode)
        {
            bool isRoot;
            if (!query.Has<Child>(entity))
            {
                isRoot = true;
            }
            else
            {
                ref readonly var child = ref query.Get<Child>(entity);
                if (child.Parent.IsNull)
                    isRoot = true;
                else
                    isRoot = !_entityToNode.ContainsKey(child.Parent);
            }

            if (isRoot)
                _roots.Add(nodeId);
        }

        // Phase 7: Also mark dirty from ECS flags (catches SetComponent-based changes)
        foreach (var (entity, nodeId) in _entityToNode)
        {
            if (query.Has<UIStyle>(entity) && query.Changed<UIStyle>(entity))
            {
                // Style was already synced structurally above;
                // this just ensures the dirty flag propagates for SetComponent changes
                // that might have been copied in Phase 4.
                MarkDirtyWithAncestors(nodeId);
            }

            if (query.Has<ContentSize>(entity) && query.Changed<ContentSize>(entity))
            {
                MarkDirtyWithAncestors(nodeId);
            }
        }
    }

    void MarkDirtyWithAncestors(int nodeId)
    {
        while (nodeId >= 0)
        {
            // Already dirty — ancestors are already marked too
            if (_caches[nodeId].IsDirty) return;

            _caches[nodeId].Clear();
            nodeId = _parentNode[nodeId];
        }
    }

    /// Marks a node and all its descendants as dirty (used for viewport resize).
    public void MarkSubtreeDirty(int nodeId)
    {
        _caches[nodeId].Clear();
        foreach (int childId in _children[nodeId])
        {
            MarkSubtreeDirty(childId);
        }
    }

    /// Checks if the root viewport size changed since last layout.
    /// Returns true if the size changed (and updates the stored size).
    public bool CheckAndUpdateRootSize(Entity rootEntity, Size<float> newSize)
    {
        if (_lastRootSizes.TryGetValue(rootEntity, out var lastSize)
            && lastSize.Width == newSize.Width && lastSize.Height == newSize.Height)
        {
            return false;
        }
        _lastRootSizes[rootEntity] = newSize;
        return true;
    }

    public Entity GetEntity(int nodeId) => _nodeToEntity[nodeId];

    public ref NodeLayout GetRoundedLayout(int nodeId) =>
        ref CollectionsMarshal.AsSpan(_layouts)[nodeId];

    public ref NodeLayout GetUnroundedLayout(int nodeId) =>
        ref CollectionsMarshal.AsSpan(_unroundedLayouts)[nodeId];

    public int GetNodeId(Entity entity) =>
        _entityToNode.TryGetValue(entity, out int id) ? id : -1;

    public int NodeCapacity => _styles.Count;
    public int ActiveNodeCount => _entityToNode.Count;

    public int ChildCount(int nodeId) => _children[nodeId].Count;

    public ReadOnlySpan<int> GetChildIds(int nodeId) =>
        CollectionsMarshal.AsSpan(_children[nodeId]);

    public ref readonly Style GetStyle(int nodeId) =>
        ref CollectionsMarshal.AsSpan(_styles)[nodeId];

    public void SetUnroundedLayout(int nodeId, in NodeLayout layout)
    {
        _unroundedLayouts[nodeId] = layout;
        _layouts[nodeId] = layout;
    }

    public ref NodeLayout GetLayout(int nodeId) =>
        ref CollectionsMarshal.AsSpan(_layouts)[nodeId];

    public LayoutOutput ComputeChildLayout(int nodeId, in LayoutInput input)
    {
        var self = this;
        return FlexLayout.ComputeFlexbox(ref self, nodeId, input);
    }

    public void SetMeasureFunc(Entity entity, MeasureFunc func)
        => _entityMeasureFuncs[entity] = func;

    public void RemoveMeasureFunc(Entity entity)
        => _entityMeasureFuncs.Remove(entity);

    public bool HasMeasureFunc(int nodeId) => _hasMeasure[nodeId];

    public LayoutOutput Measure(int nodeId, in LayoutInput input)
    {
        var func = _nodeMeasureFuncs[nodeId];
        if (func is null) return LayoutOutput.Zero;
        var size = func(input.KnownDimensions, input.AvailableSpace);
        return new LayoutOutput { Size = size };
    }

    public bool TryCacheGet(int nodeId, in LayoutInput input, out LayoutOutput output)
        => _caches[nodeId].TryGet(in input, out output);

    public void CacheStore(int nodeId, in LayoutInput input, in LayoutOutput output)
        => _caches[nodeId].Store(in input, in output);

    public void MarkDirty(int nodeId)
        => _caches[nodeId].Clear();
}
