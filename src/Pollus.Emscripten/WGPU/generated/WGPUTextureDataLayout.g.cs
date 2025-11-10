namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUTextureDataLayout
{
    public WGPUChainedStruct* NextInChain;
    public ulong Offset;
    public uint BytesPerRow;
    public uint RowsPerImage;
}
