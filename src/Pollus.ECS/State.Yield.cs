namespace Pollus.ECS;

using Pollus.Coroutine;
using Pollus.Utils;

public struct WaitForStateEnter<TState>
    where TState : unmanaged, Enum
{
    public static readonly Type[] Dependencies = [typeof(StateEvent<TState>)];
    static WaitForStateEnter()
    {
        Coroutine.RegisterHandler<WaitForStateEnter<TState>>(
        static (in Yield yield, Param<World> param) =>
        {
            var handler = yield.GetCustomData<WaitForStateEnter<TState>>();
            return handler.Execute(param);
        }, Dependencies);
    }

    public TState State { get; }
    public WaitForStateEnter(TState state)
    {
        State = state;
    }

    public bool Execute(Param<World> param)
    {
        (var world, _) = param;

        foreach (var ev in world.Events.ReadEvents<StateEvent<TState>>())
        {
            if (EqualityComparer<TState>.Default.Equals(ev.State, State) && ev.Transition == StateTransition.Enter)
            {
                return true;
            }
        }

        return false;
    }
}

public struct WaitForStateExit<TState>
    where TState : unmanaged, Enum
{
    public static readonly Type[] Dependencies = [typeof(StateEvent<TState>)];

    static WaitForStateExit()
    {
        Coroutine.RegisterHandler<WaitForStateExit<TState>>(
        static (in Yield yield, Param<World> param) =>
        {
            var handler = yield.GetCustomData<WaitForStateExit<TState>>();
            return handler.Execute(param);
        }, Dependencies);
    }

    public TState State { get; }
    public WaitForStateExit(TState state)
    {
        State = state;
    }

    public bool Execute(Param<World> param)
    {
        (var world, _) = param;

        foreach (var ev in world.Events.ReadEvents<StateEvent<TState>>())
        {
            if (EqualityComparer<TState>.Default.Equals(ev.State, State) && ev.Transition == StateTransition.Exit)
            {
                return true;
            }
        }

        return false;
    }
}