namespace Pollus.Engine;

using Assets;
using ECS;
using Utils;

public static class SceneCommands
{
    public static EntityCommands SpawnScene(this Commands commands, in Handle<Scene> scene)
    {
        var root = commands.Spawn();
        commands.AddCommand(new SpawnSceneCommand { Scene = scene, Root = root.Entity });
        return root;
    }
}

public struct SpawnSceneCommand : ICommand
{
    public static int Priority => 99;

    public required Handle<Scene> Scene;
    public required Entity Root;

    public void Execute(World world)
    {
        var assetServer = world.Resources.Get<AssetServer>();
        var sceneAssets = assetServer.GetAssets<Scene>();
        var assetInfo = sceneAssets.GetInfo(Scene);
        if (assetInfo is null) throw new Exception($"Scene {Scene} not found");

        if (assetInfo.Status != AssetStatus.Loaded || assetInfo.Asset is null)
        {
            world.Store.AddComponent(Root, new PendingSceneLoad { Scene = Scene });
            return;
        }

        SceneHelper.SpawnScene(world, sceneAssets, Root, Scene);
    }
}