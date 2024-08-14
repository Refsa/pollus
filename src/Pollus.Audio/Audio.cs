namespace Pollus.Audio;

using System.Runtime.InteropServices;
using Pollus.Mathematics;
using Silk.NET.OpenAL;

unsafe public class AudioManager : IDisposable
{
#if BROWSER
    internal Emscripten.ALContext alc;
    internal Emscripten.AL al;
#else
    internal ALContext alc;
    internal AL al;
#endif

    internal AudioDevice device;

    List<AudioSource> sources = new();
    List<AudioBuffer> buffers = new();

    bool isDisposed;

    public AudioManager()
    {
#if BROWSER
        al = new Emscripten.AL();
        alc = new Emscripten.ALContext();
#else
        al = AL.GetApi(true);
        alc = ALContext.GetApi(true);
#endif

        var nativeDevice = alc.OpenDevice("");
        if (nativeDevice == null)
        {
            throw new Exception("Failed to open audio device");
        }
        var nativeContext = alc.CreateContext(nativeDevice, null);
        if (nativeContext is null)
        {
            alc.CloseDevice(nativeDevice);
            throw new Exception("Failed to create audio context");
        }
        alc.MakeContextCurrent(nativeContext);

        device = new AudioDevice(this, nativeContext, nativeDevice);
    }

    ~AudioManager() => Dispose();

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);

        foreach (var source in sources)
        {
            source.Dispose();
        }

        foreach (var buffer in buffers)
        {
            buffer.Dispose();
        }

        device.Dispose();
    }

    public AudioSource CreateSource()
    {
        var source = new AudioSource(this);
        sources.Add(source);
        return source;
    }

    public void RemoveSource(AudioSource source) => sources.Remove(source);

    public AudioBuffer CreateBuffer()
    {
        var buffer = new AudioBuffer(this);
        buffers.Add(buffer);
        return buffer;
    }

    public void RemoveBuffer(AudioBuffer buffer) => buffers.Remove(buffer);
}