namespace Pollus.ECS;

using System.Runtime.CompilerServices;
using Pollus.Coroutine;
using Pollus.Debugging;

public class Coroutine<TSystemParam> : SystemBase<Time, TSystemParam, Param<World>>, IDisposable
    where TSystemParam : ISystemParam
{
    public delegate IEnumerable<Yield> FactoryDelegate(TSystemParam param);

    readonly CoroutineWrapper<TSystemParam> wrapper;

    public Coroutine(SystemDescriptor descriptor, FactoryDelegate factory) : base(descriptor)
    {
        wrapper = new(factory);
    }

    public override void Dispose()
    {
        base.Dispose();
        wrapper.Dispose();
    }

    protected override void OnTick(Time time, TSystemParam param, Param<World> worldParam)
    {
        try
        {
            wrapper.SetParam(param);
            ref var current = ref wrapper.Current;

            if (current.Instruction == Yield.Type.Return) { }
            else if (current.Instruction == Yield.Type.WaitForSeconds)
            {
                var seconds = current.GetData<float>();
                current.SetData(seconds - time.DeltaTimeF);
                if (seconds > 0) return;
            }
            else if (current.Instruction == Yield.Type.Custom)
            {
                if (!YieldCustomInstructionHandler<Param<World>>.Handle(in current, worldParam)) return;
            }

            wrapper.MoveNext();
        }
        catch (Exception e)
        {
            Log.Exception(e, "Failed to execute coroutine");
        }
    }
}

class CoroutineWrapper<TSystemParam> : IDisposable
    where TSystemParam : ISystemParam
{
    readonly Coroutine<TSystemParam>.FactoryDelegate factory;
    TSystemParam? param;
    IEnumerator<Yield>? enumerator;
    Yield current = Yield.Return;

    public ref Yield Current => ref current;

    public CoroutineWrapper(Coroutine<TSystemParam>.FactoryDelegate factory)
    {
        this.factory = factory;
    }

    public void Dispose()
    {
        current.Dispose();
    }

    public void SetParam(TSystemParam param)
    {
        this.param = param;
    }

    public bool MoveNext()
    {
        if (enumerator?.MoveNext() is true)
        {
            current.Dispose();
            current = enumerator.Current;
            return true;
        }

        Guard.IsNotNull(param, "Coroutine param is not set");
        enumerator = factory(param).GetEnumerator();
        return MoveNext();
    }
}

public static class Coroutine
{
    public static void RegisterHandler<TData>(YieldCustomInstructionHandler<Param<World>>.HandlerDelegate handler, Type[] dependencies)
        where TData : struct
    {
        YieldCustomInstructionHandler<Param<World>>.AddHandler<TData>(handler, dependencies);
    }

    public static Coroutine<TSystemParam> Create<TSystemParam>(
        SystemBuilderDescriptor descriptor,
        Coroutine<TSystemParam>.FactoryDelegate routineFactory
    )
        where TSystemParam : ISystemParam
    {
        var system = new Coroutine<TSystemParam>(descriptor, routineFactory);
        if (descriptor.IsExclusive) system.Descriptor.DependsOn<ExclusiveSystemMarker>();
        foreach (var local in descriptor.Locals) system.Resources.Add(local, local.TypeID);
        return system;
    }

    public static Coroutine<EmptyParam> Create(
        SystemBuilderDescriptor descriptor,
        Coroutine<EmptyParam>.FactoryDelegate routineFactory
    )
    {
        var system = new Coroutine<EmptyParam>(descriptor, routineFactory);
        if (descriptor.IsExclusive) system.Descriptor.DependsOn<ExclusiveSystemMarker>();
        foreach (var local in descriptor.Locals) system.Resources.Add(local, local.TypeID);
        return system;
    }

    public static Yield WaitForEnterState<TState>(TState state)
        where TState : unmanaged, Enum
    {
        return Yield.Custom(new WaitForStateEnter<TState>(state));
    }

    public static Yield WaitForExitState<TState>(TState state)
        where TState : unmanaged, Enum
    {
        return Yield.Custom(new WaitForStateExit<TState>(state));
    }
}