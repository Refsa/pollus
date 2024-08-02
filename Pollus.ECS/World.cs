using System.Runtime.CompilerServices;

namespace Pollus.ECS;

public class World : IDisposable
{
    public ArchetypeStore Archetypes { get; init; } = new();

    public void Dispose()
    {
        Archetypes.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Entity Spawn()
    {
        return Archetypes.CreateEntity();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Entity Spawn<TBuilder>(TBuilder builder)
        where TBuilder : IEntityBuilder
    {
        return builder.Spawn(this);
    }
}