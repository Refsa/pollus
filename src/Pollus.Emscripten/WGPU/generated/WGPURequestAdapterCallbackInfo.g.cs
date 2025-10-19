namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPURequestAdapterCallbackInfo
{
    public WGPUChainedStruct* nextInChain;
    public WGPUCallbackMode mode;
    public WGPURequestAdapterCallback callback;
    public void* userdata;
}
