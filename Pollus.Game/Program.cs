using Pollus.ECS;
using Pollus.Game;
using static Pollus.ECS.SystemBuilder;

// new DrawTriangle().Run();
// new ECSExample().Run();
// new InputExample().Run();
// new AudioExample().Run();
// new SnakeGame().Run();

using var world = new World()
    .AddPlugin<TimePlugin>();

for (int i = 0; i < 1_000_000; i++)
{
    Entity.With(new Component1()).Spawn(world);
}

world.Schedule.AddSystem(CoreStage.First, [
    FnSystem("DebugFPS", (Time time) =>
    {
        Console.WriteLine($"FPS: {time.FrameCount / time.SecondsSinceStartup}");
    }).After("TimeSystem").RunCriteria(new RunFixed(1f)).Build(),
]);
world.Schedule.AddSystem(CoreStage.Update, [
    FnSystem("QueryTest", (Query<Component1> query) =>
    {
        query.ForEach(new Iter());
    })
]);
world.Prepare();

Console.WriteLine($"{world.Schedule}");

while (true)
{
    world.Tick();
}

struct Iter : IForEach<Component1>
{
    public void Execute(ref Component1 c0)
    {
        c0.Value++;
    }
}