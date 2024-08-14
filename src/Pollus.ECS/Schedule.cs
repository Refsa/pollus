namespace Pollus.ECS;

using Pollus.ECS.Core;
using System.Text;

public class Schedule
{
    public static Schedule CreateDefault()
    {
        return new()
        {
            Stages =
            {
                new Stage(CoreStage.Init) with { RunCriteria = new RunOnce() },
                new Stage(CoreStage.PostInit) with { RunCriteria = new RunOnce() },

                new Stage(CoreStage.First),
                new Stage(CoreStage.Update),
                new Stage(CoreStage.Last),
            }
        };
    }

    public List<Stage> Stages { get; } = new();

    public void Prepare(World world)
    {
        foreach (var stage in Stages)
        {
            stage.Schedule(world);
        }
    }

    public void Tick(World world)
    {
        foreach (var stage in Stages)
        {
            stage.Tick(world);
        }
    }

    public Stage? GetStage(StageLabel label)
    {
        return Stages.Find(s => s.Label == label);
    }

    public Schedule AddSystem(StageLabel stageLabel, params ISystem[] systems)
    {
        var stage = GetStage(stageLabel) ?? throw new ArgumentException($"Stage {stageLabel.Label} not found");
        foreach (var system in systems)
        {
            stage.AddSystem(system);
        }
        return this;
    }

    public Schedule AddSystem(StageLabel stageLabel, params SystemBuilder[] builders)
    {
        var stage = GetStage(stageLabel) ?? throw new ArgumentException($"Stage {stageLabel.Label} not found");
        foreach (var builder in builders)
        {
            var system = builder.Build();
            stage.AddSystem(system);
        }
        return this;
    }

    public void AddStage(Stage stage, StageLabel? before, StageLabel? after)
    {
        if (before is not null)
        {
            var index = Stages.FindIndex(s => s.Label == before);
            Stages.Insert(index, stage);
        }
        else if (after is not null)
        {
            var index = Stages.FindIndex(s => s.Label == after);
            Stages.Insert(index + 1, stage);
        }
        else
        {
            Stages.Add(stage);
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder($"Schedule:\n");
        foreach (var stage in Stages)
        {
            sb.AppendLine(stage.ToString());
        }
        return sb.ToString();
    }
}