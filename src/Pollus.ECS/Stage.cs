namespace Pollus.ECS;

using Pollus.Debugging;
using Pollus.ECS.Core;
using System.Text;

public record struct StageLabel(string Label);

public static class CoreStage
{
    public static readonly StageLabel Init = new(nameof(Init));
    public static readonly StageLabel PostInit = new(nameof(PostInit));

    public static readonly StageLabel First = new(nameof(First));
    public static readonly StageLabel Update = new(nameof(Update));
    public static readonly StageLabel Last = new(nameof(Last));

    public static readonly StageLabel PreRender = new(nameof(PreRender));
    public static readonly StageLabel Render = new(nameof(Render));
    public static readonly StageLabel PostRender = new(nameof(PostRender));
}

public record class Stage : IDisposable
{
    public StageLabel Label { get; }
    public List<ISystem> Systems { get; } = new();
    public IRunCriteria RunCriteria { get; set; } = RunAlways.Instance;

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
        Systems.Add(system);
    }

    public void RemoveSystem(ISystem system)
    {
        Systems.Remove(system);
    }

    public void Schedule(World world)
    {
        Systems.Sort((a, b) =>
        {
            if (a.Descriptor.RunsBefore.Contains(b.Descriptor.Label))
            {
                return -1;
            }
            if (a.Descriptor.RunsAfter.Contains(b.Descriptor.Label))
            {
                return 1;
            }
            return 0;
        });
    }

    public void Tick(World world)
    {
        if (!RunCriteria.ShouldRun(world)) return;

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
                Log.Error(e, $"An error occurred while running system {system.Descriptor.Label.Label} in stage {Label.Label}.");
            }
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder($"\tStage {Label.Label}:\n");
        foreach (var system in Systems)
        {
            sb.AppendLine($"\t\t{system.Descriptor.Label.Label}");
            sb.AppendLine($"\t\t\tParameters: {string.Join(", ", system.Descriptor.Parameters)}");
            sb.AppendLine($"\t\t\tDependencies: {string.Join(", ", system.Descriptor.Dependencies)}");
        }
        return sb.ToString();
    }
}