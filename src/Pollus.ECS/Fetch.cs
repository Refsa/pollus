namespace Pollus.ECS;

public interface IFetch
{
    static abstract void Register();
}

public interface IFetch<out T> : IFetch
{
    T DoFetch(World world, ISystem system);
}

public static class Fetch
{
    public class Info
    {
        public required IFetch Fetch { get; set; }
        public required HashSet<Type> Dependencies { get; set; }
    }

    static class Lookup<T>
    {
        static volatile Info? info;

        public static bool IsSet => info != null;
        public static Info Info => info!;

        public static void Set(Info info)
        {
            Lookup<T>.info = info;
        }
    }

    public static bool IsRegistered<T>()
    {
        return Lookup<T>.IsSet;
    }

    public static void Register<T>(IFetch<T> fetch, Span<Type> dependencies)
    {
        if (Lookup<T>.IsSet)
        {
            return;
        }

        Lookup<T>.Set(new Info
        {
            Fetch = fetch,
            Dependencies = [.. dependencies],
        });
    }

    public static Info Get<T>()
    {
        if (!Lookup<T>.IsSet)
        {
            Console.WriteLine($"Fetch for {typeof(T).Name} is not registered");
            throw new Exception($"Fetch for {typeof(T).Name} is not registered");
        }

        return Lookup<T>.Info;
    }

    public static IFetch<T>? GetFetch<T>()
    {
        if (!Lookup<T>.IsSet)
        {
            Console.WriteLine($"Fetch for {typeof(T).Name} is not registered");
            throw new Exception($"Fetch for {typeof(T).Name} is not registered");
        }

        return Lookup<T>.Info.Fetch as IFetch<T>;
    }
}