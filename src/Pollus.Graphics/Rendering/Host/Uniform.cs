namespace Pollus.Graphics.Rendering;

using Pollus.Utils;

public class Uniform<T> : IBufferData
    where T : unmanaged
{
    public BufferType Usage => BufferType.Uniform;
    public ulong SizeInBytes => Alignment.GPUAlignedSize<T>(1);

    public T Data { get; set; }

    public void WriteTo(GPUBuffer target, int offset)
    {
        target.Write(Data, offset);
    }
}

public class DynamicUniform<T> : IBufferData
    where T : unmanaged
{
    public BufferType Usage => BufferType.Uniform;
    public ulong SizeInBytes => Alignment.GPUAlignedSize<T>((uint)Data.Length);

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