namespace Pollus.Audio;

using System.Runtime.CompilerServices;
using Pollus.Mathematics;
using Silk.NET.OpenAL;

public enum AudioSourceState
{
    None = 0,
    Playing,
    Paused,
    Stopped,
}

public class AudioSource : IDisposable
{
    Audio audio;
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

    public Vector3<float> Position
    {
        get
        {
            audio.al.GetSourceProperty(sourceId, SourceVector3.Position, out var value);
            return value;
        }
        set => audio.al.SetSourceProperty(sourceId, SourceVector3.Position, value);
    }

    public Vector3<float> Velocity
    {
        get
        {
            audio.al.GetSourceProperty(sourceId, SourceVector3.Velocity, out var value);
            return value;
        }
        set => audio.al.SetSourceProperty(sourceId, SourceVector3.Velocity, value);
    }

    public Vector3<float> Direction
    {
        get
        {
            audio.al.GetSourceProperty(sourceId, SourceVector3.Direction, out var value);
            return value;
        }
        set => audio.al.SetSourceProperty(sourceId, SourceVector3.Direction, value);
    }

    public AudioSource(Audio audio)
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
        audio.al.SetSourceProperty(sourceId, SourceInteger.Buffer, 0);
    }

    unsafe public void QueueBuffers(ReadOnlySpan<uint> bufferIds)
    {
        fixed (uint* ptr = bufferIds)
        {
            audio.al.SourceQueueBuffers(sourceId, bufferIds.Length, ptr);
        }
    }

    unsafe public void UnqueueBuffers(Span<uint> bufferIds)
    {
        fixed (uint* ptr = bufferIds)
        {
            audio.al.SourceUnqueueBuffers(sourceId, bufferIds.Length, ptr);
        }
    }
}