namespace Pollus.ECS;

public class Local<T>
{
    static Local() => LocalFetch<T>.Register();

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
        Fetch.Register(new LocalFetch<T>(), []);
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