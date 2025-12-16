namespace Pollus.ECS;

public record struct Parent : IComponent
{
    public Entity FirstChild = Entity.NULL;
    public Entity LastChild = Entity.NULL;
    public int ChildCount = 0;

    public Parent()
    {
    }
}

public record struct Child : IComponent
{
    public Entity Parent = Entity.NULL;
    public Entity NextSibling = Entity.NULL;
    public Entity PreviousSibling = Entity.NULL;

    public Child()
    {
    }
}

public class HierarchyPlugin : IPlugin
{
    static HierarchyPlugin()
    {
        Component.Register<Parent>();
        Component.Register<Child>();
    }

    public void Apply(World world)
    {
        world.Schedule.AddSystems(CoreStage.Last, FnSystem.Create(new("Hierarchy::Maintenance"),
            static (
                Commands commands,
                Query query,
                RemovedTracker<Parent> parentRemovedTracker,
                RemovedTracker<Child> childRemovedTracker,
                Query.Filter<Removed<Child>> qChildRemoved,
                Query.Filter<Removed<Parent>> qParentRemoved) =>
            {
                foreach (var child in childRemovedTracker)
                {
                    ref var parent = ref query.Get<Parent>(child.Component.Parent);
                    parent.ChildCount--;
                }

                foreach (var parent in parentRemovedTracker)
                {
                    var childEntity = parent.Component.FirstChild;
                    while (childEntity.IsNull is false)
                    {
                        commands.RemoveComponent<Child>(childEntity);
                        childEntity = query.Get<Child>(childEntity).NextSibling;
                    }
                }
            }));
    }
}

public ref struct HierarchyEnumerator
{
    readonly Query query;
    readonly Entity root;
    CurrentEntity currentEntity;
    public CurrentEntity Current => currentEntity;

    public HierarchyEnumerator(Entity root, Query query)
    {
        this.query = query;
        this.root = root;
        currentEntity = new CurrentEntity(root, 0);
    }

    public bool MoveNext()
    {
        if (currentEntity.Entity == Entity.NULL) return false;
        var entityRef = query.GetEntity(currentEntity.Entity);

        if (entityRef.Has<Parent>())
        {
            currentEntity.Entity = entityRef.Get<Parent>().FirstChild;
            currentEntity.Depth++;
            return true;
        }

        ref var cChild = ref entityRef.Get<Child>();
        if (cChild.NextSibling != Entity.NULL)
        {
            currentEntity.Entity = cChild.NextSibling;
            return true;
        }

        if (cChild.Parent == root) return false;

        currentEntity.Entity = query.Get<Child>(cChild.Parent).NextSibling;
        currentEntity.Depth--;
        return currentEntity.Entity != Entity.NULL;
    }

    public ref struct CurrentEntity(Entity entity, int depth)
    {
        public Entity Entity = entity;
        public int Depth = depth;
    }
}

public ref struct HierarchyEnumerable(Query Query, Entity Root)
{
    public HierarchyEnumerator GetEnumerator() => new(Root, Query);
}

public static class HierarchyQueryExt
{
    public static HierarchyEnumerable HierarchyDFS(this Query query, in Entity root)
    {
        return new HierarchyEnumerable(query, root);
    }
}