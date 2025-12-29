using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pollus.Audio;

using Core.Assets;

[Asset]
public partial class AudioBuffer : IDisposable
{
    AudioManager audio;
    uint bufferId;

    bool isDisposed;

    public uint Id => bufferId;

    public AudioBuffer(AudioManager audio)
    {
        this.audio = audio;
        bufferId = audio.al.GenBuffer();
    }

    ~AudioBuffer() => Dispose();

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);

        audio.al.DeleteBuffer(bufferId);
        audio.RemoveBuffer(this);
    }

    unsafe public void SetData<TElement>(ReadOnlySpan<TElement> data, in SampleInfo sampleInfo)
        where TElement : unmanaged, INumber<TElement>
    {
        var format = (sampleInfo.Channels, sampleInfo.BitsPerSample) switch
        {
            (1, 8) => Silk.NET.OpenAL.BufferFormat.Mono8,
            (1, 16) => Silk.NET.OpenAL.BufferFormat.Mono16,
            (2, 8) => Silk.NET.OpenAL.BufferFormat.Stereo8,
            (2, 16) => Silk.NET.OpenAL.BufferFormat.Stereo16,
            _ => throw new NotSupportedException("Unsupported format")
        };

        var size = data.Length * Unsafe.SizeOf<TElement>();
        var bytes = MemoryMarshal.AsBytes(data);

        fixed (byte* ptr = bytes)
        {
            audio.al.BufferData(bufferId, format, ptr, size, sampleInfo.SampleRate);
        }
    }
}