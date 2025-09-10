namespace Pollus.Emscripten;

public enum WGPUFeatureName_Browser : uint
{
    Undefined = 0x00000000,
    DepthClipControl = 0x00000001,
    Depth32FloatStencil8 = 0x00000002,
    TimestampQuery = 0x00000003,
    PipelineStatisticsQuery = 0x00000004,
    TextureCompressionBC = 0x00000005,
    TextureCompressionETC2 = 0x00000006,
    TextureCompressionASTC = 0x00000007,
    IndirectFirstInstance = 0x00000008,
    Force32 = 0x7FFFFFFF
}
