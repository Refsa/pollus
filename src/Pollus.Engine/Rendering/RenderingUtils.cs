namespace Pollus.Engine.Rendering;

using System.Runtime.CompilerServices;

public class RenderingUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint CreateDepthSortKey2D(float zIndex)
    {
        if (zIndex == 0f) return 0;

        var zKey = BitConverter.SingleToUInt32Bits(zIndex);
        zKey ^= (uint)(-(int)(zKey >> 31)) | 0x80000000;
        return zKey;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong CreateSortKey2D(float zIndex, int batchKey)
    {
        var depthKey = CreateDepthSortKey2D(zIndex);
        return ((ulong)depthKey << 32) | (uint)batchKey;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PackSortKeys(int key1, int key2)
    {
        return (key1 << 16) | (key2 & 0xFFFF);
    }
}