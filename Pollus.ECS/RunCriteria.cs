namespace Pollus.ECS;

public interface IRunCriteria
{
    bool ShouldRun(World world);
}

public class RunAlways : IRunCriteria
{
    public static readonly RunAlways Instance = new();
    public bool ShouldRun(World world) => true;
}

public class RunOnce : IRunCriteria
{
    bool hasRun = false;
    public bool ShouldRun(World world)
    {
        if (hasRun) return false;
        return hasRun = true;
    }
}

public class RunFixed : IRunCriteria
{
    System.Diagnostics.Stopwatch stopwatch = new();
    long previousTicks = 0;
    public float TargetFrameTime { get; }

    public RunFixed(int targetFramerate)
    {
        TargetFrameTime = 1f / targetFramerate;
    }

    public bool ShouldRun(World world)
    {
        if (!stopwatch.IsRunning)
        {
            stopwatch.Start();
            return true;
        }

        long ticks = stopwatch.ElapsedTicks;
        float deltaTime = DeltaTime(ticks);
        if (deltaTime < TargetFrameTime) return false;

        previousTicks = ticks;
        return true;
    }

    float DeltaTime(long ticks)
    {
        return (ticks - previousTicks) / (float)System.Diagnostics.Stopwatch.Frequency;
    }
}