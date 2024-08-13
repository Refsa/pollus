namespace Pollus.ECS;

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
}

public class Stage
{
    public StageLabel Label { get; }
    public List<ISystem> Systems { get; } = new();

    public Stage(StageLabel label)
    {
        Label = label;
    }

    public void AddSystem(ISystem system)
    {
        Systems.Add(system);
    }

    public void RemoveSystem(ISystem system)
    {
        Systems.Remove(system);
    }

    public void Schedule()
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
        foreach (var system in Systems)
        {
            if (system.ShouldRun(world))
            {
                system.Tick(world);
            }
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder($"\tStage {Label.Label}:\n");
        foreach (var system in Systems)
        {
            sb.AppendLine($"\t\t{system.Descriptor.Label.Label}");
        }
        return sb.ToString();
    }
}