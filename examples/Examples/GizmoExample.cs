namespace Pollus.Examples;

using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Assets;
using Pollus.Engine.Camera;
using Pollus.Engine.Debug;
using Pollus.Engine.Rendering;
using Pollus.Mathematics;
using Pollus.Utils;

public class GizmoExample : IExample
{
    public string Name => "gizmo";

    IApplication? app;

    public void Run()
    {
        app = Application.Builder
            .AddPlugins([
                new AssetPlugin() { RootPath = "assets" },
                new TimePlugin(),
                new RenderingPlugin(),
                new GizmoPlugin(),
                new PerformanceTrackerPlugin(),
            ])
            .AddSystem(CoreStage.PostInit, FnSystem.Create(new("GizmoExample::Prepare"),
            static (Commands commands) =>
            {
                commands.Spawn(Camera2D.Bundle);

            }))
            .AddSystem(CoreStage.Update, FnSystem.Create(new("GizmoExample::Update")
            {
                Locals = [Local.From(new Queue<float>())]
            },
            static (Local<Queue<float>> frameTimes, Gizmos gizmos, Time time) =>
            {
                gizmos.DrawLine(
                    new Vec2f(50f, 300f + Math.Sin((float)time.SecondsSinceStartup * 1f) * 100f),
                    new Vec2f(200f, 300f + Math.Cos((float)time.SecondsSinceStartup * 1f) * 100f),
                    Color.GREEN, 2f);

                gizmos.DrawLineString(stackalloc Vec2f[]{
                    new(500, 500),
                    new(600, 600),
                    new(700, 600),
                    new(800, 500),
                }, Color.RED, 10f);

                gizmos.DrawRect(Vec2f.One * 100f, Vec2f.One * 50f, 0f, Color.RED, true);
                gizmos.DrawRect(Vec2f.One * 100f + Vec2f.Right * 150f, Vec2f.One * 50f, Math.Sin((float)time.SecondsSinceStartup * 0.25f) * 360f, Color.BLUE, false);

                {
                    frameTimes.Value.Enqueue(time.DeltaTimeF);
                    if (frameTimes.Value.Count > 1024) frameTimes.Value.Dequeue();
                    var min = frameTimes.Value.Min() - 0.01f;
                    var max = frameTimes.Value.Max() + 0.01f;
                    var avg = frameTimes.Value.Average();

                    var bounds = Rect.FromCenterScale(new Vec2f(1000f, 400f), new Vec2f(200f, 50f));

                    gizmos.DrawRect(bounds.Center(), bounds.Extents(), 0f, Color.BLUE, false);
                    Span<Vec2f> points = stackalloc Vec2f[frameTimes.Value.Count];
                    var i = 0;
                    foreach (var frameTime in frameTimes.Value)
                    {
                        var yOffset = Math.Clamp(frameTime / avg, 0f, 1f);
                        var height = (yOffset * bounds.Height * 0.5f);

                        points[i++] = bounds.Min + new Vec2f(i * bounds.Width / (points.Length - 1), height);
                    }
                    gizmos.DrawLineString(points, Color.RED, 1f);
                }
            }))
            .Build();
        app.Run();
    }

    public void Stop()
    {
        app?.Shutdown();
    }
}
