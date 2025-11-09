namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPURenderPassTimestampWrites
{
    public WGPUQuerySet* QuerySet;
    public uint BeginningOfPassWriteIndex;
    public uint EndOfPassWriteIndex;
}
