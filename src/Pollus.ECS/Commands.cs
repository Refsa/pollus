namespace Pollus.ECS;

using Pollus.Collections;
using Pollus.Debugging;

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

    public ref TCommand AddCommand(in TCommand command)
    {
        if (count == commands.Length)
            Array.Resize(ref commands, commands.Length * 2);

        commands[count++] = command;
        return ref commands[count - 1];
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
    readonly record struct BufferEntry(ICommandBuffer Buffer, Type BufferType);

    bool needsSort = false;
    readonly Entities entities;
    readonly List<BufferEntry> commandBuffers = [];
    readonly Dictionary<Type, int> commandBuffersLookup = [];

    internal int CommandBufferCount => commandBuffers.Count;

    public Commands()
    {
        throw new InvalidOperationException("Commands cannot be instantiated directly. Use World.GetCommands() instead.");
    }

    public Commands(Entities entities)
    {
        this.entities = entities;
    }

    public ref TCommand AddCommand<TCommand>(in TCommand command)
        where TCommand : ICommand
    {
        ICommandBuffer? buffer;
        if (commandBuffersLookup.TryGetValue(typeof(TCommand), out var idx))
        {
            buffer = commandBuffers[idx].Buffer;
        }
        else
        {
            buffer = new CommandBuffer<TCommand>();
            commandBuffers.Add(new BufferEntry(buffer, typeof(TCommand)));
            commandBuffersLookup.Add(typeof(TCommand), commandBuffers.Count - 1);
            needsSort = true;
        }

        return ref ((CommandBuffer<TCommand>)buffer).AddCommand(command);
    }

    public void Flush(World world)
    {
        if (needsSort)
        {
            needsSort = false;
            commandBuffers.Sort(static (a, b) => b.Buffer.Priority.CompareTo(a.Buffer.Priority));
            commandBuffersLookup.Clear();
            for (int i = 0; i < commandBuffers.Count; i++)
            {
                commandBuffersLookup[commandBuffers[i].BufferType] = i;
            }
        }

        foreach (var buffer in new ListEnumerable<BufferEntry>(commandBuffers))
        {
            if (buffer.Buffer.Count == 0) continue;

            buffer.Buffer.Execute(world);
            buffer.Buffer.Clear();
        }
    }

    public EntityCommands Entity(in Entity entity)
    {
        return new EntityCommands(this, entity);
    }

    public EntityCommands Spawn<TBuilder>(in TBuilder builder)
        where TBuilder : IEntityBuilder
    {
        var entity = entities.Create();
        AddCommand(SpawnEntityCommand<TBuilder>.From(builder, entity));
        return new EntityCommands(this, entity);
    }

    public EntityCommands Spawn()
    {
        var entity = entities.Create();
        AddCommand(SpawnEntityCommand<EntityBuilder>.From(new EntityBuilder(), entity));
        return new EntityCommands(this, entity);
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

    public Commands AddComponent(in Entity entity, in ComponentID componentID, in byte[] data)
    {
        AddCommand(AddComponentCommand.From(entity, componentID, data));
        return this;
    }

    public Commands RemoveComponent<C>(in Entity entity)
        where C : unmanaged, IComponent
    {
        AddCommand(RemoveComponentCommand<C>.From(entity));
        return this;
    }

    public Commands SetComponent<C>(in Entity entity, in C component)
        where C : unmanaged, IComponent
    {
        AddCommand(SetComponentCommand<C>.From(entity, component));
        return this;
    }

    public EntityCommands Clone(in Entity entity)
    {
        var cloned = entities.Create();
        AddCommand(CloneEntityCommand.From(entity, cloned));
        return new EntityCommands(this, cloned);
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

public struct EntityCommands
{
    Commands commands;
    public readonly Entity Entity;
    public Commands Commands => commands;

    public EntityCommands(Commands commands, Entity entity)
    {
        this.commands = commands;
        Entity = entity;
    }

    public EntityCommands AddComponent<C>(in C component)
        where C : unmanaged, IComponent
    {
        commands.AddComponent(Entity, component);
        return this;
    }

    public EntityCommands AddComponent(in ComponentID componentID, in byte[] data)
    {
        commands.AddComponent(Entity, componentID, data);
        return this;
    }

    public EntityCommands RemoveComponent<C>()
        where C : unmanaged, IComponent
    {
        commands.RemoveComponent<C>(Entity);
        return this;
    }

    public EntityCommands SetComponent<C>(in C component)
        where C : unmanaged, IComponent
    {
        commands.SetComponent(Entity, component);
        return this;
    }

    public EntityCommands Despawn()
    {
        commands.Despawn(Entity);
        return this;
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

public struct SetComponentCommand<C> : ICommand
    where C : unmanaged, IComponent
{
    public static int Priority => 70;

    Entity entity;
    C component;

    public static SetComponentCommand<C> From(in Entity entity, in C component)
    {
        return new SetComponentCommand<C>() { entity = entity, component = component };
    }

    public void Execute(World world)
    {
        Guard.IsTrue(world.Store.HasComponent<C>(entity), $"Entity {entity} does not have component {typeof(C)}");
        world.Store.SetComponent(entity, component);
    }
}

public struct AddComponentCommand : ICommand
{
    public static int Priority => 90;

    Entity entity;
    ComponentID componentID;
    byte[] data;

    public static AddComponentCommand From(in Entity entity, in ComponentID componentID, in byte[] data)
    {
        return new AddComponentCommand()
        {
            entity = entity,
            componentID = componentID,
            data = data
        };
    }

    public void Execute(World world)
    {
        world.Store.AddComponent(entity, componentID, data);
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

public struct CloneEntityCommand : ICommand
{
    public static int Priority => 0;

    Entity entity;
    Entity cloned;

    public static CloneEntityCommand From(in Entity entity, in Entity cloned)
    {
        return new CloneEntityCommand() { entity = entity, cloned = cloned };
    }

    public void Execute(World world)
    {
        world.Clone(entity, cloned);
    }
}