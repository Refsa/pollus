
namespace Pollus.Game;

using Pollus.ECS;
using Pollus.Engine;

public class ECSExample
{
    World world = new();

    ~ECSExample()
    {
        world.Dispose();
    }

    void Setup(IApplication app)
    {
        for (int i = 0; i < 100_000; i++)
        {
            world.Spawn(new Component1(), new Component2());
        }
    }

    void Update(IApplication app)
    {
        var q = new Query<Component1, Component2>(world);
        q.ForEach((ref Component1 c1, ref Component2 c2) =>
        {
            c1.Value++;
            c2.Value++;
        });

    }

    public void Run()
    {
        (ApplicationBuilder.Default with
        {
            OnSetup = Setup,
            OnUpdate = Update,
        }).Build().Run();
    }
}