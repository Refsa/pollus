namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPURequestAdapterOptions
{
    public WGPUChainedStruct* nextInChain;
    public WGPUSurface compatibleSurface;
    public WGPUPowerPreference powerPreference;
    public WGPUBackendType backendType;
    public bool forceFallbackAdapter;
    public bool compatibilityMode;
}
