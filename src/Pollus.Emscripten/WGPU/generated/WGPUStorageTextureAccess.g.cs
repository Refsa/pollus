namespace Pollus.Emscripten.WGPU;
public enum WGPUStorageTextureAccess : int
{
    Undefined = 0,
    WriteOnly = 1,
    ReadOnly = 2,
    ReadWrite = 3,
    Force32 = 2147483647,
}
