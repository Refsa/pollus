namespace Pollus.Engine.Assets;

using Pollus.ECS;
using Core.Assets;

public class AssetsFetch<T> : IFetch<Assets<T>>
    where T : IAsset
{
    public static void Register()
    {
        Fetch.Register(new AssetsFetch<T>(), [typeof(Assets<T>)]);
    }

    public Assets<T> DoFetch(World world, ISystem system)
    {
        return world.Resources.Get<AssetServer>().GetAssets<T>();
    }
}

public class AssetPlugin : IPlugin
{
    public const string UpdateSystem = "AssetPlugin::Update";
    public const string FlushSystem = "AssetPlugin::Flush";

    static AssetPlugin()
    {
        ResourceFetch<AssetServer>.Register();
    }

    public static AssetPlugin Default => new() { RootPath = "assets" };

    public required string RootPath { get; init; }
    public bool EnableFileWatch { get; init; } = true;

    public void Apply(World world)
    {
        if (world.Resources.Has<AssetServer>()) return;

        var assetServer = new AssetServer(new FileAssetIO(RootPath));
        world.Resources.Add(assetServer);

        if (EnableFileWatch && !OperatingSystem.IsBrowser() && DevelopmentAssetsWatch.Create() is { } devAssetsWatch)
        {
            assetServer.EnableFileWatch();
            world.Resources.Add(devAssetsWatch);
        }

        world.Schedule.AddSystems(CoreStage.First, FnSystem.Create(UpdateSystem,
            static (AssetServer assetServer, Events events) =>
            {
                assetServer.Update();
                assetServer.Assets.FlushEvents(events);
            }));

        world.Schedule.AddSystems(CoreStage.Last, FnSystem.Create(FlushSystem,
            static (AssetServer assetServer) =>
            {
                assetServer.FlushQueue();
            }));
    }
}