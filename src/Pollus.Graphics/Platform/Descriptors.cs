namespace Pollus.Graphics.Platform;

using Pollus.Graphics.Rendering;

public enum PlatformPresentMode
{
    Fifo = 0,
    Immediate = 1,
    Mailbox = 2
}

public enum SurfaceSourceKind
{
    WindowHandle,
    CanvasSelector
}

public readonly struct SurfaceSource
{
    public readonly SurfaceSourceKind Kind;
    public readonly nint Handle;
    public readonly string Selector;
    public SurfaceSource(nint handle)
    {
        Kind = SurfaceSourceKind.WindowHandle;
        Handle = handle;
        Selector = string.Empty;
    }
    public SurfaceSource(string selector)
    {
        Kind = SurfaceSourceKind.CanvasSelector;
        Handle = 0;
        Selector = selector;
    }
}

public readonly struct AdapterOptions
{
    public readonly NativeHandle<SurfaceTag> CompatibleSurface;
    public AdapterOptions(NativeHandle<SurfaceTag> compatibleSurface)
    {
        CompatibleSurface = compatibleSurface;
    }
}

public readonly struct DeviceOptions
{
    public readonly uint MaxBindGroups;
    public readonly uint MinStorageBufferOffsetAlignment;
    public readonly uint MinUniformBufferOffsetAlignment;
    public DeviceOptions(uint maxBindGroups, uint minStorageBufferOffsetAlignment, uint minUniformBufferOffsetAlignment)
    {
        MaxBindGroups = maxBindGroups;
        MinStorageBufferOffsetAlignment = minStorageBufferOffsetAlignment;
        MinUniformBufferOffsetAlignment = minUniformBufferOffsetAlignment;
    }
}

public readonly struct AdapterResult
{
    public readonly bool Success;
    public readonly NativeHandle<AdapterTag> Adapter;
    public readonly string Message;
    public AdapterResult(bool success, NativeHandle<AdapterTag> adapter, string message)
    {
        Success = success;
        Adapter = adapter;
        Message = message;
    }
}

public readonly struct DeviceResult
{
    public readonly bool Success;
    public readonly NativeHandle<DeviceTag> Device;
    public readonly string Message;
    public DeviceResult(bool success, NativeHandle<DeviceTag> device, string message)
    {
        Success = success;
        Device = device;
        Message = message;
    }
}

public readonly struct SwapChainOptions
{
    public readonly TextureFormat Format;
    public readonly PlatformPresentMode PresentMode;
    public readonly TextureUsage Usage;
    public readonly uint Width;
    public readonly uint Height;
    public SwapChainOptions(TextureFormat format, PlatformPresentMode presentMode, TextureUsage usage, uint width, uint height)
    {
        Format = format;
        PresentMode = presentMode;
        Usage = usage;
        Width = width;
        Height = height;
    }
}