namespace Pollus.Engine.Assets;

using Pollus.ECS;

public static class AssetLookup
{
    static class Type<T>
        where T : notnull
    {
        public static int ID = counter++;

        static Type()
        {
            Fetch.Register(new ResourceFetch<T>(), []);
        }
    }

    static volatile int counter = 0;
    public static int ID<T>() where T : notnull => Type<T>.ID;
}