
namespace Pollus.Examples;

using Pollus.ECS;
using Pollus.Engine;
using static Pollus.ECS.SystemBuilder;

public class ECSExample
{
    World world = new();

    ~ECSExample()
    {
        world.Dispose();
    }

    public void Run() => Application.Builder
        .AddPlugin(new TimePlugin())
        .AddSystem(CoreStage.PostInit, FnSystem("Setup",
        static (World world) =>
        {
            for (int i = 0; i < 100_000; i++)
            {
                world.Spawn(new Component1(), new Component2());
            }
        }))
        .AddSystem(CoreStage.Update, FnSystem("Update",
        static (Query<Component1, Component2> query) =>
        {
            query.ForEach((ref Component1 c1, ref Component2 c2) =>
            {
                c1.Value++;
                c2.Value++;
            });
        }))
        .Run();
}