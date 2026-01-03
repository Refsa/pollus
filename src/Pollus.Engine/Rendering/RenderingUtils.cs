namespace Pollus.Engine.Rendering;

public class RenderingUtils
{
    public static ulong CreateSortKey2D(float zIndex, int batchKey)
    {
        var zKey = BitConverter.SingleToUInt32Bits(zIndex);
        zKey ^= (uint)(-(int)(zKey >> 31)) | 0x80000000;
        return ((ulong)zKey << 32) | (uint)batchKey;
    }
}