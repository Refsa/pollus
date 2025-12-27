namespace Pollus.Audio;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Core.Assets;
using Pollus.Mathematics;
using Silk.NET.OpenAL;

public enum AudioSourceState
{
    None = 0,
    Playing,
    Paused,
    Stopped,
}

[Asset]
unsafe public partial class AudioSource : IDisposable
{
    AudioManager audio;
    uint sourceId;

    bool isDisposed;

    public AudioSourceState State
    {
        get
        {
            audio.al.GetSourceProperty(sourceId, GetSourceInteger.SourceState, out var value);
            return value switch
            {
                (int)SourceState.Playing => AudioSourceState.Playing,
                (int)SourceState.Paused => AudioSourceState.Paused,
                (int)SourceState.Stopped => AudioSourceState.Stopped,
                _ => AudioSourceState.None,
            };
        }
    }

    public SourceType Type
    {
        get
        {
            audio.al.GetSourceProperty(sourceId, GetSourceInteger.SourceType, out var value);
            return (SourceType)value;
        }
        set
        {
            audio.al.SetSourceProperty(sourceId, SourceInteger.SourceType, (int)value);
        }
    }

    public bool IsPlaying
    {
        get
        {
            audio.al.GetSourceProperty(sourceId, GetSourceInteger.BuffersQueued, out var value);
            return value == 0;
        }
    }

    public float Pitch
    {
        get
        {
            audio.al.GetSourceProperty(sourceId, SourceFloat.Pitch, out var value);
            return value;
        }
        set => audio.al.SetSourceProperty(sourceId, SourceFloat.Pitch, value);
    }

    public float Gain
    {
        get
        {
            audio.al.GetSourceProperty(sourceId, SourceFloat.Gain, out var value);
            return value;
        }
        set => audio.al.SetSourceProperty(sourceId, SourceFloat.Gain, value);
    }

    public float MinGain
    {
        get
        {
            audio.al.GetSourceProperty(sourceId, SourceFloat.MinGain, out var value);
            return value;
        }
        set => audio.al.SetSourceProperty(sourceId, SourceFloat.MinGain, value);
    }

    public float MaxGain
    {
        get
        {
            audio.al.GetSourceProperty(sourceId, SourceFloat.MaxGain, out var value);
            return value;
        }
        set => audio.al.SetSourceProperty(sourceId, SourceFloat.MaxGain, value);
    }

    public bool Looping
    {
        get
        {
            audio.al.GetSourceProperty(sourceId, SourceBoolean.Looping, out var value);
            return value;
        }
        set => audio.al.SetSourceProperty(sourceId, SourceBoolean.Looping, value);
    }

    public Vec3<float> Position
    {
        get
        {
            audio.al.GetSourceProperty(sourceId, SourceVector3.Position, out var value);
            return value;
        }
        set => audio.al.SetSourceProperty(sourceId, SourceVector3.Position, Unsafe.As<Vec3<float>, System.Numerics.Vector3>(ref value));
    }

    public Vec3<float> Velocity
    {
        get
        {
            audio.al.GetSourceProperty(sourceId, SourceVector3.Velocity, out var value);
            return value;
        }
        set => audio.al.SetSourceProperty(sourceId, SourceVector3.Velocity, Unsafe.As<Vec3<float>, System.Numerics.Vector3>(ref value));
    }

    public Vec3<float> Direction
    {
        get
        {
            audio.al.GetSourceProperty(sourceId, SourceVector3.Direction, out var value);
            return value;
        }
        set => audio.al.SetSourceProperty(sourceId, SourceVector3.Direction, Unsafe.As<Vec3<float>, System.Numerics.Vector3>(ref value));
    }

    public AudioSource(AudioManager audio)
    {
        this.audio = audio;
        sourceId = audio.al.GenSource();
    }

    ~AudioSource() => Dispose();

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);

        audio.al.DeleteSource(sourceId);
        audio.RemoveSource(this);
    }

    public void Play()
    {
        audio.al.SourcePlay(sourceId);
    }

    public void Pause()
    {
        audio.al.SourcePause(sourceId);
    }

    public void Stop()
    {
        audio.al.SourceStop(sourceId);
    }

    public void Rewind()
    {
        audio.al.SourceRewind(sourceId);
    }

    public void QueueBuffer(AudioBuffer buffer)
    {
        audio.al.SetSourceProperty(sourceId, SourceInteger.Buffer, buffer.Id);
    }

    public void UnqueueBuffer(AudioBuffer buffer)
    {
        audio.al.SetSourceProperty(sourceId, SourceInteger.Buffer, buffer.Id);
    }

    unsafe public void QueueBuffers(ReadOnlySpan<uint> bufferIds)
    {
        fixed (uint* ptr = bufferIds)
        {
            audio.al.SourceQueueBuffers(sourceId, bufferIds.Length, ptr);
        }
    }

    unsafe public void UnqueueBuffers(ReadOnlySpan<uint> bufferIds)
    {
        fixed (uint* ptr = bufferIds)
        {
            audio.al.SourceUnqueueBuffers(sourceId, bufferIds.Length, ptr);
        }
    }

    public float GetProperty(SourceFloat property)
    {
        audio.al.GetSourceProperty(sourceId, property, out var value);
        return value;
    }

    public void SetProperty(SourceFloat property, float value)
    {
        audio.al.SetSourceProperty(sourceId, property, value);
    }

    public int GetProperty(GetSourceInteger property)
    {
        audio.al.GetSourceProperty(sourceId, property, out var value);
        return value;
    }

    public void SetProperty(SourceInteger property, int value)
    {
        audio.al.SetSourceProperty(sourceId, property, value);
    }

    public bool GetProperty(SourceBoolean property)
    {
        audio.al.GetSourceProperty(sourceId, property, out var value);
        return value;
    }

    public void SetProperty(SourceBoolean property, bool value)
    {
        audio.al.SetSourceProperty(sourceId, property, value);
    }

    public Vec3<float> GetProperty(SourceVector3 property)
    {
        audio.al.GetSourceProperty(sourceId, property, out var value);
        return value;
    }

    public void SetProperty(SourceVector3 property, Vec3<float> value)
    {
        audio.al.SetSourceProperty(sourceId, property, Unsafe.As<Vec3<float>, System.Numerics.Vector3>(ref value));
    }
}