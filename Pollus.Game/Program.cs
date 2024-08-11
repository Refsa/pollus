using System.Runtime.InteropServices;
using Pollus.Audio;
using Pollus.Engine;
using Pollus.Game;
using Pollus.Mathematics;

// ECSExample.Run();
// new GraphicsExample().Run();
// new InputExample().Run();

Audio? audio = null;
AudioSource? source = null;
AudioBuffer? buffer = null;

(ApplicationBuilder.Default with
{
    OnSetup = (app) =>
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
    },
    OnUpdate = (app) =>
    {
        
    }
}).Build().Run();