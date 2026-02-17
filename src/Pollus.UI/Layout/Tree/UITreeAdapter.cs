namespace Pollus.UI.Layout;

using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.ECS;

public struct SyncStats
{
    public int StylesCopied;
    public int NodesAdded;
    public int NodesRemoved;
    public int HierarchyRebuilds;
}

public sealed class UITreeAdapter : ILayoutTree
{
    const int InitialCapacity = 16;

    Style[] styles = new Style[InitialCapacity];
    NodeLayout[] layouts = new NodeLayout[InitialCapacity];
    NodeLayout[] unroundedLayouts = new NodeLayout[InitialCapacity];
    bool[] hasMeasure = new bool[InitialCapacity];
    MeasureFunc?[] nodeMeasureFuncs = new MeasureFunc?[InitialCapacity];
    LayoutCache[] caches = new LayoutCache[InitialCapacity];
    int[] parentNode = new int[InitialCapacity];
    Entity[] nodeToEntity = new Entity[InitialCapacity];

    // Flat children buffer
    int[] childBuffer = [];
    int[] childOffsets = new int[InitialCapacity];
    int[] childCounts = new int[InitialCapacity];

    int nodeCapacity;

    int[] entityMap = new int[InitialCapacity];
    int entityMapCapacity = InitialCapacity;

    readonly List<Entity> activeEntities = [];

    readonly Dictionary<Entity, MeasureFunc> entityMeasureFuncs = [];

    readonly List<int> roots = [];
    readonly Stack<int> freeNodes = new();

    readonly HashSet<Entity> tempAlive = [];
    readonly List<Entity> tempAliveList = [];
    readonly List<Entity> tempRemove = [];

    readonly Dictionary<Entity, Size<float>> lastRootSizes = [];

    bool globalDirty = true;
    bool hierarchyDirty = true;
    bool measureFuncsDirty;

    SyncStats syncStats;
    public SyncStats LastSyncStats => syncStats;

    /// True if any node's layout cache is dirty (style/hierarchy/resize changed).
    public bool IsDirty => globalDirty;

    /// Clears the global dirty flag after layout has been computed and written back.
    public void ClearDirty() => globalDirty = false;

    public ReadOnlySpan<int> Roots => CollectionsMarshal.AsSpan(roots);

    public ReadOnlySpan<Entity> ActiveEntities => CollectionsMarshal.AsSpan(activeEntities);

    public ReadOnlySpan<Entity> LastRemovedEntities => CollectionsMarshal.AsSpan(tempRemove);

    public UITreeAdapter()
    {
        // Initialize all parent nodes to -1 and entities to Null
        Array.Fill(parentNode, -1);
        Array.Fill(nodeToEntity, Entity.Null);
        Array.Fill(entityMap, -1);
        for (int i = 0; i < InitialCapacity; i++)
            caches[i] = new LayoutCache();
    }

