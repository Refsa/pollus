using System.Text;

namespace Pollus.Audio;

public class WavDecoder : Decoder
{
    public struct DecoderData
    {
        public int DataEnd;
        public int BytesPerSample;
        public int Format;
        public int Size;
        public SampleInfo Info;
    }

    DecoderData decoderData;

    public override AudioFormat Format => AudioFormat.Wav;

    public WavDecoder(string path) : base(path) { }
    public WavDecoder(Stream stream) : base(stream) { }

    protected override SampleInfo ReadInfo()
    {
        if (stream is null) throw new NullReferenceException("Stream is null");

        Span<byte> header = stackalloc byte[44];
        int result = stream.Read(header);

        decoderData = ReadHeader(header);
        return decoderData.Info;
    }

    public override long Read(Span<byte> buffer, long count)
    {
        if (stream is null) throw new NullReferenceException("Stream is null");

        if (stream.Position >= decoderData.DataEnd) return 0;

        Span<byte> block = stackalloc byte[decoderData.BytesPerSample];
        var bytesRead = 0;
        for (int i = 0; i < buffer.Length; i += decoderData.BytesPerSample)
        {
            bytesRead += stream.Read(block);

            block.CopyTo(buffer[i..]);

            if (stream.Position >= decoderData.DataEnd || bytesRead >= count) break;
        }

        return bytesRead;
    }

    static bool CompareString(ReadOnlySpan<byte> span, ReadOnlySpan<char> value)
    {
        Span<char> temp = stackalloc char[4];
        Encoding.UTF8.GetChars(span, temp);
        return temp.SequenceEqual(value);
    }

    public static DecoderData ReadHeader(ReadOnlySpan<byte> header)
    {
        if (CompareString(header[0..4], "RIFF") is false ||
            CompareString(header[8..12], "WAVE") is false ||
            CompareString(header[12..16], "fmt ") is false ||
            CompareString(header[36..40], "data") is false)
        {
            throw new InvalidDataException("Invalid WAV header");
        }

        var decoderData = new DecoderData();

        var fileSize = BitConverter.ToInt32(header[4..8]);
        decoderData.Format = BitConverter.ToInt16(header[20..22]);
        var channels = BitConverter.ToInt16(header[22..24]);
        var sampleRate = BitConverter.ToInt32(header[24..28]);
        var byteRate = BitConverter.ToInt32(header[28..32]);
        var blockAlign = BitConverter.ToInt16(header[32..34]);
        var bitsPerSample = BitConverter.ToInt16(header[34..36]);
        var chunkSize = BitConverter.ToInt32(header[40..44]);

        decoderData.BytesPerSample = bitsPerSample / 8;
        decoderData.Size = chunkSize;
        decoderData.DataEnd = 44 + chunkSize;
        decoderData.Info = new SampleInfo
        {
            SampleRate = sampleRate,
            Channels = channels,
            BitsPerSample = bitsPerSample,
        };

        return decoderData;
    }

    public static long Read(DecoderData header, ReadOnlySpan<byte> data, Span<byte> dst)
    {
        long readBytes = 0;
        data = data[44..];
        for (int i = 0; i < dst.Length; i += header.BytesPerSample)
        {
            data.Slice(i, header.BytesPerSample).CopyTo(dst[i..]);
            readBytes += header.BytesPerSample;
            if (i + header.BytesPerSample >= dst.Length) break;
        }
        return readBytes;
    }
}