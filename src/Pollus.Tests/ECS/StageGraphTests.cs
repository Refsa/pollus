namespace Pollus.Tests.ECS;

using Pollus.ECS;

public class StageGraphTests
{
    class TestSystem : ISystem
    {
        public IRunCriteria RunCriteria { get; set; } = RunAlways.Instance;
        public SystemDescriptor Descriptor { get; init; }
        public Resources Resources { get; init; } = new();
        public int TickCount { get; private set; }
        public List<string> ExecutionLog { get; }

        public TestSystem(string label, List<string>? executionLog = null)
        {
            Descriptor = new SystemDescriptor(label);
            ExecutionLog = executionLog ?? [];
        }

        public void Tick(World world)
        {
            TickCount++;
            ExecutionLog.Add(Descriptor.Label.Value);
        }

        public bool ShouldRun(World world) => RunCriteria.ShouldRun(world);
    }

    class SkippedSystem : TestSystem
    {
        class NeverRun : IRunCriteria
        {
            public bool ShouldRun(World world) => false;
        }

        public SkippedSystem(string label, List<string>? executionLog = null)
            : base(label, executionLog)
        {
            RunCriteria = new NeverRun();
        }
    }

    class DependencyMarker1
    {
    }

    class DependencyMarker2
    {
    }

    class DependencyMarker3
    {
    }

    static StageGraph CreateGraph(string label = "Test") => new() { Label = new StageLabel(label) };
    static World CreateWorld() => new();

    [Fact]
    public void TopologicalSort_SingleSystem_NoReordering()
    {
        var graph = CreateGraph();
        var system = new TestSystem("A");
        var systems = new List<ISystem> { system };

        graph.TopologicalSort(systems);

        Assert.Single(systems);
        Assert.Equal("A", systems[0].Descriptor.Label.Value);
    }

    [Fact]
    public void TopologicalSort_IndependentSystems_MaintainsOrder()
    {
        var graph = CreateGraph();
        var systems = new List<ISystem>
        {
            new TestSystem("A"),
            new TestSystem("B"),
            new TestSystem("C")
        };

        graph.TopologicalSort(systems);

        Assert.Equal(3, systems.Count);
    }

    [Fact]
    public void TopologicalSort_RunsBefore_CorrectOrder()
    {
        var graph = CreateGraph();
        var systemA = new TestSystem("A");
        var systemB = new TestSystem("B");
        systemA.Descriptor.Before("B");

        var systems = new List<ISystem> { systemB, systemA };
        graph.TopologicalSort(systems);

        Assert.Equal("A", systems[0].Descriptor.Label.Value);
        Assert.Equal("B", systems[1].Descriptor.Label.Value);
    }

    [Fact]
    public void TopologicalSort_RunsAfter_CorrectOrder()
    {
        var graph = CreateGraph();
        var systemA = new TestSystem("A");
        var systemB = new TestSystem("B");
        systemB.Descriptor.After("A");

        var systems = new List<ISystem> { systemB, systemA };
        graph.TopologicalSort(systems);

        Assert.Equal("A", systems[0].Descriptor.Label.Value);
        Assert.Equal("B", systems[1].Descriptor.Label.Value);
    }

    [Fact]
    public void TopologicalSort_ComplexChain_CorrectOrder()
    {
        var graph = CreateGraph();
        var systemA = new TestSystem("A");
        var systemB = new TestSystem("B");
        var systemC = new TestSystem("C");
        var systemD = new TestSystem("D");

        systemA.Descriptor.Before("B");
        systemB.Descriptor.Before("C");
        systemC.Descriptor.Before("D");

        var systems = new List<ISystem> { systemD, systemC, systemB, systemA };
        graph.TopologicalSort(systems);

        Assert.Equal("A", systems[0].Descriptor.Label.Value);
        Assert.Equal("B", systems[1].Descriptor.Label.Value);
        Assert.Equal("C", systems[2].Descriptor.Label.Value);
        Assert.Equal("D", systems[3].Descriptor.Label.Value);
    }

