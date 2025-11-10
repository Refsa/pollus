namespace Pollus.Emscripten.WGPU;
public enum WGPUPresentMode : int
{
    Fifo = 1,
    Immediate = 3,
    Mailbox = 4,
    Force32 = 2147483647,
}
