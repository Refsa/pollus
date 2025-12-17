namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Input;

public partial class HierarchyExample : IExample
{
    public string Name => "hierarchy";

    IApplication? app;

    partial struct Component1 : IComponent { }

    public void Run() => (app = Application.Builder
        .AddPlugins([
            new TimePlugin(),
            new HierarchyPlugin(),
            new InputPlugin(),
        ])
        .AddSystem(CoreStage.PostInit, FnSystem.Create("Spawn",
        static (Commands commands) =>
        {
            var parent = commands.Spawn()
                .AddChild(commands.Spawn().Entity)
                .AddChild(commands.Spawn().Entity)
                .AddChild(commands.Spawn().Entity)
                .Entity;

            var ent5 = commands.Spawn()
                .AddChild(commands.Spawn().Entity)
                .AddChild(commands.Spawn().Entity)
                .AddChild(commands.Spawn().Entity)
                .Entity;
        }))
        .AddSystem(CoreStage.Update, FnSystem.Create("Print",
        static (Local<float> logCD, Time time, Query<Parent>.Filter<None<Child>> qRoots, Query query) =>
        {
            logCD.Value -= time.DeltaTimeF;
            if (logCD.Value > 0)
                return;

            logCD.Value = 1;

            foreach (var root in qRoots)
            {
                Log.Info($"{root.Entity}");
                foreach (var child in query.HierarchyDFS(root.Entity))
                {
                    Log.Info($"{new string(' ', child.Depth * 2)}{child.Entity}");
                }
            }
            Log.Info("");
        }))
        .AddSystem(CoreStage.Update, FnSystem.Create("Destroy",
        static (ButtonInput<Key> keys, Commands commands, Query<Parent>.Filter<None<Child>> qRoots, Query query) =>
        {
            if (!keys.JustPressed(Key.KeyQ)) return;

            if (qRoots.EntityCount() > 0)
            {
                Log.Info($"Despawning {qRoots.Single().Entity}");
                commands.DespawnHierarchy(qRoots.Single().Entity);
            }
            else
            {
                Log.Info("Spawning");
                var parent = commands.Spawn()
                    .AddChild(commands.Spawn().Entity)
                    .AddChild(commands.Spawn().Entity)
                    .AddChild(commands.Spawn().Entity)
                    .Entity;

                var ent5 = commands.Spawn()
                    .AddChild(commands.Spawn().Entity)
                    .AddChild(commands.Spawn().Entity)
                    .AddChild(commands.Spawn().Entity)
                    .AddChild(commands.Spawn().Entity)
                    .Entity;
            }
        }))
        .Build())
        .Run();

    public void Stop()
    {
        app?.Shutdown();
    }
}