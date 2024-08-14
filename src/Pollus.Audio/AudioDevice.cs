namespace Pollus.Audio;

using Silk.NET.OpenAL;

unsafe class AudioDevice : IDisposable
{
    AudioManager audio;
    Context* context;
    Device* device;

    bool isDisposed;

    public AudioDevice(AudioManager audio, Context* context, Device* device)
    {
        this.audio = audio;
        this.context = context;
        this.device = device;
    }

    ~AudioDevice() => Dispose();

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);

        audio.alc.MakeContextCurrent(null);
        audio.alc.DestroyContext(context);
        audio.alc.CloseDevice(device);
    }
}
