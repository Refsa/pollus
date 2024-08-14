namespace Pollus.Engine.Audio;

using Pollus.Audio;
using Pollus.Engine.Assets;

[Asset<AudioAsset>]
public class AudioAsset
{
    public required SampleInfo SampleInfo { get; init; }
    public required byte[] Data { get; init; }
}