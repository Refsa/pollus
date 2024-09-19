namespace Pollus.ECS;

public interface ICommand
{
    void Execute(World world);
}

public interface ICommandBuffer
{
    void Clear();
    void Execute(World world);
}

public class CommandBuffer<TCommand> : ICommandBuffer
    where TCommand : ICommand
{
    TCommand[] commands = new TCommand[1];
    int count = 0;

    public void Clear()
    {
        count = 0;
    }

    public void AddCommand(TCommand command)
    {
        if (count == commands.Length)
        {
            var newCommands = new TCommand[commands.Length * 2];
            commands.CopyTo(newCommands, 0);
            commands = newCommands;
        }

        commands[count++] = command;
    }

    public void Execute(World world)
    {
        foreach (var command in commands.AsSpan(0, count))
        {
            command.Execute(world);
        }
    }
}

public class Commands
{
    Dictionary<Type, ICommandBuffer> commandBuffers = [];

    public void AddCommand<TCommand>(in TCommand command)
        where TCommand : ICommand
    {
        if (!commandBuffers.TryGetValue(typeof(TCommand), out var buffer))
        {
            buffer = new CommandBuffer<TCommand>();
            commandBuffers.Add(typeof(TCommand), buffer);
        }

        ((CommandBuffer<TCommand>)buffer).AddCommand(command);
    }

    public void Flush(World world)
    {
        foreach (var buffer in commandBuffers.Values)
        {
            if (buffer is ICommandBuffer commandBuffer)
            {
                commandBuffer.Execute(world);
                commandBuffer.Clear();
            }
        }
    }

    public Commands Spawn<TBuilder>(in TBuilder builder)
        where TBuilder : IEntityBuilder
    {
        AddCommand(SpawnEntityCommand<TBuilder>.From(builder));
        return this;
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
    TBuilder builder;

    public static SpawnEntityCommand<TBuilder> From(in TBuilder builder)
    {
        return new SpawnEntityCommand<TBuilder>()
        {
            builder = builder
        };
    }

    public void Execute(World world)
    {
        builder.Spawn(world);
    }
}

public struct DespawnEntityCommand : ICommand
{
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