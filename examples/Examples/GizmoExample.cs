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
            .AddSystem(CoreStage.Update, FnSystem.Create(new("GizmoExample::Update"),
            static (Gizmos gizmos, Time time) =>
            {
                gizmos.DrawLine(Vec2f.One * 200f, Vec2f.One * 300f, Color.GREEN, 10f);
                gizmos.DrawRect(Vec2f.One * 100f, Vec2f.One * 50f, 0f, Color.RED, true);
                gizmos.DrawRect(Vec2f.One * 100f + Vec2f.Right * 150f, Vec2f.One * 50f, Math.Sin((float)time.SecondsSinceStartup * 0.25f) * 360f, Color.BLUE, false);
            }))
            .Build();
        app.Run();
    }

    public void Stop()
    {
        app?.Shutdown();
    }
}
