#if BROWSER
namespace Pollus.Graphics.WGPU;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Mathematics;
using Pollus.Utils;
using Pollus.Graphics.Windowing;
using Pollus.Graphics.Rendering;
using Pollus.Debugging;

unsafe public class WGPUContextBrowser : IWGPUContext
{
    static WGPUContextBrowser _instance;

    IWindow window;
    WGPUInstance instance;

    Silk.NET.WebGPU.Surface* surface;
    Silk.NET.WebGPU.Adapter* adapter;
    Silk.NET.WebGPU.Device* device;
    Silk.NET.WebGPU.Queue* queue;
    Emscripten.WGPUSwapChain_Browser* swapChain;

    TextureFormat preferredFormat;

    bool isPreparingAdapter;
    bool isPreparingDevice;
    bool isDisposed;

    List<IGPUResourceWrapper> resources = new();

    public IWindow Window => window;
    public Emscripten.WGPUBrowser wgpu => instance.wgpu;
    public bool IsReady => surface != null && adapter != null && device != null && queue != null;

    public Silk.NET.WebGPU.Surface* Surface => surface;
    public Silk.NET.WebGPU.Adapter* Adapter => adapter;
    public Silk.NET.WebGPU.Device* Device => device;
    public Silk.NET.WebGPU.Queue* Queue => queue;
    public Emscripten.WGPUSwapChain_Browser* SwapChain => swapChain;

    public WGPUContextBrowser(IWindow window, WGPUInstance instance)
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
            Log.Info("WGPU: Surface created");
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
            Log.Info("WGPU: Queue created");
        }

        if (swapChain is null)
        {
            // preferredFormat = wgpu.SurfaceGetPreferredFormat(surface, adapter);
            preferredFormat = TextureFormat.Bgra8Unorm;
            CreateSwapChain();
            Log.Info("WGPU: Swap chain created");
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
        var surfaceDescriptorFromCanvasHTMLSelectorPtr = Unsafe.AsPointer(ref surfaceDescriptorFromCanvasHTMLSelector);

        Silk.NET.WebGPU.SurfaceDescriptor descriptor = new()
        {
            NextInChain = (Silk.NET.WebGPU.ChainedStruct*)surfaceDescriptorFromCanvasHTMLSelectorPtr
        };
        surface = wgpu.InstanceCreateSurface(instance.instance, (Silk.NET.WebGPU.SurfaceDescriptor*)Unsafe.AsPointer(ref descriptor));
    }

    void CreateSwapChain()
    {
        var descriptor = new Emscripten.WGPUSwapChainDescriptor_Browser()
        {
            Format = (Silk.NET.WebGPU.TextureFormat)preferredFormat,
            PresentMode = Silk.NET.WebGPU.PresentMode.Fifo,
            Usage = (Silk.NET.WebGPU.TextureUsage)TextureUsage.RenderAttachment,
            Height = (uint)window.Size.Y,
            Width = (uint)window.Size.X
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
            Log.Info("WGPU: Adapter acquired");
            _instance.adapter = adapter;
            _instance.isPreparingAdapter = false;
        }
        else
        {
            Log.Info("WGPU: Adapter not acquired");
        }
    }

    [MemberNotNull(nameof(device))]
    void CreateDevice()
    {
        var limits = new Emscripten.WGPULimits_Browser()
        {
            MinStorageBufferOffsetAlignment = 256,
            MinUniformBufferOffsetAlignment = 256,
            MaxBindGroups = 3,
            MaxDynamicUniformBuffersPerPipelineLayout = 1,
            MaxInterStageShaderComponents = Silk.NET.WebGPU.WebGPU.LimitU32Undefined,
        };
        var requiredLimits = new Emscripten.WGPURequiredLimits_Browser()
        {
            Limits = limits
        };
        var requiredLimitsPtr = Unsafe.AsPointer(ref requiredLimits);
        
        var requiredFeatures = stackalloc Silk.NET.WebGPU.FeatureName[]
        {
            Silk.NET.WebGPU.FeatureName.IndirectFirstInstance,
        };

        var deviceDescriptor = new Emscripten.WGPUDeviceDescriptor_Browser()
        {
            RequiredLimits = (Emscripten.WGPURequiredLimits_Browser*)requiredLimitsPtr,
            RequiredFeatureCount = (nuint)1,
            RequiredFeatures = requiredFeatures,
        };

        wgpu.AdapterRequestDevice(adapter, deviceDescriptor, &HandleRequestDeviceCallback, (void*)nint.Zero);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void HandleRequestDeviceCallback(Silk.NET.WebGPU.RequestDeviceStatus status, Silk.NET.WebGPU.Device* device, byte* message, void* userdata)
    {
        if (status == Silk.NET.WebGPU.RequestDeviceStatus.Success)
        {
            Log.Info("WGPU: Device acquired");
            _instance.device = device;
            _instance.isPreparingDevice = false;
        }
        else
        {
            var msg = Marshal.PtrToStringAnsi((IntPtr)message);
            Log.Info($"WGPU: Device not acquired: {status}, {msg}");
        }
    }

    [MemberNotNull(nameof(queue))]
    void CreateQueue()
    {
        queue = wgpu.DeviceGetQueue(device);
    }

    public void RegisterResource<TResource>(TResource resource)
        where TResource : IGPUResourceWrapper
    {
        resources.Add(resource);
    }

    public void ReleaseResource<TResource>(TResource resource)
        where TResource : IGPUResourceWrapper
    {
        resources.Remove(resource);
    }

    public TextureFormat GetSurfaceFormat()
    {
        return preferredFormat;
    }

    public void Present()
    {

    }

    public void ResizeSurface(Vec2<uint> size)
    {

    }
}
#endif