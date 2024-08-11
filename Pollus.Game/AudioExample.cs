namespace Pollus.Game;

using Pollus.Engine;
using Pollus.Audio;
using Pollus.Mathematics;

public class AudioExample
{
    Audio? audio = null;
    AudioSource? source = null;
    AudioBuffer? buffer = null;

    ~AudioExample()
    {
        audio?.Dispose();
    }

    void Setup(IApplication application)
    {
        audio = new Audio();

        source = audio.CreateSource();
        source.Looping = true;
        source.Gain = 1f;
        source.Pitch = 1f;
        source.Position = Vector3<float>.Zero;
        source.Velocity = Vector3<float>.Zero;

        using var decoder = new WavDecoder("assets/test.wav");
        var data = new byte[decoder.Size];
        var readCount = decoder.Read(data, data.Length);

        buffer = audio.CreateBuffer();
        buffer.SetData<byte>(data, decoder.Info);
        source.QueueBuffer(buffer);
        source.Play();
    }

    void Update(IApplication application)
    {

    }

    public void Run()
    {
        (ApplicationBuilder.Default with
        {
            OnSetup = Setup,
            OnUpdate = Update,
        }).Build().Run();
    }
}