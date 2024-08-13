using Pollus.ECS;
using Pollus.Game;
using static Pollus.ECS.SystemBuilder;

// new DrawTriangle().Run();
// new ECSExample().Run();
// new InputExample().Run();
// new AudioExample().Run();
// new SnakeGame().Run();

using var world = new World();
world.Schedule.AddSystem(CoreStage.First, [
    new TimeSystem()
]);
world.Schedule.AddSystem(CoreStage.Update, [
    FnSystem("System1", (Time time) => 
    {
        if (time.FrameCount % 60 == 0)
        {
            Console.WriteLine($"FPS: {time.FrameCount / time.SecondsSinceStartup}");
        }
    })
]);
world.Prepare();

Console.WriteLine($"{world.Schedule}");

while (true)
{
    world.Tick();
}
