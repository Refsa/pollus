namespace Pollus.Examples;

using Engine.Assets;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Debug;
using Utils;

public partial class SceneExample : IExample
{
    public string Name => "scene";
    IApplication? application;

    public void Run() => (application = Application.Builder
            .AddPlugins([
                new TimePlugin(),
                new PerformanceTrackerPlugin(),
                new ScenePlugin(),
            ])
            .AddSystem(CoreStage.PostInit, FnSystem.Create("Spawn", static (Commands commands, AssetServer assetServer) =>
            {
                var scene = assetServer.Load<Scene>("scenes/scene.scene");
                commands.SpawnScene(scene, Entity.NULL);
            }))
            .Build())
        .Run();

    public void Stop() => application?.Shutdown();
}

