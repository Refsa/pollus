namespace Pollus.Engine.Audio;

using Pollus.Audio;
using Pollus.Debugging;
using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Mathematics;
using Silk.NET.OpenAL;
using static Pollus.ECS.SystemBuilder;

public struct AudioSource : IComponent
{
    internal Handle<Pollus.Audio.AudioSource> DeviceSource;

    public required float Gain;
    public required float Pitch;
    public required PlaybackMode Mode;

    public AudioSource()
    {
        DeviceSource = Handle<Pollus.Audio.AudioSource>.Null;
    }
}

public struct AudioPlayback : IComponent
{
    internal Handle<Pollus.Audio.AudioBuffer> DeviceBuffer;
    internal long StartTicks;

    public required Handle<AudioAsset> Asset;

    public AudioPlayback()
    {
        DeviceBuffer = Handle<Pollus.Audio.AudioBuffer>.Null;
        StartTicks = 0;
    }
}

public enum PlaybackMode
{
    Once,
    Loop,
}


class AudioPools
{
    Stack<Handle<Pollus.Audio.AudioSource>> sources = [];
    Stack<Handle<Pollus.Audio.AudioBuffer>> buffers = [];

    public Handle<Pollus.Audio.AudioSource> GetSource()
    {
        if (sources.Count > 0)
        {
            return sources.Pop();
        }
        return Handle<Pollus.Audio.AudioSource>.Null;
    }

    public void ReturnSource(Handle<Pollus.Audio.AudioSource> source)
    {
        sources.Push(source);
    }

    public Handle<Pollus.Audio.AudioBuffer> GetBuffer()
    {
        if (buffers.Count > 0)
        {
            return buffers.Pop();
        }
        return Handle<Pollus.Audio.AudioBuffer>.Null;
    }

    public void ReturnBuffer(Handle<Pollus.Audio.AudioBuffer> buffer)
    {
        buffers.Push(buffer);
    }
}

public class AudioPlugin : IPlugin
{
    public void Apply(World world)
    {
        world.Resources.Add(new AudioManager());
        world.Resources.Add(new AudioPools());
        world.Resources.Get<AssetServer>().AddLoader<WavAssetLoader>();

        world.Schedule.AddSystems(CoreStage.Last, [
            FnSystem("AudioUpdate", static (
                Commands commands, Time time, AudioManager audioManager,
                AudioPools audioPools, Assets<AudioAsset> audioAssets,
                Assets<Pollus.Audio.AudioSource> deviceSources,
                Assets<Pollus.Audio.AudioBuffer> deviceBuffers,
                Query<AudioSource, AudioPlayback> qSources) =>
            {
                qSources.ForEach(new AudioUpdateForEach()
                {
                    Time = time,
                    Commands = commands,
                    AudioManager = audioManager,
                    AudioPools = audioPools,
                    AudioAssets = audioAssets,
                    DeviceSources = deviceSources,
                    DeviceBuffers = deviceBuffers,
                });
            }),
        ]);
    }
}

struct AudioUpdateForEach : IEntityForEach<AudioSource, AudioPlayback>
{
    public required Time Time;
    public required Commands Commands;
    public required AudioManager AudioManager;
    public required AudioPools AudioPools;
    public required Assets<AudioAsset> AudioAssets;
    public required Assets<Pollus.Audio.AudioSource> DeviceSources;
    public required Assets<Pollus.Audio.AudioBuffer> DeviceBuffers;

    public void Execute(in Entity entity, ref AudioSource source, ref AudioPlayback playback)
    {
        if (source.DeviceSource == Handle<Pollus.Audio.AudioSource>.Null)
        {
            source.DeviceSource = AudioPools.GetSource();
            if (source.DeviceSource == Handle<Pollus.Audio.AudioSource>.Null)
            {
                Log.Info("Creating new AudioSource");
                source.DeviceSource = DeviceSources.Add(AudioManager.CreateSource());
            }
        }

        if (playback.DeviceBuffer == Handle<Pollus.Audio.AudioBuffer>.Null)
        {
            playback.DeviceBuffer = AudioPools.GetBuffer();
            if (playback.DeviceBuffer == Handle<Pollus.Audio.AudioBuffer>.Null)
            {
                Log.Info("Creating new AudioBuffer");
                playback.DeviceBuffer = DeviceBuffers.Add(AudioManager.CreateBuffer());
            }
        }

        var deviceSource = DeviceSources.Get(source.DeviceSource)!;
        var deviceBuffer = DeviceBuffers.Get(playback.DeviceBuffer)!;

        if (playback.StartTicks == 0)
        {
            deviceSource.Position = Vec3<float>.Zero;
            deviceSource.Velocity = Vec3<float>.Zero;
            deviceSource.Gain = source.Gain;
            deviceSource.Pitch = source.Pitch;
            deviceSource.Looping = source.Mode == PlaybackMode.Loop;

            var audioAsset = AudioAssets.Get(playback.Asset);
            deviceBuffer.SetData<byte>(audioAsset!.Data, audioAsset.SampleInfo);
            deviceSource.QueueBuffer(deviceBuffer);
            deviceSource.Play();
            playback.StartTicks = Time.Ticks;
        }

        if (deviceSource.State is not AudioSourceState.Playing)
        {
            AudioPools.ReturnSource(source.DeviceSource);
            AudioPools.ReturnBuffer(playback.DeviceBuffer);
            Commands.Despawn(entity);
        }
    }
}