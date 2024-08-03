using System.Runtime.CompilerServices;

namespace Pollus.ECS;

public delegate void ForEachDelegate<C0>(ref C0 component) where C0 : unmanaged, IComponent;
public delegate bool FilterDelegate(Archetype archetype);

public interface IForEachBase<C0>
    where C0 : unmanaged, IComponent
{
    void Execute(ref C0 c0) { }
    void Execute(in Entity entity, ref C0 c0) { }
    void Execute(in Span<C0> chunk0) { }
}

public interface IForEach<C0> : IForEachBase<C0>
    where C0 : unmanaged, IComponent
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    new void Execute(ref C0 c0);
}

public interface IEntityForEach<C0> : IForEachBase<C0>
    where C0 : unmanaged, IComponent
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    new void Execute(in Entity entity, ref C0 c0);
}

public interface IChunkForEach<C0> : IForEachBase<C0>
    where C0 : unmanaged, IComponent
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    new void Execute(in Span<C0> chunk0);
}