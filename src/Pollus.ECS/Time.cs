namespace Pollus.ECS;

using System.Diagnostics;
using Pollus.ECS.Core;

[Resource<Time>]
public class Time
{
    public double DeltaTime { get; set; }
    public long FrameCount { get; set; }
    public long Ticks { get; set; }
    public double SecondsSinceStartup { get; set; }
}

public class TimeSystem : Sys<Time>
{
    Stopwatch stopwatch = new();
    long previousTicks = 0;

    public TimeSystem() : base(new SystemDescriptor(nameof(TimeSystem)))
    {
        Descriptor.DependsOn<Time>();
    }

    protected override void OnTick(Time time)
    {
        if (!stopwatch.IsRunning)
        {
            stopwatch.Start();
        }

        long frameTicks = stopwatch.ElapsedTicks;
        double deltaTime = (frameTicks - previousTicks) / (double)Stopwatch.Frequency;

        time.FrameCount++;
        time.DeltaTime = deltaTime;
        time.Ticks = frameTicks;
        time.SecondsSinceStartup = stopwatch.Elapsed.TotalMilliseconds / 1000.0;

        previousTicks = frameTicks;
    }
}

public class TimePlugin : IPlugin
{
    public void Apply(World world)
    {
        world.Resources.Add<Time>();
        world.Schedule.AddSystem(CoreStage.First, new TimeSystem());
    }
}