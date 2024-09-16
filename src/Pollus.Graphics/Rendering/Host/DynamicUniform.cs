namespace Pollus.Graphics.Rendering;

public class DynamicUniform<T> : IBufferData
    where T : unmanaged, IShaderType
{
    public BufferUsage Usage => BufferUsage.CopyDst | BufferUsage.Uniform;
    public BufferType Type => BufferType.Uniform;
    public ulong SizeInBytes => Alignment.AlignedSize<T>(1);

    public T[] Data { get; init; }

    public DynamicUniform(int size)
    {
        Data = new T[size];
    }

    public void WriteTo(GPUBuffer target, int offset)
    {
        target.Write<T>(Data, offset);
    }
}