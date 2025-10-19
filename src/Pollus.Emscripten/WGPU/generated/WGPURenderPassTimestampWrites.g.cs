namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPURenderPassTimestampWrites
{
    public WGPUQuerySet querySet;
    public uint beginningOfPassWriteIndex;
    public uint endOfPassWriteIndex;
}
