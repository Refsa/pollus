namespace Pollus.Examples;

using System.Net.Http.Headers;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine;
using Pollus.Engine.Input;
using Pollus.Engine.Tween;
using Pollus.Mathematics;

public class TweenExample : IExample
{
    public string Name => "tween";

    IApplication? app;

    public void Run() => (app = Application.Builder
        .AddPlugins([
            new InputPlugin(),
            new TweenPlugin(),
        ])
        .AddSystem(CoreStage.PostInit, FnSystem.Create("Setup", static (Commands commands) =>
        {
            var ent = commands.Spawn(Entity.With(
                new TweenTestComponent()
            ));

            Tween.Create(1f, 0f, 1f)
                .OnEntity(ent)
                .OnField<TweenTestComponent>(comp => comp.Float)
                .Append(commands);
        }))
        .AddSystem(CoreStage.Update, FnSystem.Create("Update", static (Query<TweenTestComponent> query) =>
        {
            foreach (var comp in query)
            {
                if (comp.Component0.Float != 0f) Log.Info($"{comp.Component0.Float}");
            }
        }))
        .Build())
        .Run();

    public void Stop()
    {
        app?.Shutdown();
    }
}