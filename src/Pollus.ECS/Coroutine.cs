namespace Pollus.ECS;

using Pollus.Coroutine;

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
}

public class Coroutine<TSystemParam, TEnumerator> : SystemBase<Time, TSystemParam>
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

    protected override void OnTick(Time time, TSystemParam param)
    {
        routine ??= factory(param);

        if (current.CurrentInstruction == Yield.Instruction.Return)
        {

        }
        else if (current.CurrentInstruction == Yield.Instruction.WaitForSeconds)
        {
            var seconds = current.GetData<float>();
            current.SetData(seconds - time.DeltaTimeF);
            if (seconds > 0) return;
        }

        if (!routine.MoveNext())
        {
            routine = factory(param);
            routine.MoveNext();
        }
        current = routine.Current;
    }
}