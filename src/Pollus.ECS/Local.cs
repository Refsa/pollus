namespace Pollus.ECS;

using Pollus.Utils;

public abstract class Local
{
    public abstract int TypeID { get; }

    public static Local<T> From<T>(T local)
    {
        return new Local<T>(local);
    }
}

public class Local<T> : Local
{
    static Local()
    {
        LocalFetch<T>.Register();
    }

    public T Value;
    public override int TypeID => TypeLookup.ID<T>();

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