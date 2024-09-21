namespace Pollus.ECS;

public enum StateTransition
{
    Enter,
    Current,
    Exit,
}

public struct StateEvent<T>
    where T : unmanaged, Enum
{
    public required T State { get; init; }
    public required StateTransition Transition { get; init; }
}

public record State<T>
    where T : unmanaged, Enum
{
    public static readonly string OnExit = $"OnExit<{typeof(T).Name}>";
    public static readonly string OnEnter = $"OnEnter<{typeof(T).Name}>";

    static State()
    {
        ResourceFetch<State<T>>.Register();
    }

    public T Current { get; private set; }
    public T? Next { get; private set; }

    public State(T defaultState)
    {
        Next = defaultState;
    }

    public void Set(T state)
    {
        Next = state;
    }

    public bool InState(T state) => Current.Equals(state);

    internal void Apply()
    {
        Current = Next ?? Current;
        Next = null;
    }
}

public class StateRunCriteria<T> : IRunCriteria
    where T : unmanaged, Enum
{
    T state;
    StateTransition target;

    public bool ShouldRun(World world)
    {
        var events = world.Events.ReadEvents<StateEvent<T>>();
        if (events.Length == 0)
        {
            var stateRes = world.Resources.Get<State<T>>();
            return EqualityComparer<T>.Default.Equals(stateRes.Current, state) && target == StateTransition.Current;
        }

        var head = events[^1];
        return head.Transition == target && EqualityComparer<T>.Default.Equals(head.State, state);
    }

    public static StateRunCriteria<T> OnEnter(T state)
    {
        return new StateRunCriteria<T>
        {
            state = state,
            target = StateTransition.Enter,
        };
    }

    public static StateRunCriteria<T> OnExit(T state)
    {
        return new StateRunCriteria<T>
        {
            state = state,
            target = StateTransition.Exit,
        };
    }

    public static StateRunCriteria<T> OnCurrent(T state)
    {
        return new StateRunCriteria<T>
        {
            state = state,
            target = StateTransition.Current,
        };
    }
}

public record struct StateEnter(string label)
{
    public static implicit operator StageLabel(StateEnter onEnter) => new(onEnter.label);

    public static StateEnter On<T>(T state)
        where T : unmanaged, Enum
    {
        return new StateEnter($"State::OnEnter::{typeof(T).Name}::{state}");
    }
}

public record struct StateExit(string label)
{
    public static implicit operator StageLabel(StateExit onExit) => new(onExit.label);

    public static StateExit On<T>(T state)
        where T : unmanaged, Enum
    {
        return new StateExit($"State::OnExit::{typeof(T).Name}::{state}");
    }
}

public class StatePlugin<T> : IPlugin
    where T : unmanaged, Enum
{
    T defaultState = default;

    public StatePlugin(T defaultState = default)
    {
        this.defaultState = defaultState;
    }

    public void Apply(World world)
    {
        world.Events.InitEvent<StateEvent<T>>();
        world.Resources.Add(new State<T>(defaultState));

        foreach (var state in Enum.GetValues<T>().Reverse())
        {
            var enterStage = new Stage(StateEnter.On<T>(state))
            {
                RunCriteria = StateRunCriteria<T>.OnEnter(state)
            };
            var exitStage = new Stage(StateExit.On<T>(state))
            {
                RunCriteria = StateRunCriteria<T>.OnExit(state)
            };

            world.Schedule.AddStage(exitStage, before: null, after: CoreStage.First);
            world.Schedule.AddStage(enterStage, before: null, after: CoreStage.First);
        }

        world.Schedule.AddSystems(CoreStage.First, FnSystem.Create(
            $"StateTransitionSystem<{typeof(T).Name}>",
            static (State<T> state, EventWriter<StateEvent<T>> writer) =>
            {
                if (state.Next is T nextState)
                {
                    if (!EqualityComparer<T>.Default.Equals(state.Current, nextState))
                    {
                        writer.Write(new StateEvent<T>
                        {
                            State = state.Current,
                            Transition = StateTransition.Exit
                        });
                    }

                    state.Apply();

                    writer.Write(new StateEvent<T>
                    {
                        State = state.Current,
                        Transition = StateTransition.Enter
                    });
                }
            }
        ));
    }
}
