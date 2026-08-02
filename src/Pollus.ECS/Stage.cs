namespace Pollus.ECS;

using System.Text;
using Pollus.Debugging;
using Pollus.Utils;

public record struct StageLabel(string Value)
{
    public override string ToString() => $"Stage<{Value}>";

    public static implicit operator StageLabel(string label) => new(label);
}

public static class CoreStage
{
    public const string Init = nameof(Init);
    public const string PostInit = nameof(PostInit);

    public const string First = nameof(First);
    public const string PreUpdate = nameof(PreUpdate);
    public const string Update = nameof(Update);
    public const string PostUpdate = nameof(PostUpdate);
    public const string Last = nameof(Last);

    public const string PreRender = nameof(PreRender);
    public const string Render = nameof(Render);
    public const string PostRender = nameof(PostRender);
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