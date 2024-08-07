#if NET8_0_BROWSER
namespace Pollus.Graphics.WGPU;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Mathematics;
using Pollus.Utils;

unsafe public class WGPUContextBrowser : IWGPUContext
{
    static WGPUContextBrowser _instance;

    Window window;
    WGPUInstance instance;

    Silk.NET.WebGPU.Surface* surface;
    Silk.NET.WebGPU.Adapter* adapter;
    Silk.NET.WebGPU.Device* device;
    Silk.NET.WebGPU.Queue* queue;
    Browser.WGPUSwapChain_Browser* swapChain;

    Silk.NET.WebGPU.TextureFormat preferredFormat;

    bool isPreparingAdapter;
    bool isPreparingDevice;
    bool isDisposed;

    List<WGPUResourceWrapper> resources = new();

    public Window Window => window;
    public Browser.WGPUBrowser wgpu => instance.wgpu;
    public bool IsReady => surface != null && adapter != null && device != null && queue != null;

    public Silk.NET.WebGPU.Surface* Surface => surface;
    public Silk.NET.WebGPU.Adapter* Adapter => adapter;
    public Silk.NET.WebGPU.Device* Device => device;
    public Silk.NET.WebGPU.Queue* Queue => queue;
    public Browser.WGPUSwapChain_Browser* SwapChain => swapChain;

    public WGPUContextBrowser(Window window, WGPUInstance instance)
    {
        this.window = window;
        this.instance = instance;
        _instance = this;
    }

    ~WGPUContextBrowser() => Dispose();

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        GC.SuppressFinalize(this);

        for (int i = resources.Count - 1; i >= 0; i--)
        {
            resources[i].Dispose();
        }

        wgpu.QueueRelease(queue);
        wgpu.DeviceRelease(device);
        wgpu.DeviceDestroy(device);
        wgpu.AdapterRelease(adapter);
    }

    public void Setup()
    {
        if (isPreparingAdapter is false && adapter is null)
        {
            isPreparingAdapter = true;
            CreateSurface();
            Console.WriteLine("WGPU: Surface created");
            CreateAdapter();
            return;
        }
        if (adapter is null) return;
        if (isPreparingDevice is false && device is null)
        {
            isPreparingDevice = true;
            CreateDevice();
            return;
        }
        if (device is null) return;

        if (queue is null)
        {
            CreateQueue();
            Console.WriteLine("WGPU: Queue created");
        }

        if (swapChain is null)
        {
            // preferredFormat = wgpu.SurfaceGetPreferredFormat(surface, adapter);
            preferredFormat = Silk.NET.WebGPU.TextureFormat.Bgra8Unorm;
            CreateSwapChain();
            Console.WriteLine("WGPU: Swap chain created");
        }
    }

    [MemberNotNull(nameof(surface))]
    void CreateSurface()
    {
        using var selectorPtr = TemporaryPin.PinString("#canvas");
        Silk.NET.WebGPU.SurfaceDescriptorFromCanvasHTMLSelector surfaceDescriptorFromCanvasHTMLSelector = new()
        {
            Chain = new Silk.NET.WebGPU.ChainedStruct
            {
                Next = null,
                SType = Silk.NET.WebGPU.SType.SurfaceDescriptorFromCanvasHtmlSelector
            },
            Selector = (byte*)selectorPtr.Ptr
        };
        using var surfaceDescriptorFromCanvasHTMLSelectorPtr = TemporaryPin.Pin(surfaceDescriptorFromCanvasHTMLSelector);

        Silk.NET.WebGPU.SurfaceDescriptor descriptor = new()
        {
            NextInChain = (Silk.NET.WebGPU.ChainedStruct*)surfaceDescriptorFromCanvasHTMLSelectorPtr.Ptr
        };
        using var descriptorPtr = TemporaryPin.Pin(descriptor);
        surface = wgpu.InstanceCreateSurface(instance.instance, (Silk.NET.WebGPU.SurfaceDescriptor*)descriptorPtr.Ptr);
    }

    void CreateSwapChain()
    {
        var descriptor = new Browser.WGPUSwapChainDescriptor_Browser()
        {
            Format = preferredFormat,
            PresentMode = Silk.NET.WebGPU.PresentMode.Fifo,
            Usage = Silk.NET.WebGPU.TextureUsage.RenderAttachment,
        };
        swapChain = wgpu.DeviceCreateSwapChain(device, surface, descriptor);
    }

    [MemberNotNull(nameof(adapter))]
    void CreateAdapter()
    {
        var requestAdapterOptions = new Silk.NET.WebGPU.RequestAdapterOptions
        {
            CompatibleSurface = surface
        };
        wgpu.InstanceRequestAdapter(instance.instance, requestAdapterOptions, &HandleRequestAdapterCallback, (void*)nint.Zero);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void HandleRequestAdapterCallback(Silk.NET.WebGPU.RequestAdapterStatus status, Silk.NET.WebGPU.Adapter* adapter, byte* message, void* userdata)
    {
        if (status == Silk.NET.WebGPU.RequestAdapterStatus.Success)
        {
            Console.WriteLine("WGPU: Adapter acquired");
            _instance.adapter = adapter;
            _instance.isPreparingAdapter = false;
        }
        else
        {
            Console.WriteLine("WGPU: Adapter not acquired");
        }
    }

    [MemberNotNull(nameof(device))]
    void CreateDevice()
    {
        var limits = new Browser.WGPULimits_Browser()
        {
            MinStorageBufferOffsetAlignment = 256,
            MinUniformBufferOffsetAlignment = 256,
            MaxDynamicUniformBuffersPerPipelineLayout = 1,
        };
        var requiredLimits = new Browser.WGPURequiredLimits_Browser()
        {
            Limits = limits
        };
        using var requiredLimitsPtr = TemporaryPin.Pin(requiredLimits);
        var deviceDescriptor = new Browser.WGPUDeviceDescriptor_Browser()
        {
            RequiredLimits = (Browser.WGPURequiredLimits_Browser*)requiredLimitsPtr.Ptr
        };

        wgpu.AdapterRequestDevice(adapter, deviceDescriptor, &HandleRequestDeviceCallback, (void*)nint.Zero);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void HandleRequestDeviceCallback(Silk.NET.WebGPU.RequestDeviceStatus status, Silk.NET.WebGPU.Device* device, byte* message, void* userdata)
    {
        if (status == Silk.NET.WebGPU.RequestDeviceStatus.Success)
        {
            Console.WriteLine("WGPU: Device acquired");
            _instance.device = device;
            _instance.isPreparingDevice = false;
        }
        else
        {
            var msg = Marshal.PtrToStringAnsi((IntPtr)message);
            Console.WriteLine($"WGPU: Device not acquired: {status}, {msg}");
        }
    }

    [MemberNotNull(nameof(queue))]
    void CreateQueue()
    {
        queue = wgpu.DeviceGetQueue(device);
    }

    public void RegisterResource(WGPUResourceWrapper resource)
    {
        resources.Add(resource);
    }

    public void ReleaseResource(WGPUResourceWrapper resource)
    {
        resources.Remove(resource);
    }

    public Silk.NET.WebGPU.TextureFormat GetSurfaceFormat()
    {
        return preferredFormat;
    }

    public void Present()
    {

    }

    public void ResizeSurface(Vector2<int> size)
    {

    }
}
#endif