using System.Buffers;
using System.Runtime.CompilerServices;
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
    const int InitialCapacity = 16;

    Style[] _styles = new Style[InitialCapacity];
    NodeLayout[] _layouts = new NodeLayout[InitialCapacity];
    NodeLayout[] _unroundedLayouts = new NodeLayout[InitialCapacity];
    bool[] _hasMeasure = new bool[InitialCapacity];
    MeasureFunc?[] _nodeMeasureFuncs = new MeasureFunc?[InitialCapacity];
    LayoutCache[] _caches = new LayoutCache[InitialCapacity];
    int[] _parentNode = new int[InitialCapacity];
    Entity[] _nodeToEntity = new Entity[InitialCapacity];

    // Flat children buffer
    int[] _childBuffer = [];
    int[] _childOffsets = new int[InitialCapacity];
    int[] _childCounts = new int[InitialCapacity];

    int _nodeCapacity;

    readonly Dictionary<Entity, int> _entityToNode = [];
    readonly Dictionary<Entity, MeasureFunc> _entityMeasureFuncs = [];

    readonly List<int> _roots = [];
    readonly Stack<int> _freeNodes = new();

    // Reusable temp collections to avoid per-frame allocation
    readonly HashSet<Entity> _tempAlive = [];
    readonly List<Entity> _tempRemove = [];

    readonly Dictionary<Entity, Size<float>> _lastRootSizes = [];

    bool _globalDirty = true;
    bool _hierarchyDirty = true;

    SyncStats _syncStats;
    public SyncStats LastSyncStats => _syncStats;

    /// True if any node's layout cache is dirty (style/hierarchy/resize changed).
    public bool IsDirty => _globalDirty;

    /// Clears the global dirty flag after layout has been computed and written back.
    public void ClearDirty() => _globalDirty = false;

    public ReadOnlySpan<int> Roots => System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_roots);

    public UITreeAdapter()
    {
        // Initialize all parent nodes to -1 and entities to Null
        Array.Fill(_parentNode, -1);
        Array.Fill(_nodeToEntity, Entity.Null);
        for (int i = 0; i < InitialCapacity; i++)
            _caches[i] = new LayoutCache();
    }

    void EnsureCapacity(int minCapacity)
    {
        if (minCapacity <= _nodeCapacity) return;
        int newCap = Math.Max(_nodeCapacity * 2, minCapacity);
        newCap = Math.Max(newCap, InitialCapacity);

        Grow(ref _styles, newCap);
        Grow(ref _layouts, newCap);
        Grow(ref _unroundedLayouts, newCap);
        Grow(ref _hasMeasure, newCap);
        Grow(ref _nodeMeasureFuncs, newCap);
        Grow(ref _parentNode, newCap);
        Grow(ref _nodeToEntity, newCap);
        Grow(ref _childOffsets, newCap);
        Grow(ref _childCounts, newCap);

        var oldCaches = _caches;
        _caches = new LayoutCache[newCap];
        Array.Copy(oldCaches, _caches, oldCaches.Length);

        // Initialize new slots
        for (int i = _nodeCapacity; i < newCap; i++)
        {
            _parentNode[i] = -1;
            _nodeToEntity[i] = Entity.Null;
            _caches[i] = new LayoutCache();
        }

        _nodeCapacity = newCap;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void Grow<T>(ref T[] array, int newCap)
    {
        var old = array;
        array = new T[newCap];
        Array.Copy(old, array, old.Length);
    }

    int AllocateNode()
    {
        int id = _nodeCapacity;
        EnsureCapacity(id + 1);
        _styles[id] = Style.Default;
        _layouts[id] = NodeLayout.Zero;
        _unroundedLayouts[id] = NodeLayout.Zero;
        _hasMeasure[id] = false;
        _nodeMeasureFuncs[id] = null;
        _caches[id] = new LayoutCache();
        _parentNode[id] = -1;
        _nodeToEntity[id] = Entity.Null;
        _childOffsets[id] = 0;
        _childCounts[id] = 0;
        _nodeCapacity = id + 1;
        return id;
    }

    int AllocateOrReuseNode()
    {
        if (_freeNodes.TryPop(out int nodeId))
        {
            _styles[nodeId] = Style.Default;
            _layouts[nodeId] = NodeLayout.Zero;
            _unroundedLayouts[nodeId] = NodeLayout.Zero;
            _hasMeasure[nodeId] = false;
            _nodeMeasureFuncs[nodeId] = null;
            _caches[nodeId] = new LayoutCache();
            _parentNode[nodeId] = -1;
            _nodeToEntity[nodeId] = Entity.Null;
            _childCounts[nodeId] = 0;
            return nodeId;
        }
        return AllocateNode();
    }

    void FreeNode(int nodeId)
    {
        _nodeToEntity[nodeId] = Entity.Null;
        _childCounts[nodeId] = 0;
        _caches[nodeId].Clear();
        _hasMeasure[nodeId] = false;
        _nodeMeasureFuncs[nodeId] = null;
        _parentNode[nodeId] = -1;
        _freeNodes.Push(nodeId);
    }

    public void SyncFull(Query<UINode> uiNodeQuery, Query query)
    {
        _syncStats = default;

        // Phase 1: Discover currently alive UINode entities
        // Uses typed Query<UINode> — only iterates archetypes containing UINode
        _tempAlive.Clear();
        uiNodeQuery.ForEach((in Entity entity, ref UINode _) =>
        {
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
            _hierarchyDirty = true;
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
            _hierarchyDirty = true;
        }

        // Phase 4: Sync styles for existing entities (structural comparison)
        foreach (var entity in _tempAlive)
        {
            int nodeId = _entityToNode[entity];

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

        // Phase 4b: Detect ECS hierarchy changes that need a rebuild
        if (!_hierarchyDirty)
        {
            foreach (var (entity, _) in _entityToNode)
            {
                if ((query.Has<Parent>(entity) && query.Changed<Parent>(entity))
                    || (query.Has<Child>(entity) && query.Changed<Child>(entity)))
                {
                    _hierarchyDirty = true;
                    break;
                }
            }
        }

        // Phase 5: Rebuild hierarchy from ECS — flat children buffer
        // Skipped when hierarchy hasn't changed (no add/remove/reparent)
        if (_hierarchyDirty)
        {
            int capacity = _nodeCapacity;
            var oldParents = ArrayPool<int>.Shared.Rent(capacity);
            try
            {
                Array.Copy(_parentNode, oldParents, capacity);

                // Reset parent and child counts
                Array.Fill(_parentNode, -1, 0, capacity);
                Array.Fill(_childCounts, 0, 0, capacity);

                // Pass 1: count children per parent
                foreach (var (entity, nodeId) in _entityToNode)
                {
                    if (!query.Has<Parent>(entity)) continue;

                    ref readonly var parent = ref query.Get<Parent>(entity);
                    var childEntity = parent.FirstChild;
                    while (!childEntity.IsNull)
                    {
                        if (_entityToNode.ContainsKey(childEntity))
                            _childCounts[nodeId]++;

                        if (!query.Has<Child>(childEntity)) break;
                        childEntity = query.Get<Child>(childEntity).NextSibling;
                    }
                }

                // Compute offsets via prefix sum
                int totalChildren = 0;
                for (int i = 0; i < capacity; i++)
                {
                    _childOffsets[i] = totalChildren;
                    totalChildren += _childCounts[i];
                }

                // Grow child buffer if needed (only grows)
                if (_childBuffer.Length < totalChildren)
                    _childBuffer = new int[Math.Max(totalChildren, 16)];

                // Reset counts to use as write cursors
                Array.Fill(_childCounts, 0, 0, capacity);

                // Pass 2: write child IDs into flat buffer
                foreach (var (entity, nodeId) in _entityToNode)
                {
                    if (!query.Has<Parent>(entity)) continue;

                    ref readonly var parent = ref query.Get<Parent>(entity);
                    var childEntity = parent.FirstChild;
                    while (!childEntity.IsNull)
                    {
                        if (_entityToNode.TryGetValue(childEntity, out int childNodeId))
                        {
                            _childBuffer[_childOffsets[nodeId] + _childCounts[nodeId]] = childNodeId;
                            _childCounts[nodeId]++;
                        }

                        if (!query.Has<Child>(childEntity)) break;
                        childEntity = query.Get<Child>(childEntity).NextSibling;
                    }
                }

                // Set parent pointers from children
                for (int i = 0; i < capacity; i++)
                {
                    var children = GetChildIds(i);
                    for (int j = 0; j < children.Length; j++)
                        _parentNode[children[j]] = i;
                }

                // Detect hierarchy changes
                for (int i = 0; i < capacity; i++)
                {
                    if (_nodeToEntity[i].IsNull) continue;
                    if (_parentNode[i] != oldParents[i])
                    {
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

            _hierarchyDirty = false;
        }

        // Phase 6+7 merged: Detect roots and mark dirty from ECS flags
        // Root detection must always run (roots list was cleared or may change)
        _roots.Clear();
        foreach (var (entity, nodeId) in _entityToNode)
        {
            // Root detection
            bool isRoot;
            if (!query.Has<Child>(entity))
            {
                isRoot = true;
            }
            else
            {
                ref readonly var child = ref query.Get<Child>(entity);
                isRoot = child.Parent.IsNull || !_entityToNode.ContainsKey(child.Parent);
            }

            if (isRoot)
                _roots.Add(nodeId);

            // Dirty from ECS flags
            if (query.Has<UIStyle>(entity) && query.Changed<UIStyle>(entity))
                MarkDirtyWithAncestors(nodeId);

            if (query.Has<ContentSize>(entity) && query.Changed<ContentSize>(entity))
                MarkDirtyWithAncestors(nodeId);
        }

        // Phase 8: Detect viewport resize — marks entire subtree dirty
        foreach (var rootNodeId in Roots)
        {
            var rootEntity = GetEntity(rootNodeId);
            if (!query.Has<UILayoutRoot>(rootEntity)) continue;

            ref readonly var layoutRoot = ref query.Get<UILayoutRoot>(rootEntity);
            if (CheckAndUpdateRootSize(rootEntity, layoutRoot.Size))
                MarkSubtreeDirty(rootNodeId);
        }
    }

    void MarkDirtyWithAncestors(int nodeId)
    {
        _globalDirty = true;
        while (nodeId >= 0)
        {
            if (!_caches[nodeId].IsDirty)
                _caches[nodeId].Clear();
            nodeId = _parentNode[nodeId];
        }
    }

    /// Marks a node and all its descendants as dirty (used for viewport resize).
    public void MarkSubtreeDirty(int nodeId)
    {
        _globalDirty = true;
        _caches[nodeId].Clear();
        var children = GetChildIds(nodeId);
        for (int i = 0; i < children.Length; i++)
            MarkSubtreeDirty(children[i]);
    }

    /// Checks if the root viewport size changed since last layout.
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

    /// Enumerates active (entity, nodeId) pairs for write-back.
    public Dictionary<Entity, int>.Enumerator GetActiveNodes() =>
        _entityToNode.GetEnumerator();

    public Entity GetEntity(int nodeId) => _nodeToEntity[nodeId];

    public ref NodeLayout GetRoundedLayout(int nodeId) =>
        ref _layouts[nodeId];

    public ref NodeLayout GetUnroundedLayout(int nodeId) =>
        ref _unroundedLayouts[nodeId];

    public int GetNodeId(Entity entity) =>
        _entityToNode.TryGetValue(entity, out int id) ? id : -1;

    public int NodeCapacity => _nodeCapacity;
    public int ActiveNodeCount => _entityToNode.Count;

    public int ChildCount(int nodeId) => _childCounts[nodeId];

    public ReadOnlySpan<int> GetChildIds(int nodeId) =>
        _childBuffer.AsSpan(_childOffsets[nodeId], _childCounts[nodeId]);

    public ref readonly Style GetStyle(int nodeId) =>
        ref _styles[nodeId];

    public void SetUnroundedLayout(int nodeId, in NodeLayout layout)
    {
        _unroundedLayouts[nodeId] = layout;
        _layouts[nodeId] = layout;
    }

    public ref NodeLayout GetLayout(int nodeId) =>
        ref _layouts[nodeId];

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
