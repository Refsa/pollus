namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUAdapterProperties
{
    public WGPUChainedStructOut* nextInChain;
    public uint vendorID;
    public char* vendorName;
    public char* architecture;
    public uint deviceID;
    public char* name;
    public char* driverDescription;
    public WGPUAdapterType adapterType;
    public WGPUBackendType backendType;
    public bool compatibilityMode;
}
