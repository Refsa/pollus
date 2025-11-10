namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPURequestAdapterCallbackInfo
{
    public WGPUChainedStruct* NextInChain;
    public WGPUCallbackMode Mode;
    public WGPURequestAdapterCallback Callback;
    public void* Userdata;
}
