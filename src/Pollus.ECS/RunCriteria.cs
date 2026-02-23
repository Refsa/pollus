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

public class WhenAll : IRunCriteria
{
    IRunCriteria[] criterias;

    public WhenAll(ReadOnlySpan<IRunCriteria> criterias)
    {
        this.criterias = criterias.ToArray();
    }

    public static WhenAll Create(params ReadOnlySpan<IRunCriteria> criterias) => new(criterias);

    public bool ShouldRun(World world)
    {
        var s = criterias.AsSpan();
        for (int i = 0; i < s.Length; i++)
        {
            if (!s[i].ShouldRun(world)) return false;
        }
        return true;
    }
}

public class WhenAny : IRunCriteria
{
    IRunCriteria[] criterias;

    public WhenAny(ReadOnlySpan<IRunCriteria> criterias)
    {
        this.criterias = criterias.ToArray();
    }

    public static WhenAny Create(params ReadOnlySpan<IRunCriteria> criterias) => new(criterias);

    public bool ShouldRun(World world)
    {
        var s = criterias.AsSpan();
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i].ShouldRun(world)) return true;
        }
        return false;
    }
}

public class RunFixed : IRunCriteria
{
    long previousTicks = 0;
    public float TargetFrameTime { get; }

    public RunFixed(float targetFramerate)
    {
        TargetFrameTime = 1f / targetFramerate;
    }

    public bool ShouldRun(World world)
    {
        if (TargetFrameTime == 0) return true;

        var time = world.Resources.Get<Time>();

        float deltaTime = DeltaTime(time.Ticks);
        if (deltaTime < TargetFrameTime) return false;

        previousTicks = time.Ticks;
        return true;
    }

    float DeltaTime(long ticks)
    {
        return (ticks - previousTicks) / (float)System.Diagnostics.Stopwatch.Frequency;
    }
}