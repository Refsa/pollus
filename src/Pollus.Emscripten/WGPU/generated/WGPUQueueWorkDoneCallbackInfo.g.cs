namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUQueueWorkDoneCallbackInfo
{
    public WGPUChainedStruct* NextInChain;
    public WGPUCallbackMode Mode;
    public WGPUQueueWorkDoneCallback Callback;
    public void* Userdata;
}
