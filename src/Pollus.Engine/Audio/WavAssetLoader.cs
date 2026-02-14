namespace Pollus.Engine.Audio;

using Pollus.Audio;
using Pollus.Assets;

public class WavAssetLoader : AssetLoader<AudioAsset>
{
    public override string[] Extensions => [".wav"];

    protected override void Load(ReadOnlySpan<byte> data, ref LoadContext context)
    {
        var decoderData = WavDecoder.ReadHeader(data);
        var dst = new byte[decoderData.Size];
        WavDecoder.Read(decoderData, data, dst);

        var asset = new AudioAsset
        {
            SampleInfo = decoderData.Info,
            Data = dst
        };

        context.SetAsset(asset);
    }
}