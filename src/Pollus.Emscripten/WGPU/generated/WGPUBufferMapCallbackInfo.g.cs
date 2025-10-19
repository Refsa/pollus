namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUBufferMapCallbackInfo
{
    public WGPUChainedStruct* nextInChain;
    public WGPUCallbackMode mode;
    public WGPUBufferMapCallback callback;
    public void* userdata;
}
