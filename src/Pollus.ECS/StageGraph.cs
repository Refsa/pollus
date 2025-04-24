namespace Pollus.ECS;

using System.Text;
using Pollus.Debugging;
using Pollus.Utils;

public class StageGraph
{
    public StageLabel Label { get; init; }
    public StageGraphNode? First { get; set; }

    Pool<List<ISystem>> dependencyPool = new(() => new List<ISystem>(), 4);
    Dictionary<ISystem, List<ISystem>> dependencies = [];
    Dictionary<ISystem, int> inDegree = [];

    HashSet<ISystem> seenSystems = [];
    List<StageGraphNode> nodes = [];
    Pool<StageGraphNode> nodePool = new(() => new StageGraphNode(), 4);

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

        var current = First = nodePool.Rent();
        nodes.Add(current);

        foreach (var system in systems)
        {
            if (seenSystems.Contains(system)) continue;
            seenSystems.Add(system);

            var compatibleSystems = systems.Where(e => !seenSystems.Contains(e) &&
                !e.Descriptor.Dependencies.Overlaps(system.Descriptor.Dependencies));

            if (!compatibleSystems.Any())
            {
                if (current.Systems.Count != 0)
                {
                    var prev = current;
                    current = nodePool.Rent();
                    nodes.Add(current);
                    prev.Next = current;
                }
            }
            current.Systems.Add(system);

            foreach (var compatibleSystem in compatibleSystems)
            {
                if (current.Systems.Any(e => e.Descriptor.Dependencies.Overlaps(compatibleSystem.Descriptor.Dependencies)))
                {
                    continue;
                }

                current.Systems.Add(compatibleSystem);
                seenSystems.Add(compatibleSystem);
            }
        }

        seenSystems.Clear();
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
            var systems = current.Systems.Where(s => s.ShouldRun(world));

            // systems.AsParallel().ForAll((system) => TickSystem(world, system));
            foreach (var system in systems)
            {
                TickSystem(world, system);
            }
            current = current.Next;
        }

        void TickSystem(World world, ISystem system)
        {
            try
            {
                system.Tick(world);
            }
            catch (Exception e)
            {
                Log.Error(e, $"An error occurred while running system {system.Descriptor.Label.Value} in stage {Label.Value}.");
                throw;
            }
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

public class StageGraphNode
{
    public List<ISystem> Systems { get; } = [];
    public StageGraphNode? Next { get; set; }
}
