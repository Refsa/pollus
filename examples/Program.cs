using Pollus.ECS;
using Pollus.Engine;
using Pollus.Game;
using static Pollus.ECS.SystemBuilder;

// new DrawTriangle().Run();
// new ECSExample().Run();
// new InputExample().Run();
new SnakeGame().Run();
// new AudioExample().Run();

/* (ApplicationBuilder.Default with
{
    World = new World().AddPlugin<TimePlugin>(),
    OnSetup = (app) =>
    {
        for (int i = 0; i < 1_000_000; i++)
        {
            Entity.With(new Component1 { Value = i }).Spawn(app.World);
        }

        app.World.Schedule.AddSystem(CoreStage.First, [
            FnSystem("DebugFPS", (Time time) =>
            {
                Console.WriteLine($"FPS: {time.FrameCount / time.SecondsSinceStartup}");
            }).After("TimeSystem").RunCriteria(new RunFixed(1f)).Build(),
        ]);
        app.World.Schedule.AddSystem(CoreStage.Update, [
            FnSystem("QueryTest", (Query<Component1> query) =>
            {
                query.ForEach(new Iter());
            })
        ]);
        app.World.Prepare();

        Console.WriteLine($"{app.World.Schedule}");
    },
    OnUpdate = (app) =>
    {
        app.World.Tick();
    }
}).Build().Run();

struct Iter : IForEach<Component1>
{
    public void Execute(ref Component1 c0)
    {
        c0.Value++;
    }
} */