using System.Runtime.CompilerServices;

namespace Pollus.ECS;

public class World : IDisposable
{
    public ArchetypeStore Store { get; init; } = new();

    public void Dispose()
    {
        Store.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Entity Spawn()
    {
        return Store.CreateEntity();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Entity Spawn<TBuilder>(TBuilder builder)
        where TBuilder : IEntityBuilder
    {
        return builder.Spawn(this);
    }
}