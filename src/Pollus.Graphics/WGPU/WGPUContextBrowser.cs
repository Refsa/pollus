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
using System.Runtime.InteropServices.JavaScript;

unsafe public class WGPUContextBrowser : IWGPUContext
{
    public enum SetupState
    {
        None,
        SurfaceCreated,
        RequestingAdapter,
        AdapterReady,
        RequestingDevice,
        DeviceReady,
        Ready,
        Failed
    }

    static WGPUContextBrowser _instance;

    IWindow window;
    WGPUInstance instance;

    Silk.NET.WebGPU.Surface* surface;
    Silk.NET.WebGPU.Adapter* adapter;
    Silk.NET.WebGPU.Device* device;
    Silk.NET.WebGPU.Queue* queue;
    Emscripten.WGPUSwapChain_Browser* swapChain;

    TextureFormat preferredFormat;

    SetupState state;
    bool isDisposed;

    List<IGPUResourceWrapper> resources = new();

    public IWindow Window => window;
    public Emscripten.WGPUBrowser wgpu => instance.wgpu;
    public bool IsReady => state is SetupState.Ready;

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
        switch (state)
        {
            case SetupState.None:
                CreateSurface();
                Log.Info("WGPU: Surface created");
                state = SetupState.RequestingAdapter;
                CreateAdapter();
                return;
            case SetupState.AdapterReady:
                state = SetupState.RequestingDevice;
                CreateDevice();
                return;
            case SetupState.DeviceReady:
                CreateQueue();
                Log.Info("WGPU: Queue created");

                preferredFormat = TextureFormat.Bgra8Unorm;
                CreateSwapChain();
                Log.Info("WGPU: Swap chain created");
                state = SetupState.Ready;
                return;
            case SetupState.Failed:
                Log.Error("WGPU: Setup failed");
                return;
            default: return;
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
                SType = Silk.NET.WebGPU.SType.SurfaceDescriptorFromCanvasHtmlSelector,
            },
            Selector = (byte*)selectorPtr.Ptr
        };

        Silk.NET.WebGPU.SurfaceDescriptor descriptor = new()
        {
            NextInChain = (Silk.NET.WebGPU.ChainedStruct*)&surfaceDescriptorFromCanvasHTMLSelector
        };
        surface = wgpu.InstanceCreateSurface(instance.instance, (Silk.NET.WebGPU.SurfaceDescriptor*)Unsafe.AsPointer(in descriptor));
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
        if (swapChain == null) throw new ApplicationException("Failed to create swap chain");
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
            _instance.state = SetupState.AdapterReady;
        }
        else
        {
            Log.Warn("WGPU: Adapter not acquired");
            _instance.state = SetupState.Failed;
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
            MaxInterStageShaderComponents = 4294967295U,
        };
        var requiredLimits = new Emscripten.WGPURequiredLimits_Browser()
        {
            Limits = limits
        };
        var requiredLimitsPtr = Unsafe.AsPointer(ref requiredLimits);

        var requiredFeatures = stackalloc Emscripten.WGPUFeatureName_Browser[]
        {
            Emscripten.WGPUFeatureName_Browser.IndirectFirstInstance,
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
            _instance.state = SetupState.DeviceReady;
        }
        else
        {
            var msg = Marshal.PtrToStringAnsi((IntPtr)message);
            Log.Warn($"WGPU: Device not acquired: {status}, {msg}");
            _instance.state = SetupState.Failed;
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