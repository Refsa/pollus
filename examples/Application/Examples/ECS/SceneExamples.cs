namespace Pollus.Examples;

using Engine.Assets;
using Engine.Camera;
using Engine.Rendering;
using Engine.Transform;
using Mathematics;
using Pollus.Debugging;
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
                new RenderingPlugin(),
                new ScenePlugin(),
            ])
            .AddSystem(CoreStage.PostInit, FnSystem.Create("Spawn",
                static (Commands commands, AssetServer assetServer) =>
                {
                    commands.Spawn(Camera2D.Bundle);

                    var scene = assetServer.Load<Scene>("scenes/scene.scene");
                    var root = commands.SpawnScene(scene);
                    root.AddComponent(new GlobalTransform())
                        .AddComponent(new Transform2D());
                }))
            .AddSystem(CoreStage.Update, FnSystem.Create("Update",
                static (Time time, Query query, Query.Filter<All<SceneRoot>> qSceneRoots) =>
                {
                    
                }))
            .Build())
        .Run();

    public void Stop() => application?.Shutdown();
}

