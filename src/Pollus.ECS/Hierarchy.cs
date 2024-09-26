namespace Pollus.ECS;

using System.Runtime.CompilerServices;

public record struct Parent : IComponent
{
    public Entity FirstChild = Entity.NULL;
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
        static (Commands commands, Query.Filter<Removed<Child>> qChildRemoved, Query.Filter<Removed<Parent>> qParentRemoved) =>
        {
            // TODO: We dont currently track the underlying removed components
            // either they need to be tracked or the user needs to use the hierarchy commands
            // to remove the child and parent relationships

            /* foreach (var child in qChildRemoved)
            {
                commands.RemoveChild(child.Parent, child);
            } */

            /* foreach (var parent in qParentRemoved)
            {
                commands.RemoveChildren(parent);
            } */
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

public static class HierarchyCommandsExt
{
    /// <summary>
    /// Sets the parent of the given child.
    /// </summary>
    /// <param name="commands">The commands instance to add the command to.</param>
    /// <param name="child">The child entity.</param>
    /// <param name="parent">The parent entity.</param>
    /// <returns>The commands instance.</returns>
    public static Commands SetParent(this Commands commands, in Entity child, in Entity parent)
    {
        commands.AddCommand(new AddChildCommand { Child = child, Parent = parent });
        return commands;
    }

    /// <summary>
    /// Adds the given child to the given parent.
    /// </summary>
    /// <param name="commands">The commands instance to add the command to.</param>
    /// <param name="parent">The parent entity.</param>
    /// <param name="child">The child entity.</param>
    /// <returns>The commands instance.</returns>
    public static Commands AddChild(this Commands commands, in Entity parent, in Entity child)
    {
        commands.AddCommand(new AddChildCommand { Child = child, Parent = parent });
        return commands;
    }

    /// <summary>
    /// Removes the given child from the given parent. Does not despawn the child.
    /// </summary>
    /// <param name="commands">The commands instance to add the command to.</param>
    /// <param name="parent">The parent entity.</param>
    /// <param name="child">The child entity.</param>
    /// <returns>The commands instance.</returns>
    public static Commands RemoveChild(this Commands commands, in Entity parent, in Entity child)
    {
        commands.AddCommand(new RemoveChildCommand { Parent = parent, Child = child });
        return commands;
    }

    /// <summary>
    /// Removes all children of the given parent. Does not despawn them.
    /// </summary>
    /// <param name="commands">The commands instance to add the command to.</param>
    /// <param name="parent">The parent entity.</param>
    /// <returns>The commands instance.</returns>
    public static Commands RemoveChildren(this Commands commands, in Entity parent)
    {
        commands.AddCommand(new RemoveChildrenCommand { Parent = parent });
        return commands;
    }

    /// <summary>
    /// Despawns the hierarchy rooted at the given entity.
    /// </summary>
    /// <param name="commands">The commands instance to add the command to.</param>
    /// <param name="root"></param>
    /// <returns></returns>
    public static Commands DespawnHierarchy(this Commands commands, in Entity root)
    {
        commands.AddCommand(new DespawnHierarchyCommand { Root = root });
        return commands;
    }
}

public struct AddChildCommand : ICommand
{
    public required Entity Parent;
    public required Entity Child;

    public void Execute(World world)
    {
        if (!world.Store.HasComponent<Parent>(Parent))
            world.Store.AddComponent(Parent, new Parent());

        if (!world.Store.HasComponent<Child>(Child))
            world.Store.AddComponent(Child, new Child());

        ref var cParent = ref world.Store.GetComponent<Parent>(Parent);
        ref var cChild = ref world.Store.GetComponent<Child>(Child);

        cChild.Parent = Parent;

        if (cParent.FirstChild != Entity.NULL)
        {
            cChild.NextSibling = cParent.FirstChild;
            world.Store.GetComponent<Child>(cParent.FirstChild).PreviousSibling = Child;
        }

        cParent.FirstChild = Child;
    }
}

public struct RemoveChildCommand : ICommand
{
    public required Entity Parent;
    public required Entity Child;

    public void Execute(World world)
    {
        if (!world.Store.HasComponent<Parent>(Parent))
            return;

        if (!world.Store.HasComponent<Child>(Child))
            return;

        ref var cParent = ref world.Store.GetComponent<Parent>(Parent);
        ref var cChild = ref world.Store.GetComponent<Child>(Child);

        if (cParent.FirstChild == Child)
        {
            cParent.FirstChild = cChild.NextSibling;

            if (cChild.NextSibling != Entity.NULL)
            {
                world.Store.GetComponent<Child>(cChild.NextSibling).PreviousSibling = Entity.NULL;
            }
        }
        else
        {
            var current = cParent.FirstChild;
            while (current != Child)
            {
                var next = world.Store.GetComponent<Child>(current).NextSibling;
                if (next == Child)
                {
                    world.Store.GetComponent<Child>(current).NextSibling = cChild.NextSibling;
                    world.Store.GetComponent<Child>(cChild.NextSibling).PreviousSibling = current;
                }
                current = next;
            }
        }

        world.Store.RemoveComponent<Child>(Child);
    }
}

public struct RemoveChildrenCommand : ICommand
{
    public required Entity Parent;

    public void Execute(World world)
    {
        if (!world.Store.HasComponent<Parent>(Parent))
            return;

        ref var cParent = ref world.Store.GetComponent<Parent>(Parent);
        var current = cParent.FirstChild;
        while (current != Entity.NULL)
        {
            world.Store.RemoveComponent<Child>(current);
            current = world.Store.GetComponent<Child>(current).NextSibling;
        }

        world.Store.RemoveComponent<Parent>(Parent);
    }
}

public struct DespawnHierarchyCommand : ICommand
{
    public required Entity Root;

    public void Execute(World world)
    {
        DespawnDFS(world, Root);
    }

    void DespawnDFS(World world, in Entity entity)
    {
        if (world.Store.HasComponent<Parent>(entity))
        {
            var current = world.Store.GetComponent<Parent>(entity).FirstChild;
            while (current != Entity.NULL)
            {
                var down = current;
                current = world.Store.GetComponent<Child>(current).NextSibling;
                DespawnDFS(world, down);
            }
        }

        world.Despawn(entity);
    }
}

