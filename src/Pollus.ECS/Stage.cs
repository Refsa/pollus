namespace Pollus.ECS;

using System.Text;
using Pollus.Debugging;
using Pollus.Utils;

public record struct StageLabel(string Value)
{
    public override string ToString() => $"Stage<{Value}>";
}

public static class CoreStage
{
    public static readonly StageLabel Init = new(nameof(Init));
    public static readonly StageLabel PostInit = new(nameof(PostInit));

    public static readonly StageLabel First = new(nameof(First));
    public static readonly StageLabel PreUpdate = new(nameof(PreUpdate));
    public static readonly StageLabel Update = new(nameof(Update));
    public static readonly StageLabel PostUpdate = new(nameof(PostUpdate));
    public static readonly StageLabel Last = new(nameof(Last));

    public static readonly StageLabel PreRender = new(nameof(PreRender));
    public static readonly StageLabel Render = new(nameof(Render));
    public static readonly StageLabel PostRender = new(nameof(PostRender));
}

public record class Stage : IDisposable
{
    StageGraph stageGraph = new();

    public StageLabel Label { get; }
    public List<ISystem> Systems { get; } = new();
    public IRunCriteria RunCriteria { get; set; } = RunAlways.Instance;
    public StageGraph StageGraph => stageGraph;

    public Stage(StageLabel label)
    {
        Label = label;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var system in Systems)
        {
            if (system is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public void AddSystem(ISystem system)
    {
        if (Systems.Find(e => e.Descriptor.Label == system.Descriptor.Label) is not null)
        {
            system.Descriptor.WithLabel($"{system.Descriptor.Label}-{RandomUtils.RandomString(Random.Shared, 16)}");
        }
        Systems.Add(system);
    }

    public void RemoveSystem(ISystem system)
    {
        Systems.Remove(system);
    }

    public void Schedule(World world)
    {
        stageGraph.Reset();
        stageGraph.Schedule(world, Systems);
    }

    public void Tick(World world)
    {
        if (!RunCriteria.ShouldRun(world)) return;

        if (stageGraph is not null)
        {
            stageGraph.Tick(world);
        }
        else
        {
            foreach (var system in Systems)
            {
                try
                {
                    if (system.ShouldRun(world))
                    {
                        system.Tick(world);
                    }
                }
                catch (Exception e)
                {
                    Log.Exception(e, $"An error occurred while running system {system.Descriptor.Label.Value} in stage {Label.Value}.");
                    throw;
                }
            }
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(Label.ToString());
        if (stageGraph != null)
        {
            sb.Append(stageGraph.ToString());
            return sb.ToString();
        }

        foreach (var system in Systems)
        {
            sb.AppendLine($"\t{system.Descriptor.Label.Value}");
        }
        return sb.ToString();
    }
}