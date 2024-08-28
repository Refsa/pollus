namespace Pollus.ECS;

using Pollus.Collections;
using Pollus.ECS.Core;
using System.Collections.Generic;
using System.Text;

public class Schedule : IDisposable
{
    public enum Step
    {
        Before,
        After,
    }

    public static Schedule CreateDefault()
    {
        return new()
        {
            stages =
            {
                new Stage(CoreStage.Init) with { RunCriteria = new RunOnce() },
                new Stage(CoreStage.PostInit) with { RunCriteria = new RunOnce() },

                new Stage(CoreStage.First),
                new Stage(CoreStage.Update),
                new Stage(CoreStage.Last),

                new Stage(CoreStage.PreRender),
                new Stage(CoreStage.Render),
                new Stage(CoreStage.PostRender),
            }
        };
    }

    List<Stage> stages { get; } = new();
    
    public ListEnumerable<Stage> Stages => new(stages);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var stage in stages)
        {
            stage.Dispose();
        }
    }

    public void Prepare(World world)
    {
        foreach (var stage in stages)
        {
            stage.Schedule(world);
        }
    }

    public void Tick(World world)
    {
        foreach (var stage in stages)
        {
            stage.Tick(world);
        }
    }

    public Stage? GetStage(StageLabel label)
    {
        return stages.Find(s => s.Label == label);
    }

    public Schedule AddSystems(StageLabel stageLabel, params ISystem[] systems)
    {
        var stage = GetStage(stageLabel) ?? throw new ArgumentException($"Stage {stageLabel.Label} not found");
        foreach (var system in systems)
        {
            stage.AddSystem(system);
        }
        return this;
    }

    public Schedule AddSystems(StageLabel stageLabel, params SystemBuilder[] builders)
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
            var index = stages.FindIndex(s => s.Label == before);
            stages.Insert(index, stage);
        }
        else if (after is not null)
        {
            var index = stages.FindIndex(s => s.Label == after);
            stages.Insert(index + 1, stage);
        }
        else
        {
            stages.Add(stage);
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder($"Schedule:\n");
        foreach (var stage in stages)
        {
            sb.AppendLine(stage.ToString());
        }
        return sb.ToString();
    }
}
