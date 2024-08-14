namespace Pollus.Game;

using Pollus.Engine;
using Pollus.Audio;
using Pollus.Mathematics;

public class AudioExample
{
    AudioSource? source = null;
    AudioBuffer? buffer = null;

    void Setup(IApplication app)
    {
        source = app.Audio.CreateSource();
        source.Looping = true;
        source.Gain = 1f;
        source.Pitch = 1f;
        source.Position = Vec3<float>.Zero;
        source.Velocity = Vec3<float>.Zero;

        using var decoder = new WavDecoder("assets/test.wav");
        var data = new byte[decoder.Size];
        var readCount = decoder.Read(data, data.Length);

        buffer = app.Audio.CreateBuffer();
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