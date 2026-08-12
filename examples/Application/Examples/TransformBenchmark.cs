namespace Pollus.Examples;

using System.Diagnostics;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Rendering;
using Pollus.Engine.Transform;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;

public class TransformBenchmark : IExample
{
    const float TargetFps = 60.0f;
    const float TargetTolerance = 0.5f;
    const int InitialAdjustmentStep = 100_000;
    const int MinimumAdjustmentStep = 100;

    public string Name => "transform-benchmark";
    IApplication? app;

    struct FPS
    {
        public Stopwatch? Stopwatch;
        public int Frames;
        public int AdjustmentStep;
        public int LastDirection;
    }

    readonly struct RestartStopwatchCommand(Stopwatch stopwatch) : ICommand
    {
        public static int Priority => int.MinValue;

        public void Execute(World world)
        {
            stopwatch.Restart();
        }
    }

    public void Run()
    {
        app = Application.Builder
            .AddPlugins([
                new RenderingPlugin(),
                new RandomPlugin(),
            ])
            .AddSystems(CoreStage.Update, FnSystem.Create("TransformBenchmark::Update",
                static (Commands commands, IWindow window, Random random, Local<FPS> fps, Query.Filter<All<Transform2D, GlobalTransform>> qTransforms) =>
                {
                    if (fps.Value.Stopwatch is null)
                    {
                        fps.Value.Stopwatch = Stopwatch.StartNew();
                        return;
                    }

                    fps.Value.Frames++;
                    var elapsed = fps.Value.Stopwatch.Elapsed.TotalSeconds;
                    if (elapsed < 2.0) return;

                    var measuredFps = fps.Value.Frames / elapsed;
                    if (fps.Value.AdjustmentStep == 0)
                    {
                        fps.Value.AdjustmentStep = InitialAdjustmentStep;
                    }

                    var direction = measuredFps switch
                    {
                        > TargetFps + TargetTolerance => 1,
                        < TargetFps - TargetTolerance => -1,
                        _ => 0,
                    };

                    if (direction != 0)
                    {
                        if (fps.Value.LastDirection != 0 && direction != fps.Value.LastDirection)
                        {
                            fps.Value.AdjustmentStep = int.Max(
                                fps.Value.AdjustmentStep / 2,
                                MinimumAdjustmentStep
                            );
                        }

                        if (direction > 0)
                        {
                            for (int i = 0; i < fps.Value.AdjustmentStep; i++)
                            {
                                commands.Spawn(Entity.With(
                                    Transform2D.Default with
                                    {
                                        Position = new Vec2f(random.NextFloat(-10_000f, 10_000f), random.NextFloat(-10_000f, 10_000f)),
                                        Scale = new Vec2f(random.NextFloat(0.5f, 2f), random.NextFloat(0.5f, 2f)),
                                        Rotation = random.NextFloat(0f, 360f),
                                        ZIndex = random.NextFloat(0f, 1024f),
                                    },
                                    GlobalTransform.Default
                                ));
                            }
                        }
                        else
                        {
                            var toDespawn = fps.Value.AdjustmentStep;
                            foreach (var entity in qTransforms)
                            {
                                if (toDespawn-- <= 0) break;
                                commands.Despawn(entity);
                            }
                        }

                        fps.Value.LastDirection = direction;
                    }

                    var msg = $"FPS: {measuredFps:F1} | Transforms: {qTransforms.EntityCount()} | Adjustment: {fps.Value.AdjustmentStep}";
                    Log.Info(msg);
                    window.SetTitle(msg);

                    fps.Value.Frames = 0;
                    commands.AddCommand(new RestartStopwatchCommand(fps.Value.Stopwatch));
                }))
            .Build();
        app.Run();
    }

    public void Stop()
    {
        app?.Shutdown();
    }
}