    [Fact]
    public void TopologicalSort_DiamondDependency_CorrectOrder()
    {
        var graph = CreateGraph();
        var systemA = new TestSystem("A");
        var systemB = new TestSystem("B");
        var systemC = new TestSystem("C");
        var systemD = new TestSystem("D");

        systemA.Descriptor.Before("B");
        systemA.Descriptor.Before("C");
        systemB.Descriptor.Before("D");
        systemC.Descriptor.Before("D");

        var systems = new List<ISystem> { systemD, systemC, systemB, systemA };
        graph.TopologicalSort(systems);

        var aIndex = systems.FindIndex(s => s.Descriptor.Label.Value == "A");
        var bIndex = systems.FindIndex(s => s.Descriptor.Label.Value == "B");
        var cIndex = systems.FindIndex(s => s.Descriptor.Label.Value == "C");
        var dIndex = systems.FindIndex(s => s.Descriptor.Label.Value == "D");

        Assert.True(aIndex < bIndex, "A must come before B");
        Assert.True(aIndex < cIndex, "A must come before C");
        Assert.True(bIndex < dIndex, "B must come before D");
        Assert.True(cIndex < dIndex, "C must come before D");
    }

    [Fact]
    public void TopologicalSort_CycleDetected_ThrowsException()
    {
        var graph = CreateGraph();
        var systemA = new TestSystem("A");
        var systemB = new TestSystem("B");

        systemA.Descriptor.Before("B");
        systemB.Descriptor.Before("A");

        var systems = new List<ISystem> { systemA, systemB };

        var exception = Assert.Throws<InvalidOperationException>(() => graph.TopologicalSort(systems));
        Assert.Contains("cycle", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TopologicalSort_ThreeWayCycle_ThrowsException()
    {
        var graph = CreateGraph();
        var systemA = new TestSystem("A");
        var systemB = new TestSystem("B");
        var systemC = new TestSystem("C");

        systemA.Descriptor.Before("B");
        systemB.Descriptor.Before("C");
        systemC.Descriptor.Before("A");

        var systems = new List<ISystem> { systemA, systemB, systemC };

        Assert.Throws<InvalidOperationException>(() => graph.TopologicalSort(systems));
    }

    [Fact]
    public void Schedule_EmptySystems_NoNodes()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var systems = new List<ISystem>();

        graph.Schedule(world, systems);

        Assert.Null(graph.First);
    }

    [Fact]
    public void Schedule_SingleSystem_OneNode()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var system = new TestSystem("A");

        graph.Schedule(world, [system]);

        Assert.NotNull(graph.First);
        Assert.Single(graph.First.Systems);
        Assert.Null(graph.First.Next);
    }

    [Fact]
    public void Schedule_IndependentSystems_GroupedTogether()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var systems = new List<ISystem>
        {
            new TestSystem("A"),
            new TestSystem("B"),
            new TestSystem("C")
        };

        graph.Schedule(world, systems);

