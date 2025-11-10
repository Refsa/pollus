namespace Pollus.Emscripten.WGPU;
public enum WGPUCallbackMode : int
{
    WaitAnyOnly = 0,
    AllowProcessEvents = 1,
    AllowSpontaneous = 2,
    Force32 = 2147483647,
}
