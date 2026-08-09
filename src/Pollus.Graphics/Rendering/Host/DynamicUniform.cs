namespace Pollus.Graphics.Rendering;

using System.Runtime.CompilerServices;

public class DynamicUniform<T> : IBufferData
    where T : unmanaged, IShaderType
{
    public BufferUsage Usage => BufferUsage.CopyDst | BufferUsage.Uniform;
    public BufferType Type => BufferType.Uniform;
    public ulong SizeInBytes
    {
        get
        {
            ValidateHostStride();
            return checked((ulong)T.PaddedSize * (uint)Data.Length);
        }
    }

    public T[] Data { get; init; }

    public DynamicUniform(int size)
    {
        Data = new T[size];
    }

    public void WriteTo(GPUBuffer target, int offset)
    {
        ValidateHostStride();
        target.Write<T>(Data, offset);
    }

    static void ValidateHostStride()
    {
        var hostSize = (uint)Unsafe.SizeOf<T>();
        if (hostSize != T.PaddedSize)
        {
            throw new InvalidOperationException($"{typeof(T).Name} host size {hostSize} does not match shader stride {T.PaddedSize}.");
        }
    }
}
