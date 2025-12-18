namespace Pollus.Engine.Assets;

using Pollus.ECS;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class AssetAttribute : Attribute
{
}

public class AssetsFetch<T> : IFetch<Assets<T>>
    where T : notnull
{
    public static void Register()
    {
        Fetch.Register(new AssetsFetch<T>(), [typeof(Assets<T>)]);
    }

    public Assets<T> DoFetch(World world, ISystem system)
    {
        return world.Resources.Get<Assets>().GetAssets<T>();
    }
}

public class AssetPlugin : IPlugin
{
    static AssetPlugin()
    {
        ResourceFetch<AssetServer>.Register();
    }

    public static AssetPlugin Default => new() { RootPath = "assets" };

    public required string RootPath { get; init; }

    public void Apply(World world)
    {
        if (world.Resources.Has<AssetServer>()) return;

        var assetServer = new AssetServer(new FileAssetIO(RootPath));
        world.Resources.Add(assetServer);
        world.Resources.Add(assetServer.Assets);
    }
}