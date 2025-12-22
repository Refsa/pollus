namespace Pollus.Examples;

using System.Text;
using Engine.Assets;
using Engine.Camera;
using Engine.Input;
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

    partial struct Rotate : IComponent
    {
        public float Speed;
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
            .AddSystem(CoreStage.PostInit, FnSystem.Create("Init",
                static (Commands commands, AssetServer assetServer) =>
                {
                    Log.Info("""

                             S: Save Scene
                             L: Load Scene
                             U: Unload Scene
                             """);

                    commands.Spawn(Camera2D.Bundle);

                    // var scene = assetServer.Load<Scene>("scenes/scene.scene");
                    // _ = commands.SpawnScene(scene);

                    var parentScene = assetServer.Load<Scene>("scenes/parent.scene");
                    _ = commands.SpawnScene(parentScene);
                }))
            .AddSystem(CoreStage.Update, FnSystem.Create("SaveLoadUnload",
                static (SceneSerializer sceneSerializer, World world, Commands commands, ButtonInput<Key> keyInputs, AssetServer assetServer, Query<SceneRoot> qSceneRoot) =>
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
                        commands.SpawnScene(assetServer.Load<Scene>("scenes/scene.scene"));
                    }

                    if (keyInputs.JustPressed(Key.KeyU) && qSceneRoot.Any())
                    {
                        commands.DespawnHierarchy(qSceneRoot.Single().Entity);
                    }
                }))
            .AddSystem(CoreStage.Update, FnSystem.Create("Rotate::Update",
                static (Time time, Query<Transform2D, Rotate> qRotate) => { qRotate.ForEach(time.DeltaTimeF, static (in deltaTime, ref transform, ref rotate) => { transform.Rotation += rotate.Speed * deltaTime; }); }))
            .Build())
        .Run();

    public void Stop() => application?.Shutdown();
}

