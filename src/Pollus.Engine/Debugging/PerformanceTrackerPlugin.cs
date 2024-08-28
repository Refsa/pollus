namespace Pollus.Engine.Debug;

using Pollus.Debugging;
using Pollus.ECS;

public class PerformanceTrackerPlugin : IPlugin
{
    public void Apply(World world)
    {
        world.Schedule.AddSystems(CoreStage.Last, SystemBuilder.FnSystem("LogPerformanceMetrics",
        static (Time time) =>
        {
            var fps = 1f / time.DeltaTime;
            Log.Info($"FPS: {fps}");
        }).RunCriteria(new RunFixed(1f)));
    }
}