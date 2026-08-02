namespace Pollus.Examples;

using System.Diagnostics;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Debug;
using Pollus.Input;

public partial class SystemGroupExample : IExample
{
    public string Name => "system-group";

    enum SystemGroups
    {
        Group1,
        Group2,
    }

    IApplication? app;

    public void Run()
    {
        app = Application.Builder
            .AddPlugins([
                new TimePlugin(),
                new PerformanceTrackerPlugin(),
                new StatePlugin<SystemGroups>(),
                new InputPlugin(),
            ])
            .AddSystemSet<SystemGroup1>()
            .AddSystemSet<SystemGroup2>()
            .AddSystem(CoreStage.Update, FnSystem.Create("ToggleState", static (State<SystemGroups> state, ButtonInput<Key> keys) =>
            {
                if (keys.JustPressed(Key.Space))
                {
                    state.Set(state.Current switch
                    {
                        SystemGroups.Group1 => SystemGroups.Group2,
                        SystemGroups.Group2 => SystemGroups.Group1,
                        _ => throw new UnreachableException(),
                    });
                }
            }))
            .Build();
        app.Run();
    }

    public void Stop()
    {
        app?.Shutdown();
    }

    [SystemSet(SystemGroups.Group1)]
    partial class SystemGroup1
    {
        [System(nameof(CoreStage.Update)), RunCriteria<RunFixed>(1f)]
        static void SystemInGroup1(Time time)
        {
            Log.Info("Group 1");
        }
    }

    [SystemSet(SystemGroups.Group2)]
    partial class SystemGroup2
    {
        [System(nameof(CoreStage.Update))]
        static void SystemInGroup2(Time time)
        {
            if (time.FrameCount % 100 == 0)
            {
                Log.Info("Group 2");
            }
        }
    }
}
