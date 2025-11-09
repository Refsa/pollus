namespace Pollus.Emscripten;

public partial class WGPUBrowser
{
    const uint UINT32_MAX = 0xffffffffu;
    const ulong UINT64_MAX = 0xffffffffffffffffu;
    const ulong SIZE_MAX = UINT64_MAX;

    public const uint WGPU_ARRAY_LAYER_COUNT_UNDEFINED = UINT32_MAX;
    public const uint WGPU_COPY_STRIDE_UNDEFINED = UINT32_MAX;
    public const uint WGPU_DEPTH_SLICE_UNDEFINED = UINT32_MAX;
    public const uint WGPU_LIMIT_U32_UNDEFINED = UINT32_MAX;
    public const ulong WGPU_LIMIT_U64_UNDEFINED = UINT64_MAX;
    public const uint WGPU_MIP_LEVEL_COUNT_UNDEFINED = UINT32_MAX;
    public const uint WGPU_QUERY_SET_INDEX_UNDEFINED = UINT32_MAX;
    public const ulong WGPU_WHOLE_MAP_SIZE = SIZE_MAX;
    public const ulong WGPU_WHOLE_SIZE = UINT64_MAX;
}