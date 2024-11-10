using Pollus.Collections;

namespace Pollus.ECS;

public interface ICommand
{
    static abstract int Priority { get; }
    void Execute(World world);
}

public interface ICommandBuffer
{
    int Priority { get; }
    int Count { get; }

    void Clear();
    void Execute(World world);
}

public class CommandBuffer<TCommand> : ICommandBuffer
    where TCommand : ICommand
{
    TCommand[] commands = new TCommand[1];
    int count = 0;

    public int Priority => TCommand.Priority;
    public int Count => count;

    public void Clear()
    {
        count = 0;
    }

    public void AddCommand(TCommand command)
    {
        if (count == commands.Length)
            Array.Resize(ref commands, commands.Length * 2);

        commands[count++] = command;
    }

    public void Execute(World world)
    {
        foreach (ref var command in commands.AsSpan(0, count))
        {
            command.Execute(world);
        }
    }
}

public class Commands
{
    bool needsSort = false;
    readonly Entities entities;
    readonly List<ICommandBuffer> commandBuffers = [];
    readonly Dictionary<Type, int> commandBuffersLookup = [];

    public Commands()
    {
        throw new InvalidOperationException("Commands cannot be instantiated directly. Use World.GetCommands() instead.");
    }

    public Commands(Entities entities)
    {
        this.entities = entities;
    }

    public void AddCommand<TCommand>(in TCommand command)
        where TCommand : ICommand
    {
        ICommandBuffer? buffer;
        if (commandBuffersLookup.TryGetValue(typeof(TCommand), out var idx))
        {
            buffer = commandBuffers[idx];
        }
        else
        {
            buffer = new CommandBuffer<TCommand>();
            commandBuffers.Add(buffer);
            commandBuffersLookup.Add(typeof(TCommand), commandBuffers.Count - 1);
            needsSort = true;
        }

        ((CommandBuffer<TCommand>)buffer).AddCommand(command);
    }

    public void Flush(World world)
    {
        if (needsSort)
        {
            needsSort = false;
            commandBuffers.Sort(static (a, b) => b.Priority.CompareTo(a.Priority));
            commandBuffersLookup.Clear();
            for (int i = 0; i < commandBuffers.Count; i++)
            {
                commandBuffersLookup[commandBuffers[i].GetType()] = i;
            }
        }

        foreach (var buffer in new ListEnumerable<ICommandBuffer>(commandBuffers))
        {
            if (buffer.Count == 0) continue;

            buffer.Execute(world);
            buffer.Clear();
        }
    }

    public Entity Spawn<TBuilder>(in TBuilder builder)
        where TBuilder : IEntityBuilder
    {
        var entity = entities.Create();
        AddCommand(SpawnEntityCommand<TBuilder>.From(builder, entity));
        return entity;
    }

    public Entity Spawn()
    {
        var entity = entities.Create();
        AddCommand(SpawnEntityCommand<EntityBuilder>.From(new EntityBuilder(), entity));
        return entity;
    }

    public Commands Despawn(in Entity entity)
    {
        AddCommand(DespawnEntityCommand.From(entity));
        return this;
    }

    public Commands AddComponent<C>(in Entity entity, in C component)
        where C : unmanaged, IComponent
    {
        AddCommand(AddComponentCommand<C>.From(entity, component));
        return this;
    }

    public Commands RemoveComponent<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        AddCommand(RemoveComponentCommand<C>.From(entity));
        return this;
    }

    public Commands Defer(Action<World> action)
    {
        AddCommand(DelegateCommand.From(action));
        return this;
    }
}

public class CommandsFetch : IFetch<Commands>
{
    public static void Register()
    {
        Fetch.Register(new CommandsFetch(), []);
    }

    public Commands DoFetch(World world, ISystem system)
    {
        return world.GetCommands();
    }
}

public struct SpawnEntityCommand<TBuilder> : ICommand
    where TBuilder : IEntityBuilder
{
    public static int Priority => 100;

    TBuilder builder;
    Entity entity;

    public static SpawnEntityCommand<TBuilder> From(in TBuilder builder, in Entity entity)
    {
        return new SpawnEntityCommand<TBuilder>()
        {
            builder = builder,
            entity = entity
        };
    }

    public void Execute(World world)
    {
        builder.Spawn(world, entity);
    }
}

public struct DespawnEntityCommand : ICommand
{
    public static int Priority => 0;

    Entity entity;

    public static DespawnEntityCommand From(Entity entity)
    {
        return new DespawnEntityCommand()
        {
            entity = entity
        };
    }

    public void Execute(World world)
    {
        world.Despawn(entity);
    }
}

public struct AddComponentCommand<C> : ICommand
    where C : unmanaged, IComponent
{
    public static int Priority => 90;

    Entity entity;
    C component;

    public static AddComponentCommand<C> From(in Entity entity, in C component)
    {
        return new AddComponentCommand<C>()
        {
            entity = entity,
            component = component
        };
    }

    public void Execute(World world)
    {
        world.Store.AddComponent(entity, component);
    }
}

public struct RemoveComponentCommand<C> : ICommand
    where C : unmanaged, IComponent
{
    public static int Priority => 90;

    Entity entity;

    public static RemoveComponentCommand<C> From(in Entity entity)
    {
        return new RemoveComponentCommand<C>()
        {
            entity = entity
        };
    }

    public void Execute(World world)
    {
        world.Store.RemoveComponent<C>(entity);
    }
}

public struct DelegateCommand : ICommand
{
    public static int Priority => 0;

    Action<World> action;

    public static DelegateCommand From(Action<World> action)
    {
        return new DelegateCommand()
        {
            action = action
        };
    }

    public void Execute(World world)
    {
        action(world);
    }
}