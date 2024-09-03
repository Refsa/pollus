namespace Pollus.Graphics.Rendering;

using Pollus.Utils;

public class Uniform<T> : IBufferData
    where T : unmanaged, IShaderType
{
    public BufferType Usage => BufferType.Uniform;
    public ulong SizeInBytes => Alignment.AlignedSize<T>(1);

    public T Data { get; set; }

    public void WriteTo(GPUBuffer target, int offset)
    {
        target.Write(Data, offset);
    }
}
