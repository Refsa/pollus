namespace Pollus.ECS;

using Pollus.Coroutine;
using Pollus.Utils;

public enum WorldYieldInstruction
{
    WaitForEvent,
}

public struct WaitForStateEnter<TState>
    where TState : unmanaged, Enum
{
    static WaitForStateEnter()
    {
        YieldCustomInstructionHandler<WorldYieldInstruction, Param<World>>.AddHandler(TypeLookup.ID<WaitForStateEnter<TState>>(), (in Yield yield, Param<World> param) =>
        {
            var handler = yield.GetData<WaitForStateEnter<TState>>(8);
            return handler.Execute(param);
        });
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
    static WaitForStateExit()
    {
        YieldCustomInstructionHandler<WorldYieldInstruction, Param<World>>.AddHandler(TypeLookup.ID<WaitForStateExit<TState>>(), (in Yield yield, Param<World> param) =>
        {
            var handler = yield.GetData<WaitForStateExit<TState>>(8);
            return handler.Execute(param);
        });
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