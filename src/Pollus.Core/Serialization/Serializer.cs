namespace Pollus.Core.Serialization;

public interface ISerializer
{
    public ReadOnlySpan<byte> Buffer { get; }

    void Clear();
    void Write(ReadOnlySpan<byte> data);
    void Write<T>(ReadOnlySpan<T> data) where T : unmanaged;
    void Write<T>(T value) where T : unmanaged;
    void Write<T>(T[] values) where T : unmanaged;
    void Write(string value);
}

public interface IDeserializer
{
    void Init(byte[] data);
    string ReadString();
    T Read<T>() where T : unmanaged;
    T[] ReadArray<T>() where T : unmanaged;
    void ReadTo<T>(Span<T> target) where T : unmanaged;
}
