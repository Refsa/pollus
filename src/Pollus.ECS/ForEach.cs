namespace Pollus.ECS;

using System.Runtime.CompilerServices;

public delegate void ForEachDelegate<C0>(ref C0 component) where C0 : unmanaged, IComponent;
public delegate void ForEachEntityDelegate(in Entity entity);
public delegate void ForEachEntityDelegate<C0>(in Entity entity, ref C0 component) where C0 : unmanaged, IComponent;

public enum ForEachType
{
    Component, Entity, Chunk,
}

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
    new void Execute(scoped ref C0 c0);
}

public interface IEntityForEach<C0> : IForEachBase<C0>
    where C0 : unmanaged, IComponent
{
    new void Execute(scoped in Entity entity, scoped ref C0 c0);
}

public interface IChunkForEach<C0> : IForEachBase<C0>
    where C0 : unmanaged, IComponent
{
    new void Execute(scoped in Span<C0> chunk0);
}