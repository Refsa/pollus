namespace Pollus.ECS;

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
        commands.AddChild(parent, child);
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

    public static EntityCommands SetParent(this EntityCommands builder, in Entity parent)
    {
        builder.Commands.SetParent(builder.Entity, parent);
        return builder;
    }

    public static EntityCommands AddChild(this EntityCommands builder, in Entity child)
    {
        builder.Commands.AddChild(builder.Entity, child);
        return builder;
    }

    public static EntityCommands AddChildren(this EntityCommands builder, params Span<Entity> children)
    {
        for (int i = 0; i < children.Length; i++)
        {
            builder.AddChild(children[i]);
        }

        return builder;
    }

    public static EntityCommands RemoveChild(this EntityCommands builder, in Entity child)
    {
        builder.Commands.RemoveChild(builder.Entity, child);
        return builder;
    }

    public static EntityCommands RemoveChildren(this EntityCommands builder)
    {
        builder.Commands.RemoveChildren(builder.Entity);
        return builder;
    }
}

public struct AddChildCommand : ICommand
{
    public static int Priority => 20;

    public required Entity Parent;
    public required Entity Child;

    public void Execute(World world)
    {
        if (!world.Store.HasComponent<Parent>(Parent))
        {
            world.Store.AddComponent(Parent, new Parent { FirstChild = Entity.NULL, LastChild = Entity.NULL });
        }

        if (!world.Store.HasComponent<Child>(Child))
        {
            world.Store.AddComponent(Child, new Child { Parent = Entity.NULL });
        }

        ref var cParent = ref world.Store.GetComponent<Parent>(Parent);
        ref var cChild = ref world.Store.GetComponent<Child>(Child);

        cParent.ChildCount++;
        cChild.Parent = Parent;
        cChild.NextSibling = Entity.NULL;
        cChild.PreviousSibling = Entity.NULL;

        if (cParent.FirstChild.IsNull)
        {
            cParent.FirstChild = Child;
            cParent.LastChild = Child;
        }
        else
        {
            var lastChild = cParent.LastChild;
            cChild.PreviousSibling = lastChild;
            world.Store.GetComponent<Child>(lastChild).NextSibling = Child;
            cParent.LastChild = Child;
        }
    }
}

public struct RemoveChildCommand : ICommand
{
    public static int Priority => AddChildCommand.Priority - 1;

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

        if (cChild.Parent != Parent) return;

        cParent.ChildCount--;
        if (cParent.FirstChild == Child)
        {
            cParent.FirstChild = cChild.NextSibling;
            if (cChild.NextSibling.IsNull)
            {
                cParent.LastChild = Entity.NULL;
            }
            else
            {
                world.Store.GetComponent<Child>(cChild.NextSibling).PreviousSibling = Entity.NULL;
            }
        }
        else
        {
            if (!cChild.PreviousSibling.IsNull)
            {
                world.Store.GetComponent<Child>(cChild.PreviousSibling).NextSibling = cChild.NextSibling;
            }

            if (!cChild.NextSibling.IsNull)
            {
                world.Store.GetComponent<Child>(cChild.NextSibling).PreviousSibling = cChild.PreviousSibling;
            }
            else
            {
                cParent.LastChild = cChild.PreviousSibling;
            }
        }

        world.Store.RemoveComponent<Child>(Child);
    }
}

public struct RemoveChildrenCommand : ICommand
{
    public static int Priority => AddChildCommand.Priority - 1;

    public required Entity Parent;

    public void Execute(World world)
    {
        if (!world.Store.HasComponent<Parent>(Parent))
            return;

        ref var cParent = ref world.Store.GetComponent<Parent>(Parent);
        cParent.ChildCount = 0;

        var current = cParent.FirstChild;
        while (!current.IsNull)
        {
            world.Store.RemoveComponent<Child>(current);
            current = world.Store.GetComponent<Child>(current).NextSibling;
        }

        world.Store.RemoveComponent<Parent>(Parent);
    }
}

public struct DespawnHierarchyCommand : ICommand
{
    public static int Priority => AddChildCommand.Priority - 1;

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
            while (!current.IsNull)
            {
                var down = current;
                current = world.Store.GetComponent<Child>(current).NextSibling;
                DespawnDFS(world, down);
            }
        }

        world.Despawn(entity);
    }
}
