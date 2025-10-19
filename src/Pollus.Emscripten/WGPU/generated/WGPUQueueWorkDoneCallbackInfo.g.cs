namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUQueueWorkDoneCallbackInfo
{
    public WGPUChainedStruct* nextInChain;
    public WGPUCallbackMode mode;
    public WGPUQueueWorkDoneCallback callback;
    public void* userdata;
}
