namespace Pollus.ECS;

using System.Text;
using Pollus.Debugging;
using Pollus.Utils;

public class StageGraph
{
    public class Node
    {
        public List<ISystem> Systems { get; } = [];
        public Node? Next { get; set; }
    }

    public StageLabel Label { get; init; }
    public Node? First { get; set; }

    Pool<List<ISystem>> dependencyPool = new(() => new List<ISystem>(), 4);
    Dictionary<ISystem, List<ISystem>> dependencies = [];

    List<Node> nodes = [];
    Pool<Node> nodePool = new(() => new Node(), 4);

    Dictionary<ISystem, int> inDegree = [];
    HashSet<ISystem> seenSystems = [];
    Dictionary<SystemLabel, ISystem> labelToSystem = [];

    public void Reset()
    {
        First = null;
        foreach (var node in nodes)
        {
            node.Systems.Clear();
            node.Next = null;
            nodePool.Return(node);
        }

        nodes.Clear();
    }

    public void Schedule(World world, List<ISystem> systems)
    {
        if (systems.Count == 0) return;
        TopologicalSort(systems);

        labelToSystem.Clear();
        foreach (var system in systems)
        {
            labelToSystem[system.Descriptor.Label] = system;
        }

        var current = First = nodePool.Rent();
        nodes.Add(current);

        foreach (var system in systems)
        {
            if (seenSystems.Contains(system)) continue;

            bool fits = current.Systems.Count == 0 || !current.Systems.Any(s =>
                s.Descriptor.Dependencies.Overlaps(system.Descriptor.Dependencies) ||
                system.Descriptor.RunsAfter.Contains(s.Descriptor.Label) ||
                s.Descriptor.RunsBefore.Contains(system.Descriptor.Label)
            );

            if (!fits)
            {
                var next = nodePool.Rent();
                nodes.Add(next);
                current.Next = next;
                current = next;
            }

            current.Systems.Add(system);
            seenSystems.Add(system);

            foreach (var candidate in systems)
            {
                if (seenSystems.Contains(candidate)) continue;
                if (candidate.Descriptor.Dependencies.Overlaps(system.Descriptor.Dependencies)) continue;
                if (!AllPredecessorsSeen(candidate)) continue;

                if (current.Systems.Any(e =>
                        e.Descriptor.Dependencies.Overlaps(candidate.Descriptor.Dependencies) ||
                        candidate.Descriptor.RunsAfter.Contains(e.Descriptor.Label) ||
                        e.Descriptor.RunsBefore.Contains(candidate.Descriptor.Label)))
                {
                    continue;
                }

                current.Systems.Add(candidate);
                seenSystems.Add(candidate);
            }
        }

        seenSystems.Clear();
    }

    bool AllPredecessorsSeen(ISystem system)
    {
        foreach (var predLabel in system.Descriptor.RunsAfter)
        {
            if (labelToSystem.TryGetValue(predLabel, out var pred) && !seenSystems.Contains(pred))
                return false;
        }

        foreach (var (_, other) in labelToSystem)
        {
            if (other.Descriptor.RunsBefore.Contains(system.Descriptor.Label) && !seenSystems.Contains(other))
                return false;
        }

        return true;
    }

    public void TopologicalSort(List<ISystem> systems)
    {
        foreach (var system in systems)
        {
            dependencies[system] = dependencyPool.Rent();
            inDegree[system] = 0;
        }

        foreach (var system in systems)
        {
            var systemGraph = dependencies[system];
            foreach (var otherSystem in systems)
            {
                if (system == otherSystem) continue;
                if (system.Descriptor.RunsBefore.Contains(otherSystem.Descriptor.Label))
                {
                    systemGraph.Add(otherSystem);
                    inDegree[otherSystem]++;
                }
                else if (system.Descriptor.RunsAfter.Contains(otherSystem.Descriptor.Label))
                {
                    dependencies[otherSystem].Add(system);
                    inDegree[system]++;
                }
            }
        }

        var queue = new Queue<ISystem>();
        foreach (var system in systems)
        {
            if (inDegree[system] == 0) queue.Enqueue(system);
        }

        int index = 0;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            systems[index++] = current;

            foreach (var neighbor in dependencies[current])
            {
                if (--inDegree[neighbor] == 0) queue.Enqueue(neighbor);
            }
        }

        foreach (var system in dependencies)
        {
            system.Value.Clear();
            dependencyPool.Return(system.Value);
        }

        dependencies.Clear();
        inDegree.Clear();

        if (index != systems.Count)
        {
            throw new InvalidOperationException($"A cycle was detected in stage {Label.Value}.");
        }
    }

    public void Tick(World world)
    {
        var current = First;
        while (current != null)
        {
            foreach (var system in current.Systems)
            {
                if (!system.ShouldRun(world)) continue;

                try
                {
                    system.Tick(world);
                }
                catch (Exception e)
                {
                    Log.Exception(e, $"An error occurred while running system {system.Descriptor.Label.Value} in stage {Label.Value}.");
                    throw;
                }
            }

            current = current.Next;
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Label.Value);

        var current = First;
        while (current != null)
        {
            sb.AppendLine(new string(' ', 7) + "> " + string.Join(", ", current.Systems.Select(e => e.Descriptor.Label.Value)));
            current = current.Next;
        }

        return sb.ToString();
    }
}
