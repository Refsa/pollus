namespace Pollus.Graphics.Rendering;

using Core.Assets;

[Asset]
public partial class Uniform<T> : IBufferData
    where T : unmanaged, IShaderType
{
    public BufferUsage Usage => BufferUsage.CopyDst | BufferUsage.Uniform;
    public BufferType Type => BufferType.Uniform;
    public ulong SizeInBytes => Alignment.AlignedSize<T>(1);

    T data;
    public ref T Data => ref data;

    public void WriteTo(GPUBuffer target, int offset)
    {
        target.Write(Data, offset);
    }
}
