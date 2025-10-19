namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUTextureDataLayout
{
    public WGPUChainedStruct* nextInChain;
    public ulong offset;
    public uint bytesPerRow;
    public uint rowsPerImage;
}
