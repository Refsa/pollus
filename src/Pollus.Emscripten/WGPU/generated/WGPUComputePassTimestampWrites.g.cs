namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUComputePassTimestampWrites
{
    public WGPUQuerySet* QuerySet;
    public uint BeginningOfPassWriteIndex;
    public uint EndOfPassWriteIndex;
}
