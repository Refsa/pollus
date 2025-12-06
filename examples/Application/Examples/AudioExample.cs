namespace Pollus.Examples;

using Pollus.Engine;
using Pollus.ECS;
using Pollus.Engine.Audio;
using Pollus.Engine.Assets;

public class AudioExample : IExample
{
    public string Name => "audio";
    IApplication? application;
    public void Stop() => application?.Shutdown();

    public void Run()
    {
        application = Application.Builder
            .AddPlugin(new AssetPlugin { RootPath = "assets" })
            .AddPlugin<TimePlugin>()
            .AddPlugin<AudioPlugin>()
            .AddSystem(CoreStage.PostInit, FnSystem.Create("Setup",
            static (World world, AssetServer assetServer) =>
            {
                Entity.With(
                    new AudioSource
                    {
                        Gain = 1.0f,
                        Pitch = 1.0f,
                        Mode = PlaybackMode.Loop,
                    },
                    new AudioPlayback
                    {
                        Asset = assetServer.Load<AudioAsset>("sounds/test.wav"),
                    }
                ).Spawn(world);
            }))
            .Build();
        application.Run();
    }
}