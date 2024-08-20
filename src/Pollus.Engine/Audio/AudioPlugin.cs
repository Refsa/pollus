namespace Pollus.Engine.Audio;

using Pollus.Audio;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Mathematics;
using static Pollus.ECS.SystemBuilder;

public struct AudioSource : IComponent
{
    internal Handle<Pollus.Audio.AudioSource> DeviceSource;

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

class AudioPools
{
    Stack<Pollus.Audio.AudioSource> sources = [];
    Stack<Pollus.Audio.AudioBuffer> buffers = [];

    AudioManager audioManager;

    public AudioPools(AudioManager audioManager)
    {
        this.audioManager = audioManager;
    }

    public Pollus.Audio.AudioSource CreateSource()
    {
        if (sources.Count > 0)
        {
            return sources.Pop();
        }
        Log.Info("Creating new audio source");
        return audioManager.CreateSource();
    }

    public void ReturnSource(Pollus.Audio.AudioSource source)
    {
        sources.Push(source);
    }

    public Pollus.Audio.AudioBuffer CreateBuffer()
    {
        if (buffers.Count > 0)
        {
            return buffers.Pop();
        }
        return audioManager.CreateBuffer();
    }

    public void ReturnBuffer(Pollus.Audio.AudioBuffer buffer)
    {
        buffers.Push(buffer);
    }
}

public class AudioPlugin : IPlugin
{
    static AudioPlugin()
    {
        ResourceFetch<AudioManager>.Register();
        ResourceFetch<AudioPools>.Register();
        AssetsFetch<Pollus.Audio.AudioSource>.Register();
        AssetsFetch<Pollus.Audio.AudioBuffer>.Register();
        AssetsFetch<AudioAsset>.Register();
    }

    public void Apply(World world)
    {
        var audioManager = new AudioManager();
        world.Resources.Add(audioManager);
        world.Resources.Add(new AudioPools(audioManager));
        world.Resources.Get<AssetServer>().AddLoader<WavAssetLoader>();

        world.Schedule.AddSystems(CoreStage.Last, [
            FnSystem("AudioUpdate", static (
                World world,
                AudioPools audioPools, Assets<AudioAsset> audioAssets,
                Assets<Pollus.Audio.AudioSource> deviceSources,
                Assets<Pollus.Audio.AudioBuffer> deviceBuffers,
                Query<AudioSource, AudioPlayback> qSources) =>
            {
                var completedSources = new List<Entity>();

                qSources.ForEach((in Entity entity, ref AudioSource source, ref AudioPlayback playback) =>
                {
                    var deviceSource = deviceSources.Get(source.DeviceSource);

                    if (deviceSource is null)
                    {
                        deviceSource = audioPools.CreateSource();
                        source.DeviceSource = deviceSources.Add(deviceSource, null);
                        deviceSource.Position = Vec3<float>.Zero;
                        deviceSource.Velocity = Vec3<float>.Zero;
                        deviceSource.Gain = source.Gain;
                        deviceSource.Pitch = source.Pitch;
                        deviceSource.Looping = source.Mode == PlaybackMode.Loop;
                    }

                    var deviceBuffer = deviceBuffers.Get(playback.DeviceBuffer);
                    if (deviceBuffer is null)
                    {
                        deviceBuffer = audioPools.CreateBuffer();
                        playback.DeviceBuffer = deviceBuffers.Add(deviceBuffer, null);
                        var audioAsset = audioAssets.Get(playback.Asset);
                        deviceBuffer.SetData<byte>(audioAsset!.Data, audioAsset.SampleInfo);
                        deviceSource.QueueBuffer(deviceBuffer);
                        deviceSource.Play();
                        return;
                    }

                    if (!deviceSource.IsPlaying)
                    {   
                        audioPools.ReturnSource(deviceSource);
                        audioPools.ReturnBuffer(deviceBuffer);
                        completedSources.Add(entity);
                    }
                });

                foreach (var entity in completedSources)
                {
                    world.Despawn(entity);
                }
            }),
        ]);
    }
}