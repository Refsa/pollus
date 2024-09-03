namespace Pollus.Engine.Audio;

using Pollus.Audio;

public class AudioAsset
{
    public required SampleInfo SampleInfo { get; init; }
    public required byte[] Data { get; init; }
}