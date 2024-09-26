namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Input;

public class HierarchyExample : IExample
{
    public string Name => "hierarchy";

    IApplication? app;

    struct Component1 : IComponent { }

    public void Run() => (app = Application.Builder
        .AddPlugins([
            new TimePlugin(),
            new HierarchyPlugin(),
            new InputPlugin(),
        ])
        .AddSystem(CoreStage.PostInit, FnSystem.Create("Spawn",
        static (World world, Commands commands) =>
        {
            var parent = world.Spawn();
            commands.AddChild(parent, world.Spawn());
            commands.AddChild(parent, world.Spawn());
            commands.AddChild(parent, world.Spawn());

            var ent5 = world.Spawn();
            commands.AddChild(parent, ent5);
            commands.AddChild(ent5, world.Spawn());
            commands.AddChild(ent5, world.Spawn());
            commands.AddChild(ent5, world.Spawn());
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
        static (ButtonInput<Key> keys, World world, Commands commands, Query<Parent>.Filter<None<Child>> qRoots, Query query) =>
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
                var parent = world.Spawn();
                commands.AddChild(parent, world.Spawn());
                commands.AddChild(parent, world.Spawn());
                commands.AddChild(parent, world.Spawn());

                var ent5 = world.Spawn();
                commands.AddChild(parent, ent5);
                commands.AddChild(ent5, world.Spawn());
                commands.AddChild(ent5, world.Spawn());
                commands.AddChild(ent5, world.Spawn());
            }
        }))
        .Build())
        .Run();

    public void Stop()
    {
        app?.Shutdown();
    }
}