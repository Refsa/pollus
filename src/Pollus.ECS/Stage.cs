namespace Pollus.ECS;

using Pollus.Debugging;
using System.Text;

public record struct StageLabel(string Label)
{
    public override string ToString() => $"Stage<{Label}>";
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
        var graph = new Dictionary<ISystem, List<ISystem>>();
        var inDegree = new Dictionary<ISystem, int>();
        foreach (var system in Systems)
        {
            graph[system] = [];
            inDegree[system] = 0;
        }

        foreach (var system in Systems)
        {
            var systemGraph = graph[system];
            foreach (var otherSystem in Systems)
            {
                if (system == otherSystem) continue;
                if (system.Descriptor.RunsBefore.Contains(otherSystem.Descriptor.Label))
                {
                    systemGraph.Add(otherSystem);
                    inDegree[otherSystem]++;
                }
                else if (system.Descriptor.RunsAfter.Contains(otherSystem.Descriptor.Label))
                {
                    graph[otherSystem].Add(system);
                    inDegree[system]++;
                }
            }
        }

        var queue = new Queue<ISystem>();
        foreach (var system in Systems)
        {
            if (inDegree[system] == 0) queue.Enqueue(system);
        }

        int index = 0;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            Systems[index++] = current;

            foreach (var neighbor in graph[current])
            {
                if (--inDegree[neighbor] == 0) queue.Enqueue(neighbor);
            }
        }

        if (index != Systems.Count)
        {
            throw new InvalidOperationException($"A cycle was detected in stage {Label.Label}.");
        }
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
                throw;
            }
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(Label.ToString());
        foreach (var system in Systems)
        {
            sb.AppendLine($"\t{system.Descriptor.Label.Label}");
            // sb.AppendLine($"\t\tParameters: {string.Join(", ", system.Descriptor.Parameters)}");
            // sb.AppendLine($"\t\tDependencies: {string.Join(", ", system.Descriptor.Dependencies)}");
            // if (system.Descriptor.RunsBefore.Count > 0)
                // sb.AppendLine($"\t\tRuns Before: {string.Join(", ", system.Descriptor.RunsBefore)}");
            // if (system.Descriptor.RunsAfter.Count > 0)
                // sb.AppendLine($"\t\tRuns After: {string.Join(", ", system.Descriptor.RunsAfter)}");
        }
        return sb.ToString();
    }
}