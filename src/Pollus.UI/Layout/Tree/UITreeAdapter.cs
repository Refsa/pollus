using System.Runtime.InteropServices;
using Pollus.ECS;

namespace Pollus.UI.Layout;

public class UITreeAdapter : ILayoutTree
{
    readonly List<Style> _styles = [];
    readonly List<List<int>> _children = [];
    readonly List<NodeLayout> _layouts = [];
    readonly List<NodeLayout> _unroundedLayouts = [];
    readonly List<bool> _hasMeasure = [];

    readonly List<Entity> _nodeToEntity = [];
    readonly Dictionary<Entity, int> _entityToNode = [];

    readonly List<int> _roots = [];

    public ReadOnlySpan<int> Roots => CollectionsMarshal.AsSpan(_roots);

    int AllocateNode()
    {
        int id = _styles.Count;
        _styles.Add(Style.Default);
        _children.Add([]);
        _layouts.Add(NodeLayout.Zero);
        _unroundedLayouts.Add(NodeLayout.Zero);
        _hasMeasure.Add(false);
        _nodeToEntity.Add(Entity.Null);
        return id;
    }

    public void SyncFull(Query query)
    {
        // Clear all existing state
        _entityToNode.Clear();
        _roots.Clear();
        _styles.Clear();
        _children.Clear();
        _layouts.Clear();
        _unroundedLayouts.Clear();
        _hasMeasure.Clear();
        _nodeToEntity.Clear();

        // Allocate nodes for all UINode entities and copy styles
        query.ForEach((in Entity entity) =>
        {
            if (!query.Has<UINode>(entity)) return;

            int nodeId = AllocateNode();
            _nodeToEntity[nodeId] = entity;
            _entityToNode[entity] = nodeId;

            if (query.Has<UIStyle>(entity))
            {
                _styles[nodeId] = query.Get<UIStyle>(entity).Value;
            }
        });

        // Build children arrays from hierarchy
        foreach (var (entity, nodeId) in _entityToNode)
        {
            if (!query.Has<Parent>(entity)) continue;

            ref readonly var parent = ref query.Get<Parent>(entity);
            var childEntity = parent.FirstChild;
            while (!childEntity.IsNull)
            {
                if (_entityToNode.TryGetValue(childEntity, out int childNodeId))
                {
                    _children[nodeId].Add(childNodeId);
                }

                if (!query.Has<Child>(childEntity)) break;
                childEntity = query.Get<Child>(childEntity).NextSibling;
            }
        }

        // Detect roots: UINode entities with no Child component,
        // or whose Child.Parent doesn't have UINode
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
                {
                    isRoot = true;
                }
                else
                {
                    isRoot = !_entityToNode.ContainsKey(child.Parent);
                }
            }

            if (isRoot)
            {
                _roots.Add(nodeId);
            }
        }
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

    public bool HasMeasureFunc(int nodeId) => _hasMeasure[nodeId];

    public LayoutOutput Measure(int nodeId, in LayoutInput input) => LayoutOutput.Zero;
}
