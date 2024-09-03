namespace Pollus.Engine.Assets;

using Pollus.ECS;
using Pollus.ECS.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class AssetAttribute : Attribute { }

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
    public required string RootPath { get; init; }

    public void Apply(World world)
    {
        var assetServer = new AssetServer(new FileAssetIO(RootPath));
        world.Resources.Add(assetServer);
        world.Resources.Add(assetServer.Assets);
    }
}