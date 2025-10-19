namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUBufferMapCallbackInfo
{
    public WGPUChainedStruct* NextInChain;
    public WGPUCallbackMode Mode;
    public WGPUBufferMapCallback Callback;
    public void* Userdata;
}
