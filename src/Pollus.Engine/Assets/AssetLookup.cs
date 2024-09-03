namespace Pollus.Engine.Assets;

using System.Collections.Concurrent;
using Pollus.ECS;

public static class AssetLookup
{
    static class Type<T>
        where T : notnull
    {
        public static int ID = Interlocked.Increment(ref counter);

        static Type()
        {
            AssetsFetch<T>.Register();
            lookup.TryAdd(ID, typeof(T));
        }
    }

    static readonly ConcurrentDictionary<int, Type> lookup = new();
    static volatile int counter = 0;
    public static int ID<T>() where T : notnull => Type<T>.ID;

    public static Type? GetType(int assetId)
    {
        if (lookup.TryGetValue(assetId, out var type))
        {
            return type;
        }
        return null;
    }
}