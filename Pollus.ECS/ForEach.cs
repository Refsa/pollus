using System.Runtime.CompilerServices;

namespace Pollus.ECS;

public delegate void IterDelegate<C0>(scoped ref C0 component) where C0 : unmanaged, IComponent;
public delegate bool FilterDelegate(Archetype archetype);

public interface IForEachBase<C0>
    where C0 : unmanaged, IComponent
{
    void Execute(scoped ref C0 c0) { }
    void Execute(scoped in Entity entity, scoped ref C0 c0) { }
    void Execute(scoped in Span<C0> chunk0) { }
}

public interface IForEach<C0> : IForEachBase<C0>
    where C0 : unmanaged, IComponent
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    new void Execute(scoped ref C0 c0);
}

public interface IEntityForEach<C0> : IForEachBase<C0>
    where C0 : unmanaged, IComponent
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    new void Execute(scoped in Entity entity, scoped ref C0 c0);
}

public interface IChunkForEach<C0> : IForEachBase<C0>
    where C0 : unmanaged, IComponent
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    new void Execute(scoped in Span<C0> chunk0);
}