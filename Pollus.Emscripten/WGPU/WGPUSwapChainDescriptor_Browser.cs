namespace Pollus.Emscripten;

using Silk.NET.WebGPU;

unsafe public struct WGPUSwapChainDescriptor_Browser
{
    public ChainedStruct* NextInChain;
    public byte* Label; // nullable
    public TextureUsage Usage;
    public TextureFormat Format;
    public uint Width;
    public uint Height;
    public PresentMode PresentMode;
}
