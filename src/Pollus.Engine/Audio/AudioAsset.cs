namespace Pollus.Engine.Audio;

using Core.Assets;
using Pollus.Audio;

[Asset]
public partial class AudioAsset
{
    public required SampleInfo SampleInfo { get; init; }
    public required byte[] Data { get; init; }
}