using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;

namespace Pollus.Audio;

public enum AudioFormat
{
    Wav,
}

public struct SampleInfo
{
    public int SampleRate;
    public int Channels;
    public int BitsPerSample;
}

public abstract class Decoder : IDisposable
{
    protected Stream? stream;

    public SampleInfo Info { get; private set; }
    public long Size { get; protected set; }
    public abstract AudioFormat Format { get; }

    public Decoder(string path)
    {
        stream = File.OpenRead(path);
        Info = ReadInfo();
    }

    public Decoder(Stream stream)
    {
        this.stream = stream;
        Info = ReadInfo();
    }

    public void Dispose()
    {
        stream?.Dispose();
    }

    protected abstract SampleInfo ReadInfo();
    public abstract long Read(Span<byte> buffer, long count);
}
