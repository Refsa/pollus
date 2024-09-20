namespace Pollus.Engine.Debug;

using Pollus.Debugging;
using Pollus.ECS;

public class PerformanceTrackerPlugin : IPlugin
{
    public void Apply(World world)
    {
        world.Schedule.AddSystems(CoreStage.Last, FnSystem.Create(new("LogPerformanceMetrics")
        {
            RunCriteria = new RunFixed(1f)
        },
        static (Time time) =>
        {
            var fps = 1f / time.DeltaTime;
            Log.Info($"FPS: {fps}");
        }));
    }
}