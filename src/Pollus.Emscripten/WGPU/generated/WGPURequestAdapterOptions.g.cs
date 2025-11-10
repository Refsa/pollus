namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPURequestAdapterOptions
{
    public WGPUChainedStruct* NextInChain;
    public WGPUSurface* CompatibleSurface;
    public WGPUPowerPreference PowerPreference;
    public WGPUBackendType BackendType;
    public bool ForceFallbackAdapter;
    public bool CompatibilityMode;
}