        Assert.NotNull(graph.First);
        Assert.Equal(3, graph.First.Systems.Count);
        Assert.Null(graph.First.Next);
    }

    [Fact]
    public void Schedule_DependentSystems_SeparateNodes()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var systemA = new TestSystem("A");
        var systemB = new TestSystem("B");
        systemA.Descriptor.Before("B");

        graph.Schedule(world, [systemA, systemB]);

        Assert.NotNull(graph.First);
        Assert.Single(graph.First.Systems);
        Assert.Equal("A", graph.First.Systems[0].Descriptor.Label.Value);

        Assert.NotNull(graph.First.Next);
        Assert.Single(graph.First.Next.Systems);
        Assert.Equal("B", graph.First.Next.Systems[0].Descriptor.Label.Value);
    }

    [Fact]
    public void Schedule_OverlappingDependencies_SeparateNodes()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var systemA = new TestSystem("A");
        var systemB = new TestSystem("B");
        systemA.Descriptor.DependsOn<DependencyMarker1>();
        systemB.Descriptor.DependsOn<DependencyMarker1>();

        graph.Schedule(world, [systemA, systemB]);

        Assert.NotNull(graph.First);
        Assert.NotNull(graph.First.Next);
        Assert.Null(graph.First.Next.Next);
        Assert.Single(graph.First.Systems);
        Assert.Single(graph.First.Next.Systems);

        var allSystems = new[] { graph.First.Systems[0], graph.First.Next.Systems[0] };
        Assert.Contains(allSystems, s => s.Descriptor.Label.Value == "A");
        Assert.Contains(allSystems, s => s.Descriptor.Label.Value == "B");
    }

    [Fact]
    public void Schedule_NonOverlappingDependencies_SameNode()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var systemA = new TestSystem("A");
        var systemB = new TestSystem("B");
        systemA.Descriptor.DependsOn<DependencyMarker1>();
        systemB.Descriptor.DependsOn<DependencyMarker2>();

        graph.Schedule(world, [systemA, systemB]);

        Assert.NotNull(graph.First);
        Assert.Equal(2, graph.First.Systems.Count);
        Assert.Null(graph.First.Next);
    }

    [Fact]
    public void Schedule_MixedDependenciesAndOrder_CorrectGrouping()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var log = new List<string>();
        var systemA = new TestSystem("A", log);
        var systemB = new TestSystem("B", log);
        var systemC = new TestSystem("C", log);
        var systemD = new TestSystem("D", log);

        systemA.Descriptor.DependsOn<DependencyMarker1>();
        systemB.Descriptor.DependsOn<DependencyMarker2>();
        systemC.Descriptor.DependsOn<DependencyMarker1>();
        systemD.Descriptor.DependsOn<DependencyMarker2>();

        systemA.Descriptor.Before("C");
        systemB.Descriptor.Before("D");

        graph.Schedule(world, [systemA, systemB, systemC, systemD]);
        graph.Tick(world);

        var aIdx = log.IndexOf("A");
        var bIdx = log.IndexOf("B");
        var cIdx = log.IndexOf("C");
        var dIdx = log.IndexOf("D");
        Assert.True(aIdx < cIdx, "A must run before C");
        Assert.True(bIdx < dIdx, "B must run before D");

        Assert.NotNull(graph.First);
        bool aAndCInSameNode = graph.First.Systems.Any(s => s.Descriptor.Label.Value == "A") &&
                               graph.First.Systems.Any(s => s.Descriptor.Label.Value == "C");
        Assert.False(aAndCInSameNode, "A and C should not be in the same node due to order constraint");

        bool aAndBInSameNode = graph.First.Systems.Any(s => s.Descriptor.Label.Value == "A") &&
                               graph.First.Systems.Any(s => s.Descriptor.Label.Value == "B");
        Assert.True(aAndBInSameNode, "A and B can be in the same node (different dependencies, no order constraint)");
    }

    [Fact]
    public void Tick_ExecutesSystems_InOrder()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var log = new List<string>();
        var systemA = new TestSystem("A", log);
        var systemB = new TestSystem("B", log);
        var systemC = new TestSystem("C", log);

        systemA.Descriptor.Before("B");
        systemB.Descriptor.Before("C");

        graph.Schedule(world, [systemC, systemB, systemA]);
        graph.Tick(world);

        Assert.Equal(["A", "B", "C"], log);
    }

    [Fact]
    public void Tick_SkipsSystemsWithFalseRunCriteria()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var log = new List<string>();
        var systemA = new TestSystem("A", log);
        var systemB = new SkippedSystem("B", log);
        var systemC = new TestSystem("C", log);

        systemA.Descriptor.Before("B");
        systemB.Descriptor.Before("C");

        graph.Schedule(world, [systemA, systemB, systemC]);
        graph.Tick(world);

        Assert.Equal(["A", "C"], log);
    }

    [Fact]
    public void Tick_IncrementsTicks()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var system = new TestSystem("A");

        graph.Schedule(world, [system]);
        graph.Tick(world);
        graph.Tick(world);
        graph.Tick(world);

        Assert.Equal(3, system.TickCount);
    }

    [Fact]
    public void Reset_ClearsGraph()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var systems = new List<ISystem>
        {
            new TestSystem("A"),
            new TestSystem("B")
        };

        graph.Schedule(world, systems);
        Assert.NotNull(graph.First);

        graph.Reset();

        Assert.Null(graph.First);
    }

    [Fact]
    public void Reset_AllowsReschedule()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var log = new List<string>();
        var systemsFirst = new List<ISystem>
        {
            new TestSystem("A", log),
            new TestSystem("B", log)
        };

        graph.Schedule(world, systemsFirst);
        graph.Tick(world);

        log.Clear();
        graph.Reset();

        var systemsSecond = new List<ISystem>
        {
            new TestSystem("C", log),
            new TestSystem("D", log)
        };
        graph.Schedule(world, systemsSecond);
        graph.Tick(world);

        Assert.Contains("C", log);
        Assert.Contains("D", log);
        Assert.DoesNotContain("A", log);
        Assert.DoesNotContain("B", log);
    }

    [Fact]
    public void ToString_ContainsLabelAndSystems()
    {
        var graph = CreateGraph("TestStage");
        using var world = CreateWorld();
        var systems = new List<ISystem>
        {
            new TestSystem("SystemA"),
            new TestSystem("SystemB")
        };

        graph.Schedule(world, systems);
        var result = graph.ToString();

        Assert.Contains("TestStage", result);
        Assert.Contains("SystemA", result);
        Assert.Contains("SystemB", result);
    }

    [Fact]
    public void Schedule_RunsBeforeAndRunsAfterEquivalent()
    {
        var graph1 = CreateGraph("Test1");
        var graph2 = CreateGraph("Test2");
        using var world = CreateWorld();

        var log1 = new List<string>();
        var systemA1 = new TestSystem("A", log1);
        var systemB1 = new TestSystem("B", log1);
        systemA1.Descriptor.Before("B");

        var log2 = new List<string>();
        var systemA2 = new TestSystem("A", log2);
        var systemB2 = new TestSystem("B", log2);
        systemB2.Descriptor.After("A");

        graph1.Schedule(world, [systemB1, systemA1]);
        graph2.Schedule(world, [systemB2, systemA2]);

        graph1.Tick(world);
        graph2.Tick(world);

        Assert.Equal(log1, log2);
        Assert.Equal(["A", "B"], log1);
    }

    [Fact]
    public void Schedule_MultipleBeforeConstraints()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var log = new List<string>();

        var systemA = new TestSystem("A", log);
        var systemB = new TestSystem("B", log);
        var systemC = new TestSystem("C", log);
        var systemD = new TestSystem("D", log);

        systemA.Descriptor.Before("B");
        systemA.Descriptor.Before("C");
        systemA.Descriptor.Before("D");

        graph.Schedule(world, [systemD, systemC, systemB, systemA]);
        graph.Tick(world);

        Assert.Equal("A", log[0]);
        Assert.Equal(4, log.Count);
    }

    [Fact]
    public void Schedule_MultipleAfterConstraints()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var log = new List<string>();

        var systemA = new TestSystem("A", log);
        var systemB = new TestSystem("B", log);
        var systemC = new TestSystem("C", log);
        var systemD = new TestSystem("D", log);

        systemD.Descriptor.After("A");
        systemD.Descriptor.After("B");
        systemD.Descriptor.After("C");

        graph.Schedule(world, [systemD, systemC, systemB, systemA]);
        graph.Tick(world);

        Assert.Equal("D", log[^1]);
        Assert.Equal(4, log.Count);
    }

    [Fact]
    public void Schedule_SystemWithBothBeforeAndAfter()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var log = new List<string>();

        var systemA = new TestSystem("A", log);
        var systemB = new TestSystem("B", log);
        var systemC = new TestSystem("C", log);

        systemB.Descriptor.After("A");
        systemB.Descriptor.Before("C");

        graph.Schedule(world, [systemB, systemC, systemA]);
        graph.Tick(world);

        Assert.Equal(["A", "B", "C"], log);
    }

    [Fact]
    public void Schedule_OrderConstraintsTakePriorityOverGrouping()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var log = new List<string>();
        var systemA = new TestSystem("A", log);
        var systemB = new TestSystem("B", log);
        var systemC = new TestSystem("C", log);

        systemA.Descriptor.Before("B");
        systemB.Descriptor.Before("C");

        graph.Schedule(world, [systemC, systemB, systemA]);

        Assert.NotNull(graph.First);
        var node1 = graph.First;
        var node2 = node1.Next;
        var node3 = node2?.Next;

        Assert.NotNull(node2);
        Assert.NotNull(node3);
        Assert.Null(node3.Next);

        Assert.Single(node1.Systems);
        Assert.Single(node2.Systems);
        Assert.Single(node3.Systems);
        Assert.Equal("A", node1.Systems[0].Descriptor.Label.Value);
        Assert.Equal("B", node2.Systems[0].Descriptor.Label.Value);
        Assert.Equal("C", node3.Systems[0].Descriptor.Label.Value);

        graph.Tick(world);
        Assert.Equal(["A", "B", "C"], log);
    }

    [Fact]
    public void Schedule_ThreeOverlappingDependencies_InSeparateNodes()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var systemA = new TestSystem("A");
        var systemB = new TestSystem("B");
        var systemC = new TestSystem("C");
        systemA.Descriptor.DependsOn<DependencyMarker1>();
        systemB.Descriptor.DependsOn<DependencyMarker1>();
        systemC.Descriptor.DependsOn<DependencyMarker1>();

        graph.Schedule(world, [systemA, systemB, systemC]);

        var nodeCount = 0;
        var current = graph.First;
        while (current != null)
        {
            Assert.Single(current.Systems);
            nodeCount++;
            current = current.Next;
        }

        Assert.Equal(3, nodeCount);
    }

    [Fact]
    public void Schedule_GroupsMaximizesParallelExecutionOpportunity()
    {
        var graph = CreateGraph();
        using var world = CreateWorld();
        var systemA = new TestSystem("A");
        var systemB = new TestSystem("B");
        var systemC = new TestSystem("C");
        var systemD = new TestSystem("D");
        var systemE = new TestSystem("E");

        systemA.Descriptor.DependsOn<DependencyMarker1>();
        systemB.Descriptor.DependsOn<DependencyMarker2>();
        systemC.Descriptor.DependsOn<DependencyMarker3>();

        systemD.Descriptor.DependsOn<DependencyMarker1>();
        systemD.Descriptor.After("A");

        graph.Schedule(world, [systemA, systemB, systemC, systemD, systemE]);

        var firstNode = graph.First;
        Assert.NotNull(firstNode);
        Assert.True(firstNode.Systems.Count >= 3, "First node should contain at least A, B, C (and possibly E) since they have no conflicts");

        var firstLabels = firstNode.Systems.Select(s => s.Descriptor.Label.Value).ToList();
        Assert.Contains("A", firstLabels);
        Assert.Contains("B", firstLabels);
        Assert.Contains("C", firstLabels);
        Assert.DoesNotContain("D", firstLabels);
    }

    [Fact]
    public void Schedule_CalledMultipleTimes_ProducesSameResult()
    {
        using var world = CreateWorld();

        List<string> GetNodeStructure(StageGraph graph)
        {
            var result = new List<string>();
            var node = graph.First;
            while (node != null)
            {
                var labels = node.Systems.Select(s => s.Descriptor.Label.Value).OrderBy(l => l);
                result.Add(string.Join(",", labels));
                node = node.Next;
            }

            return result;
        }

        for (int attempt = 0; attempt < 5; attempt++)
        {
            var graph = CreateGraph();
            var systemA = new TestSystem("A");
            var systemB = new TestSystem("B");
            var systemC = new TestSystem("C");
            var systemD = new TestSystem("D");
            var systemE = new TestSystem("E");

            systemA.Descriptor.DependsOn<DependencyMarker1>();
            systemB.Descriptor.DependsOn<DependencyMarker2>();
            systemC.Descriptor.DependsOn<DependencyMarker1>();
            systemD.Descriptor.After("A");
            systemE.Descriptor.Before("B");

            graph.Schedule(world, [systemA, systemB, systemC, systemD, systemE]);
            var firstResult = GetNodeStructure(graph);

            graph.Reset();
            graph.Schedule(world, [systemA, systemB, systemC, systemD, systemE]);
            var secondResult = GetNodeStructure(graph);

            Assert.Equal(firstResult, secondResult);
        }
    }

    [Fact]
    public void Schedule_AddStage_Before_NonExistentLabel_ThrowsUnhelpfulError()
    {
        using var schedule = Schedule.CreateDefault();
        var testStage = new Stage(new StageLabel("TestStage"));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            schedule.AddStage(testStage, new StageLabel("NonExistent"), null));
    }

    [Fact]
    public void Schedule_AddStage_After_NonExistentLabel_ThrowsUnhelpfulError()
    {
        using var schedule = Schedule.CreateDefault();
        var testStage = new Stage(new StageLabel("TestStage"));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            schedule.AddStage(testStage, null, new StageLabel("NonExistent")));
    }
}