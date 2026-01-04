namespace Pollus.ECS;

public delegate void ForEachDelegate<C0>(scoped ref C0 component) where C0 : unmanaged, IComponent;

public delegate void ForEachUserDataDelegate<TUserData, C0>(scoped in TUserData userData, scoped ref C0 component) where C0 : unmanaged, IComponent;

public delegate void ForEachEntityDelegate(scoped in Entity entity);

public delegate void ForEachEntityUserDataDelegate<TUserData>(scoped in TUserData userData, scoped in Entity entity);

public delegate void ForEachEntityDelegate<C0>(scoped in Entity entity, scoped ref C0 component) where C0 : unmanaged, IComponent;

public delegate void ForEachEntityUserDataDelegate<TUserData, C0>(scoped in TUserData userData, scoped in Entity entity, scoped ref C0 component) where C0 : unmanaged, IComponent;

public interface IForEach<C0>
    where C0 : unmanaged, IComponent
{
    void Execute(scoped in Entity entity, scoped ref C0 c0);
}

public interface IChunkForEach<C0>
    where C0 : unmanaged, IComponent
{
    void Execute(scoped in ReadOnlySpan<Entity> entities, scoped in Span<C0> chunk0);
}

public interface IRawChunkForEach
{
    void Execute(scoped in ArchetypeChunk chunk);
}
