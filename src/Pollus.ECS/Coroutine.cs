namespace Pollus.ECS;

using Pollus.Coroutine;
using Pollus.Utils;

public static class Coroutine
{
    public static Coroutine<TSystemParam, TEnumerator> Create<TSystemParam, TEnumerator>(SystemBuilderDescriptor descriptor, Func<TSystemParam, TEnumerator> routineFactory)
        where TSystemParam : ISystemParam
        where TEnumerator : IEnumerator<Yield>
    {
        var system = new Coroutine<TSystemParam, TEnumerator>(descriptor, routineFactory);
        if (descriptor.IsExclusive) system.Descriptor.DependsOn<ExclusiveSystemMarker>();
        foreach (var local in descriptor.Locals) system.Resources.Add(local, local.TypeID);
        return system;
    }

    public static Yield WaitForEnterState<TState>(TState state)
        where TState : unmanaged, Enum
    {
        return Yield.Custom(Wrap(WorldYieldInstruction.WaitForEvent, new WaitForStateEnter<TState>(state)));
    }

    public static Yield WaitForExitState<TState>(TState state)
        where TState : unmanaged, Enum
    {
        return Yield.Custom(Wrap(WorldYieldInstruction.WaitForEvent, new WaitForStateExit<TState>(state)));
    }

    static YieldCustomData<WorldYieldInstruction, TData> Wrap<TData>(WorldYieldInstruction instruction, TData data)
        where TData : unmanaged
    {
        return new YieldCustomData<WorldYieldInstruction, TData>
        {
            Instruction = instruction,
            TypeID = TypeLookup.ID<TData>(),
            Data = data,
        };
    }
}

public class Coroutine<TSystemParam, TEnumerator> : SystemBase<Time, TSystemParam, Param<World>>
    where TSystemParam : ISystemParam
    where TEnumerator : IEnumerator<Yield>
{
    Func<TSystemParam, TEnumerator> factory;
    TEnumerator? routine;
    Yield current;

    public Coroutine(SystemDescriptor descriptor, Func<TSystemParam, TEnumerator> factory) : base(descriptor)
    {
        this.factory = factory;
    }

    protected override void OnTick(Time time, TSystemParam param, Param<World> worldParam)
    {
        routine ??= factory(param);

        if (current.Instruction == Yield.Type.Return)
        {

        }
        else if (current.Instruction == Yield.Type.WaitForSeconds)
        {
            var seconds = current.GetData<float>();
            current.SetData(seconds - time.DeltaTimeF);
            if (seconds > 0) return;
        }
        else if (current.Instruction == Yield.Type.Custom)
        {
            if (!YieldCustomInstructionHandler<WorldYieldInstruction, Param<World>>.Handle(in current, worldParam)) return;
        }

        if (!routine.MoveNext())
        {
            routine = factory(param);
            routine.MoveNext();
        }
        current = routine.Current;
    }
}