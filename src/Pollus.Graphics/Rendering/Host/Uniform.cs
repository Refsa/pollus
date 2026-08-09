namespace Pollus.Graphics.Rendering;

using Core.Assets;
using System.Runtime.CompilerServices;

[Asset]
public partial class Uniform<T> : IBufferData
    where T : unmanaged, IShaderType
{
    public BufferUsage Usage => BufferUsage.CopyDst | BufferUsage.Uniform;
    public BufferType Type => BufferType.Uniform;
    public ulong SizeInBytes
    {
        get
        {
            ValidateHostLayout();
            return T.PaddedSize;
        }
    }

    T data;
    public ref T Data => ref data;

    public void WriteTo(GPUBuffer target, int offset)
    {
        ValidateHostLayout();
        target.Write(Data, offset);
    }

    static void ValidateHostLayout()
    {
        var hostSize = (uint)Unsafe.SizeOf<T>();
        if (hostSize != T.SizeOf && hostSize != T.PaddedSize)
        {
            throw new InvalidOperationException($"{typeof(T).Name} host size {hostSize} does not match shader size {T.SizeOf} or padded size {T.PaddedSize}.");
        }
    }
}
