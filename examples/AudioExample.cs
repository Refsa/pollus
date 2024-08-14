namespace Pollus.Game;

using Pollus.Engine;
using Pollus.Mathematics;
using Pollus.ECS;
using Pollus.Engine.Audio;
using Pollus.Engine.Assets;

public class AudioExample
{
    void Setup(IApplication app)
    {
        var assetServer = app.World.Resources.Get<AssetServer>();
        Entity.With(
            new AudioSource
            {
                Playing = true,
                Gain = 1.0f,
                Pitch = 1.0f,
                Mode = PlaybackMode.Loop,
            },
            new AudioPlayback
            {
                Asset = assetServer.Load<AudioAsset>("test.wav"),
            }
        ).Spawn(app.World);

        app.World.Prepare();
        Console.WriteLine($"{app.World.Schedule}");
    }

    void Update(IApplication app)
    {
        app.World.Tick();
    }

    public void Run()
    {
        (ApplicationBuilder.Default with
        {
            World = new World()
                .AddPlugin<TimePlugin>()
                .AddPlugin(new AssetPlugin { RootPath = "assets" })
                .AddPlugin<AudioPlugin>(),
            OnSetup = Setup,
            OnUpdate = Update,
        }).Build().Run();
    }
}