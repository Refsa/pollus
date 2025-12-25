namespace Pollus.Engine.Debug;

using Pollus.Engine.Window;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Graphics.Windowing;

public class PerformanceTrackerPlugin : IPlugin
{
    static PerformanceTrackerPlugin()
    {
        ResourceFetch<PerformanceMetrics>.Register();
    }

    public PluginDependency[] Dependencies => [
        PluginDependency.From<TimePlugin>(),
        PluginDependency.From<WindowPlugin>(),
    ];

    public class PerformanceMetrics
    {
        readonly Queue<float> frameTimes = new();
        float frameTime;

        public float FrameTime => frameTime;
        public float AverageFrameTime => frameTimes.DefaultIfEmpty(1f).Average();
        public float AverageFPS => 1f / AverageFrameTime;

        public void AddFrameTime(float frameTime)
        {
            this.frameTime = frameTime;
            frameTimes.Enqueue(frameTime);
            while (frameTimes.Count > int.Max(60, (int)(1f / frameTime * 4))) frameTimes.Dequeue();
        }
    }

    public void Apply(World world)
    {
        world.Resources.Add(new PerformanceMetrics());

        world.Schedule.AddSystems(CoreStage.Last, FnSystem.Create(new("LogPerformanceMetrics")
        {
            RunCriteria = new RunFixed(1f)
        },
        static (IWindow window, PerformanceMetrics metrics, Time time) =>
        {
            metrics.AddFrameTime(time.DeltaTimeF);
            window.SetTitle($"Pollus - {metrics.AverageFPS:F2} FPS");
        }));
    }
}