    void EnsureCapacity(int minCapacity)
    {
        if (minCapacity <= nodeCapacity) return;
        int newCap = Math.Max(nodeCapacity * 2, minCapacity);
        newCap = Math.Max(newCap, InitialCapacity);

        Grow(ref styles, newCap);
        Grow(ref layouts, newCap);
        Grow(ref unroundedLayouts, newCap);
        Grow(ref hasMeasure, newCap);
        Grow(ref nodeMeasureFuncs, newCap);
        Grow(ref parentNode, newCap);
        Grow(ref nodeToEntity, newCap);
        Grow(ref childOffsets, newCap);
        Grow(ref childCounts, newCap);

        var oldCaches = caches;
        caches = new LayoutCache[newCap];
        Array.Copy(oldCaches, caches, oldCaches.Length);

        // Initialize new slots
        for (int i = nodeCapacity; i < newCap; i++)
        {
            parentNode[i] = -1;
            nodeToEntity[i] = Entity.Null;
            caches[i] = new LayoutCache();
        }

        nodeCapacity = newCap;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void Grow<T>(ref T[] array, int newCap)
    {
        var old = array;
        array = new T[newCap];
        Array.Copy(old, array, old.Length);
    }

    void EnsureEntityMapCapacity(int minCapacity)
    {
        if (minCapacity <= entityMapCapacity) return;
        int newCap = Math.Max(entityMapCapacity * 2, minCapacity);
        var old = entityMap;
        entityMap = new int[newCap];
        Array.Copy(old, entityMap, old.Length);
        Array.Fill(entityMap, -1, old.Length, newCap - old.Length);
        entityMapCapacity = newCap;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool HasEntityMapping(Entity entity) =>
        entity.ID >= 0 && entity.ID < entityMapCapacity && entityMap[entity.ID] >= 0;

    int AllocateNode()
    {
        int id = nodeCapacity;
        EnsureCapacity(id + 1);
        styles[id] = Style.Default;
        layouts[id] = NodeLayout.Zero;
        unroundedLayouts[id] = NodeLayout.Zero;
        hasMeasure[id] = false;
        nodeMeasureFuncs[id] = null;
        caches[id] = new LayoutCache();
        parentNode[id] = -1;
        nodeToEntity[id] = Entity.Null;
        childOffsets[id] = 0;
        childCounts[id] = 0;
        nodeCapacity = id + 1;
        return id;
    }

    int AllocateOrReuseNode()
    {
        if (freeNodes.TryPop(out int nodeId))
        {
            styles[nodeId] = Style.Default;
            layouts[nodeId] = NodeLayout.Zero;
            unroundedLayouts[nodeId] = NodeLayout.Zero;
            hasMeasure[nodeId] = false;
            nodeMeasureFuncs[nodeId] = null;
            caches[nodeId] = new LayoutCache();
            parentNode[nodeId] = -1;
            nodeToEntity[nodeId] = Entity.Null;
            childCounts[nodeId] = 0;
            return nodeId;
        }

        return AllocateNode();
    }

    void FreeNode(int nodeId)
    {
        nodeToEntity[nodeId] = Entity.Null;
        childCounts[nodeId] = 0;
        caches[nodeId].Clear();
        hasMeasure[nodeId] = false;
        nodeMeasureFuncs[nodeId] = null;
        parentNode[nodeId] = -1;
        freeNodes.Push(nodeId);
    }

    public void SyncFull(Query<UINode> uiNodeQuery, Query query)
    {
        syncStats = default;
        SyncEntitySet(uiNodeQuery, query);
        RebuildHierarchy(query);
        CheckViewportResize(query);
    }

    void SyncEntitySet(Query<UINode> uiNodeQuery, Query query)
    {
        bool entitySetChanged = uiNodeQuery.EntityCount() != activeEntities.Count
            || query.AnyAdded<UINode>();
        bool anyStyleChanged = query.AnyChanged<UIStyle>();

        if (!entitySetChanged && !anyStyleChanged && !measureFuncsDirty)
            return;

        if (!entitySetChanged)
        {
            // Entity set unchanged — only sync styles and measure funcs
            var activeSpan = CollectionsMarshal.AsSpan(activeEntities);
            for (int i = 0; i < activeSpan.Length; i++)
                SyncExistingEntity(query, activeSpan[i]);
            measureFuncsDirty = false;
            return;
        }

        // Phase 1: Discover currently alive UINode entities
        tempAlive.Clear();
        tempAliveList.Clear();
        uiNodeQuery.ForEach((_tempAlive: tempAlive, _tempAliveList: tempAliveList),
            static (in (HashSet<Entity> set, List<Entity> list) ctx, in Entity entity, ref UINode _) =>
            {
                ctx.set.Add(entity);
                ctx.list.Add(entity);
            });

        // Phase 2: Remove nodes for despawned entities
        tempRemove.Clear();
        var prevSpan = CollectionsMarshal.AsSpan(activeEntities);
        for (int i = 0; i < prevSpan.Length; i++)
        {
            if (!tempAlive.Contains(prevSpan[i]))
                tempRemove.Add(prevSpan[i]);
        }

        foreach (var entity in tempRemove)
        {
            int nodeId = entityMap[entity.ID];

            int oldParentId = parentNode[nodeId];
            if (oldParentId >= 0)
                MarkDirtyWithAncestors(oldParentId);

            entityMap[entity.ID] = -1;
            entityMeasureFuncs.Remove(entity);
            lastRootSizes.Remove(entity);
            FreeNode(nodeId);
            syncStats.NodesRemoved++;
            hierarchyDirty = true;
        }

        // Phase 3+4: Add new entities and sync existing in one pass
        activeEntities.Clear();
        var aliveSpan = CollectionsMarshal.AsSpan(tempAliveList);
        for (int ai = 0; ai < aliveSpan.Length; ai++)
        {
            var entity = aliveSpan[ai];
            activeEntities.Add(entity);

            if (HasEntityMapping(entity))
            {
                SyncExistingEntity(query, entity);
                continue;
            }

            // New entity — allocate node and set initial state
            int nodeId = AllocateOrReuseNode();
            nodeToEntity[nodeId] = entity;
            EnsureEntityMapCapacity(entity.ID + 1);
            entityMap[entity.ID] = nodeId;

            if (query.Has<UIStyle>(entity))
                styles[nodeId] = query.Get<UIStyle>(entity).Value;

            if (query.Has<ContentSize>(entity)
                && entityMeasureFuncs.TryGetValue(entity, out var measureFunc))
            {
                hasMeasure[nodeId] = true;
                nodeMeasureFuncs[nodeId] = measureFunc;
            }

            MarkDirtyWithAncestors(nodeId);
            syncStats.NodesAdded++;
            hierarchyDirty = true;
        }

        measureFuncsDirty = false;
    }

    void RebuildHierarchy(Query query)
    {
        if (!hierarchyDirty) return;

        int capacity = nodeCapacity;
        var oldParents = ArrayPool<int>.Shared.Rent(capacity);
        try
        {
            Array.Copy(parentNode, oldParents, capacity);

            Array.Fill(parentNode, -1, 0, capacity);
            Array.Fill(childCounts, 0, 0, capacity);

            // Pass 1: count children per parent
            var activeSpan = CollectionsMarshal.AsSpan(activeEntities);
            for (int i = 0; i < activeSpan.Length; i++)
            {
                var entity = activeSpan[i];
                int nodeId = entityMap[entity.ID];
                if (!query.Has<Parent>(entity)) continue;

                ref readonly var parent = ref query.Get<Parent>(entity);
                var childEntity = parent.FirstChild;
                while (!childEntity.IsNull)
                {
                    if (HasEntityMapping(childEntity))
                        childCounts[nodeId]++;

                    if (!query.Has<Child>(childEntity)) break;
                    childEntity = query.Get<Child>(childEntity).NextSibling;
                }
            }

            // Compute offsets via prefix sum
            int totalChildren = 0;
            for (int i = 0; i < capacity; i++)
            {
                childOffsets[i] = totalChildren;
                totalChildren += childCounts[i];
            }

            if (childBuffer.Length < totalChildren)
                childBuffer = new int[Math.Max(totalChildren, 16)];

            Array.Fill(childCounts, 0, 0, capacity);

            // Pass 2: write child IDs into flat buffer
            for (int i = 0; i < activeSpan.Length; i++)
            {
                var entity = activeSpan[i];
                int nodeId = entityMap[entity.ID];
                if (!query.Has<Parent>(entity)) continue;

                ref readonly var parent = ref query.Get<Parent>(entity);
                var childEntity = parent.FirstChild;
                while (!childEntity.IsNull)
                {
                    if (HasEntityMapping(childEntity))
                    {
                        int childNodeId = entityMap[childEntity.ID];
                        childBuffer[childOffsets[nodeId] + childCounts[nodeId]] = childNodeId;
                        childCounts[nodeId]++;
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
                    parentNode[children[j]] = i;
            }

            // Detect hierarchy changes
            for (int i = 0; i < capacity; i++)
            {
                if (nodeToEntity[i].IsNull) continue;
                if (parentNode[i] != oldParents[i])
                {
                    if (parentNode[i] >= 0)
                        MarkDirtyWithAncestors(parentNode[i]);
                    if (oldParents[i] >= 0 && !nodeToEntity[oldParents[i]].IsNull)
                        MarkDirtyWithAncestors(oldParents[i]);
                    syncStats.HierarchyRebuilds++;
                }
            }
        }
        finally
        {
            ArrayPool<int>.Shared.Return(oldParents);
        }

        // Rebuild root list
        roots.Clear();
        var rootSpan = CollectionsMarshal.AsSpan(activeEntities);
        for (int i = 0; i < rootSpan.Length; i++)
        {
            var entity = rootSpan[i];
            int nodeId = entityMap[entity.ID];

            bool isRoot = !query.Has<Child>(entity)
                || query.Get<Child>(entity).Parent.IsNull
                || !HasEntityMapping(query.Get<Child>(entity).Parent);

            if (isRoot)
                roots.Add(nodeId);
        }

        hierarchyDirty = false;
    }

    void CheckViewportResize(Query query)
    {
        foreach (var rootNodeId in Roots)
        {
            var rootEntity = GetEntity(rootNodeId);
            if (!query.Has<UILayoutRoot>(rootEntity)) continue;

            ref readonly var layoutRoot = ref query.Get<UILayoutRoot>(rootEntity);
            if (CheckAndUpdateRootSize(rootEntity, layoutRoot.Size))
                MarkSubtreeDirty(rootNodeId);
        }
    }

    void SyncExistingEntity(Query query, Entity entity)
    {
        int nodeId = entityMap[entity.ID];

        if (query.Has<UIStyle>(entity) && query.Changed<UIStyle>(entity))
        {
            styles[nodeId] = query.Get<UIStyle>(entity).Value;
            MarkDirtyWithAncestors(nodeId);
            syncStats.StylesCopied++;
        }

        entityMeasureFuncs.TryGetValue(entity, out var mf);
        bool shouldHaveMeasure = query.Has<ContentSize>(entity) && mf is not null;
        if (shouldHaveMeasure != hasMeasure[nodeId])
        {
            hasMeasure[nodeId] = shouldHaveMeasure;
            nodeMeasureFuncs[nodeId] = shouldHaveMeasure ? mf : null;
            MarkDirtyWithAncestors(nodeId);
        }
        else if (shouldHaveMeasure && nodeMeasureFuncs[nodeId] != mf)
        {
            nodeMeasureFuncs[nodeId] = mf;
            MarkDirtyWithAncestors(nodeId);
        }
    }

    void MarkDirtyWithAncestors(int nodeId)
    {
        globalDirty = true;
        while (nodeId >= 0)
        {
            if (!caches[nodeId].IsDirty)
                caches[nodeId].Clear();
            nodeId = parentNode[nodeId];
        }
    }

    /// Marks a node and all its descendants as dirty (used for viewport resize).
    public void MarkSubtreeDirty(int nodeId)
    {
        globalDirty = true;
        caches[nodeId].Clear();
        var children = GetChildIds(nodeId);
        for (int i = 0; i < children.Length; i++)
            MarkSubtreeDirty(children[i]);
    }

    /// Checks if the root viewport size changed since last layout.
    public bool CheckAndUpdateRootSize(Entity rootEntity, Size<float> newSize)
    {
        if (lastRootSizes.TryGetValue(rootEntity, out var lastSize)
            && lastSize.Width == newSize.Width && lastSize.Height == newSize.Height)
        {
            return false;
        }

        lastRootSizes[rootEntity] = newSize;
        return true;
    }

    public Entity GetEntity(int nodeId) => nodeToEntity[nodeId];

    public ref NodeLayout GetRoundedLayout(int nodeId) =>
        ref layouts[nodeId];

    public ref NodeLayout GetUnroundedLayout(int nodeId) =>
        ref unroundedLayouts[nodeId];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetNodeId(Entity entity) =>
        entity.ID >= 0 && entity.ID < entityMapCapacity ? entityMap[entity.ID] : -1;

    public int NodeCapacity => nodeCapacity;
    public int ActiveNodeCount => activeEntities.Count;

    public int ChildCount(int nodeId) => childCounts[nodeId];

    public ReadOnlySpan<int> GetChildIds(int nodeId) =>
        childBuffer.AsSpan(childOffsets[nodeId], childCounts[nodeId]);

    public ref readonly Style GetStyle(int nodeId) =>
        ref styles[nodeId];

    public void SetUnroundedLayout(int nodeId, in NodeLayout layout)
    {
        unroundedLayouts[nodeId] = layout;
        layouts[nodeId] = layout;
    }

    public ref NodeLayout GetLayout(int nodeId) =>
        ref layouts[nodeId];

    public LayoutOutput ComputeChildLayout(int nodeId, in LayoutInput input)
    {
        var treeRef = new UITreeRef(this);
        return FlexLayout.ComputeFlexbox(ref treeRef, nodeId, input);
    }

    public void SetMeasureFunc(Entity entity, MeasureFunc func)
    {
        entityMeasureFuncs[entity] = func;
        measureFuncsDirty = true;
    }

    public void RemoveMeasureFunc(Entity entity)
    {
        entityMeasureFuncs.Remove(entity);
        measureFuncsDirty = true;
    }

    public bool HasMeasureFunc(int nodeId) => hasMeasure[nodeId];

    public LayoutOutput Measure(int nodeId, in LayoutInput input)
    {
        var func = nodeMeasureFuncs[nodeId];
        if (func is null) return LayoutOutput.Zero;
        var size = func(input.KnownDimensions, input.AvailableSpace);
        return new LayoutOutput { Size = size };
    }

    public bool TryCacheGet(int nodeId, in LayoutInput input, out LayoutOutput output)
        => caches[nodeId].TryGet(in input, out output);

    public void CacheStore(int nodeId, in LayoutInput input, in LayoutOutput output)
        => caches[nodeId].Store(in input, in output);

    public void MarkDirty(int nodeId)
        => caches[nodeId].Clear();
}
