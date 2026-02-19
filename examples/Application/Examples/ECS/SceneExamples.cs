namespace Pollus.Examples;

using System.Text;
using Pollus.Assets;
using Engine.Camera;
using Pollus.Input;
using Engine.Rendering;
using Engine.Transform;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Debug;

public partial class SceneExample : IExample
{
    public string Name => "scene";
    IApplication? application;

    partial struct Rotate : IComponent
    {
        public float Speed;
    }

    partial struct Root : IComponent
    {
    }

    public void Run() => (application = Application.Builder
            .AddPlugins([
                new TimePlugin(),
                new PerformanceTrackerPlugin(),
                new RenderingPlugin(),
                new ScenePlugin()
                {
                    TypesVersion = 1,
                },
                new InputPlugin(),
            ])
            .AddSystems(CoreStage.PostInit, FnSystem.Create("Init",
                static (Commands commands, AssetServer assetServer) =>
                {
                    Log.Info("""

                             S: Save Scene
                             L: Load Scene
                             U: Unload Scene
                             """);

                    commands.Spawn(Camera2D.Bundle);

                    var parentScene = assetServer.LoadAsync<Scene>("scenes/parent.scene");
                    _ = commands.SpawnScene(parentScene)
                        .AddComponent(new Root());
                }))
            .AddSystems(CoreStage.Update, FnSystem.Create("SaveLoadUnload",
                static (SceneSerializer sceneSerializer, World world, Commands commands, ButtonInput<Key> keyInputs, AssetServer assetServer, Query<Root> qSceneRoot) =>
                {
                    if (keyInputs.JustPressed(Key.KeyS) && qSceneRoot.Any())
                    {
                        using var sceneWriter = sceneSerializer.GetWriter(new()
                        {
                            Indented = true,
                        });
                        var sceneData = sceneWriter.Write(world, qSceneRoot.Single().Entity);
                        Log.Info($"Scene data: \n{Encoding.UTF8.GetString(sceneData)}");
                    }

                    if (keyInputs.JustPressed(Key.KeyL))
                    {
                        var parentScene = assetServer.LoadAsync<Scene>("scenes/parent.scene");
                        _ = commands.SpawnScene(parentScene)
                            .AddComponent(new Root());
                    }

                    if (keyInputs.JustPressed(Key.KeyU) && qSceneRoot.Any())
                    {
                        commands.DespawnHierarchy(qSceneRoot.Single().Entity);
                    }
                }))
            .AddSystems(CoreStage.Update, FnSystem.Create("Rotate::Update",
                static (Time time, Query<Transform2D, Rotate> qRotate) => { qRotate.ForEach(time.DeltaTimeF, static (in deltaTime, ref transform, ref rotate) => { transform.Rotation += rotate.Speed * deltaTime; }); }))
            .Build())
        .Run();

    public void Stop() => application?.Shutdown();
}

