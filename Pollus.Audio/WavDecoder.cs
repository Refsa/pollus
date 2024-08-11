using System.Text;

namespace Pollus.Audio;

public class WavDecoder : Decoder
{
    int dataEnd;
    int bytesPerSample;
    int format;
    int size;

    public override AudioFormat Format => AudioFormat.Wav;

    public WavDecoder(string path) : base(path) { }
    public WavDecoder(Stream stream) : base(stream) { }

    protected bool CompareString(Span<byte> span, ReadOnlySpan<char> value)
    {
        Span<char> temp = stackalloc char[4];
        Encoding.UTF8.GetChars(span, temp);
        return temp.SequenceEqual(value);
    }

    protected override SampleInfo ReadInfo()
    {
        Span<byte> header = stackalloc byte[44];
        int result = stream.Read(header);

        if (CompareString(header[0..4], "RIFF") is false ||
            CompareString(header[8..12], "WAVE") is false ||
            CompareString(header[12..16], "fmt ") is false ||
            CompareString(header[36..40], "data") is false)
        {
            throw new InvalidDataException("Invalid WAV header");
        }

        var fileSize = BitConverter.ToInt32(header[4..8]);
        format = BitConverter.ToInt16(header[20..22]);
        var channels = BitConverter.ToInt16(header[22..24]);
        var sampleRate = BitConverter.ToInt32(header[24..28]);
        var byteRate = BitConverter.ToInt32(header[28..32]);
        var blockAlign = BitConverter.ToInt16(header[32..34]);
        var bitsPerSample = BitConverter.ToInt16(header[34..36]);
        var chunkSize = BitConverter.ToInt32(header[40..44]);

        bytesPerSample = bitsPerSample / 8;
        Size = chunkSize;

        dataEnd = 44 + chunkSize;

        return new SampleInfo
        {
            SampleRate = sampleRate,
            Channels = channels,
            BitsPerSample = bitsPerSample,
        };
    }

    public override long Read(Span<byte> buffer, long count)
    {
        if (stream.Position >= dataEnd) return 0;

        Span<byte> block = stackalloc byte[bytesPerSample];
        var bytesRead = 0;
        for (int i = 0; i < buffer.Length; i += bytesPerSample)
        {
            bytesRead += stream.Read(block);

            block.CopyTo(buffer[i..]);

            if (stream.Position >= dataEnd || bytesRead >= count) break;
        }

        return bytesRead;
    }
}