namespace Pollus.ECS;

public record struct Parent : IComponent
{
    public Entity FirstChild = Entity.NULL;
    public Entity LastChild = Entity.NULL;
    public Parent() { }
}

public record struct Child : IComponent
{
    public Entity Parent = Entity.NULL;
    public Entity NextSibling = Entity.NULL;
    public Entity PreviousSibling = Entity.NULL;
    public Child() { }
}

public class HierarchyPlugin : IPlugin
{
    public void Apply(World world)
    {
        world.Schedule.AddSystems(CoreStage.Last, FnSystem.Create(new("Hierarchy::Maintenance"),
        static (
            Commands commands,
            RemovedTracker<Child> childRemovedTracker,
            Query.Filter<Removed<Child>> qChildRemoved,
            Query.Filter<Removed<Parent>> qParentRemoved) =>
        {
            foreach (var child in qChildRemoved)
            {
                if (childRemovedTracker.WasRemoved(child))
                {
                    var parent = childRemovedTracker.GetRemoved(child).Parent;
                    commands.RemoveChild(parent, child);
                }
            }

            foreach (var parent in qParentRemoved)
            {
                commands.RemoveChildren(parent);
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

        if (query.Has<Parent>(currentEntity.Entity))
        {
            currentEntity.Entity = query.Get<Parent>(currentEntity.Entity).FirstChild;
            currentEntity.Depth++;
            return true;
        }

        ref var cChild = ref query.Get<Child>(currentEntity.Entity);
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