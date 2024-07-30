namespace Pollus.ECS;

public class World : IDisposable
{
    public ArchetypeStore Archetypes { get; init; } = new();

    public void Dispose()
    {
        Archetypes.Dispose();
    }

    public Entity Spawn<TBuilder>(TBuilder builder)
        where TBuilder : IEntityBuilder
    {
        return builder.Spawn(this);
    }
}