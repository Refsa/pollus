namespace Pollus.ECS;

public readonly struct Ref<TSystemParam>
{
    public required readonly TSystemParam Value { get; init; }
}

public class RefFetch<TSystemParam> : IFetch<Ref<TSystemParam>>
    where TSystemParam : notnull
{
    public static void Register()
    {
        var fetch = new RefFetch<TSystemParam>();
        Fetch.Register<Ref<TSystemParam>>(fetch, []);
    }

    public Ref<TSystemParam> DoFetch(World world, ISystem system)
    {
        return new()
        {
            Value = world.Resources.Get<TSystemParam>()
        };
    }
}