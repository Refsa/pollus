namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;

public class HierarchyExample : IExample
{
    public string Name => "hierarchy";

    IApplication? app;

    struct Component1 : IComponent { }

    public void Run() => (app = Application.Builder
        .AddSystem(CoreStage.PostInit, FnSystem.Create("Spawn",
        static (World world, Commands commands) =>
        {
            var parent = world.Spawn();
            var child = world.Spawn();
            commands.AddChild(parent, child);
        }))
        .AddSystem(CoreStage.Update, FnSystem.Create("Print",
        static (Query<Parent> qParents, Query query) =>
        {
            foreach (var parent in qParents)
            {
                Log.Info($"Parent: {parent.Entity}");
                
                var current = parent.Component0.FirstChild;
                while (current != Entity.NULL)
                {
                    Log.Info($"    Child: {current}");
                    current = query.Get<Child>(current).NextSibling;
                }
            }
        }))
        .Build())
        .Run();

    public void Stop()
    {
        app?.Shutdown();
    }
}