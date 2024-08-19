namespace Pollus.Examples;

using Pollus.Engine;
using Pollus.Mathematics;
using Pollus.ECS;
using Pollus.Engine.Audio;
using Pollus.Engine.Assets;
using static Pollus.ECS.SystemBuilder;

public class AudioExample
{
    public void Run()
    {
        Application.Builder
            .AddPlugin(new AssetPlugin { RootPath = "assets" })
            .AddPlugin<TimePlugin>()
            .AddPlugin<AudioPlugin>()
            .AddSystem(CoreStage.PostInit, FnSystem("Setup",
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
            .Run();
    }
}