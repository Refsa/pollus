namespace Pollus.Engine.Audio;

using Pollus.Audio;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Mathematics;
using static Pollus.ECS.SystemBuilder;

public struct AudioSource : IComponent
{
    internal Handle<Pollus.Audio.AudioSource> DeviceSource;

    public bool Playing;
    public float Gain;
    public float Pitch;
    public PlaybackMode Mode;
}

public struct AudioPlayback : IComponent
{
    internal Handle<Pollus.Audio.AudioBuffer> DeviceBuffer;
    public Handle<AudioAsset> Asset;
}

public enum PlaybackMode
{
    Once,
    Loop,
}

public class AudioPlugin : IPlugin
{
    static AudioPlugin()
    {
        ResourceFetch<AudioManager>.Register();
        AssetsFetch<Pollus.Audio.AudioSource>.Register();
        AssetsFetch<Pollus.Audio.AudioBuffer>.Register();
    }

    public void Apply(World world)
    {
        world.Resources.Add(new AudioManager());
        world.Resources.Get<AssetServer>().AddLoader<WavAssetLoader>();

        world.Schedule.AddSystems(CoreStage.Last, [
            FnSystem("AudioUpdate", static (
                AudioManager audioManager, Assets<AudioAsset> audioAssets,
                Assets<Pollus.Audio.AudioSource> deviceSources,
                Assets<Pollus.Audio.AudioBuffer> deviceBuffers,
                Query<AudioSource, AudioPlayback> qSources) =>
            {
                qSources.ForEach((ref AudioSource source, ref AudioPlayback playback) =>
                {
                    var deviceSource = deviceSources.Get(source.DeviceSource);
                    if (deviceSource is null)
                    {
                        deviceSource = audioManager.CreateSource();
                        source.DeviceSource = deviceSources.Add(deviceSource, null);
                        deviceSource.Position = Vec3<float>.Zero;
                        deviceSource.Velocity = Vec3<float>.Zero;
                    }

                    var deviceBuffer = deviceBuffers.Get(playback.DeviceBuffer);
                    if (deviceBuffer is null)
                    {
                        deviceBuffer = audioManager.CreateBuffer();
                        playback.DeviceBuffer = deviceBuffers.Add(deviceBuffer, null);

                        var audioAsset = audioAssets.Get(playback.Asset);
                        deviceBuffer.SetData<byte>(audioAsset!.Data, audioAsset.SampleInfo);
                        deviceSource.QueueBuffer(deviceBuffer);
                    }

                    deviceSource.Gain = source.Gain;
                    deviceSource.Pitch = source.Pitch;
                    deviceSource.Looping = source.Mode == PlaybackMode.Loop;

                    if (source.Playing && deviceSource.State != AudioSourceState.Playing)
                    {
                        deviceSource.Play();
                    }
                    else if (!source.Playing && deviceSource.State == AudioSourceState.Playing)
                    {
                        deviceSource.Stop();

                    }
                });

            }).RunCriteria(new RunFixed(60)),
        ]);
    }
}