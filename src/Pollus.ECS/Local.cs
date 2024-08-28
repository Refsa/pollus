using Pollus.ECS.Core;

namespace Pollus.ECS;

public class Local<T>
{
    public T Value;

    public Local(T value)
    {
        Value = value;
    }
}

public class LocalFetch<T> : IFetch<Local<T>>
{
    public static void Register()
    {
        Fetch.Register(new LocalFetch<Local<T>>(), []);
    }

    public Local<T> DoFetch(World world, ISystem system)
    {
        if (!system.Resources.TryGet<Local<T>>(out var local))
        {
            local = new Local<T>(default!);
            system.Resources.Add(local);
        }
        return local;
    }
}