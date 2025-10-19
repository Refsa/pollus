namespace Pollus.Emscripten.WGPU;
unsafe public struct WGPUCompilationMessage
{
    public WGPUChainedStruct* nextInChain;
    public char* message;
    public WGPUCompilationMessageType type;
    public ulong lineNum;
    public ulong linePos;
    public ulong offset;
    public ulong length;
    public ulong utf16LinePos;
    public ulong utf16Offset;
    public ulong utf16Length;
}
