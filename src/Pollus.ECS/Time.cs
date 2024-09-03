namespace Pollus.ECS;

using System.Diagnostics;
using Pollus.ECS.Core;
using Pollus.Mathematics;

public class Time
{
    public double DeltaTime { get; set; }
    public long FrameCount { get; set; }
    public long Ticks { get; set; }
    public double SecondsSinceStartup { get; set; }

    public float DeltaTimeF => (float)DeltaTime;
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
        deltaTime = deltaTime.Clamp(0, 0.1);
        previousTicks = frameTicks;

        time.FrameCount++;
        time.DeltaTime = deltaTime;
        time.Ticks = frameTicks;
        time.SecondsSinceStartup = stopwatch.Elapsed.TotalMilliseconds / 1000.0;
    }
}

public class TimePlugin : IPlugin
{
    public void Apply(World world)
    {
        world.Resources.Add<Time>();
        world.Schedule.AddSystems(CoreStage.First, new TimeSystem());
    }
}