namespace Pollus.Engine.Audio;

using Pollus.Audio;
using Pollus.Engine.Assets;

public class WavAssetLoader : AssetLoader<AudioAsset>
{
    public override string[] Extensions => [".wav"];

    protected override void Load(ReadOnlySpan<byte> data, ref LoadContext<AudioAsset> context)
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