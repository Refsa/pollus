namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUCompilationMessage
{
    public WGPUChainedStruct* NextInChain;
    public byte* Message;
    public WGPUCompilationMessageType Type;
    public ulong LineNum;
    public ulong LinePos;
    public ulong Offset;
    public ulong Length;
    public ulong Utf16LinePos;
    public ulong Utf16Offset;
    public ulong Utf16Length;
}
