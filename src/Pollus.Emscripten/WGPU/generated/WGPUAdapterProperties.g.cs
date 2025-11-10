namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUAdapterProperties
{
    public WGPUChainedStructOut* NextInChain;
    public uint VendorID;
    public byte* VendorName;
    public byte* Architecture;
    public uint DeviceID;
    public byte* Name;
    public byte* DriverDescription;
    public WGPUAdapterType AdapterType;
    public WGPUBackendType BackendType;
    public bool CompatibilityMode;
}
