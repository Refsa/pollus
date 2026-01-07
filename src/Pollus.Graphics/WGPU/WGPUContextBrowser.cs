namespace Pollus.Graphics.WGPU;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Mathematics;
using Pollus.Utils;
using Pollus.Graphics.Windowing;
using Pollus.Graphics.Rendering;
using Pollus.Debugging;
using Pollus.Graphics.Platform;
using Pollus.Graphics.Platform.Emscripten;

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

    static WGPUContextBrowser? _instance;

    IWindow window;
    WGPUInstance instance;

    Emscripten.WGPU.WGPUSurface* surface;
    Emscripten.WGPU.WGPUAdapter* adapter;
    Emscripten.WGPU.WGPUDevice* device;
    Emscripten.WGPU.WGPUQueue* queue;
    Emscripten.WGPU.WGPUSwapChain* swapChain;

    TextureFormat preferredFormat;

    SetupState state;
    bool isDisposed;

    List<IGPUResourceWrapper> resources = new();

    public IWindow Window => window;
    public bool IsReady => state is SetupState.Ready;

    public IWgpuBackend Backend => backend;
    public NativeHandle<DeviceTag> DeviceHandle => new((nint)device);
    public NativeHandle<QueueTag> QueueHandle => new((nint)queue);

    IWgpuBackend backend;
    Emscripten.WGPUBrowser wgpu => (backend as EmscriptenWgpuBackend)!.wgpu;

    public WGPUContextBrowser(IWindow window, WGPUInstance instance)
    {
        this.window = window;
        this.instance = instance;
        _instance = this;
        backend = WgpuBackendProvider.Get();
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
                Log.Debug("WGPU: Surface created");
                state = SetupState.RequestingAdapter;
                CreateAdapter();
                return;
            case SetupState.AdapterReady:
                state = SetupState.RequestingDevice;
                CreateDevice();
                return;
            case SetupState.DeviceReady:
                CreateQueue();
                Log.Debug("WGPU: Queue created");

                preferredFormat = TextureFormat.Bgra8Unorm;
                CreateSwapChain();
                Log.Debug("WGPU: Swap chain created");
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
        Emscripten.WGPU.WGPUSurfaceDescriptorFromCanvasHTMLSelector surfaceDescriptorFromCanvasHTMLSelector = new()
        {
            Chain = new Emscripten.WGPU.WGPUChainedStruct
            {
                Next = null,
                SType = Emscripten.WGPU.WGPUSType.SurfaceDescriptorFromCanvasHTMLSelector,
            },
            Selector = (byte*)selectorPtr.Ptr
        };

        Emscripten.WGPU.WGPUSurfaceDescriptor descriptor = new()
        {
            NextInChain = (Emscripten.WGPU.WGPUChainedStruct*)&surfaceDescriptorFromCanvasHTMLSelector
        };
        surface = wgpu.InstanceCreateSurface(instance.Instance.As<Emscripten.WGPU.WGPUInstance>(), (Emscripten.WGPU.WGPUSurfaceDescriptor*)Unsafe.AsPointer(ref descriptor));
    }

    void CreateSwapChain()
    {
        var descriptor = new Emscripten.WGPU.WGPUSwapChainDescriptor()
        {
            Format = (Emscripten.WGPU.WGPUTextureFormat)preferredFormat,
            PresentMode = Emscripten.WGPU.WGPUPresentMode.Fifo,
            Usage = (Emscripten.WGPU.WGPUTextureUsage)TextureUsage.RenderAttachment,
            Height = window.Size.Y,
            Width = window.Size.X
        };
        swapChain = wgpu.DeviceCreateSwapChain(device, surface, descriptor);
        if (swapChain == null) throw new ApplicationException("Failed to create swap chain");
    }

    void CreateAdapter()
    {
        var requestAdapterOptions = new Emscripten.WGPU.WGPURequestAdapterOptions
        {
            CompatibleSurface = surface
        };
        wgpu.InstanceRequestAdapter(instance.Instance.As<Emscripten.WGPU.WGPUInstance>(), requestAdapterOptions, &HandleRequestAdapterCallback, (void*)nint.Zero);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void HandleRequestAdapterCallback(Emscripten.WGPU.WGPURequestAdapterStatus status, Emscripten.WGPU.WGPUAdapter* adapter, byte* message, void* userdata)
    {
        if (status == Emscripten.WGPU.WGPURequestAdapterStatus.Success)
        {
            var instA = _instance;
            if (instA == null) return;
            Log.Debug("WGPU: Adapter acquired");
            instA.adapter = adapter;
            instA.state = SetupState.AdapterReady;
        }
        else
        {
            Log.Warn("WGPU: Adapter not acquired");
            if (_instance == null) return;
            _instance.state = SetupState.Failed;
        }
    }

    void CreateDevice()
    {
        var limits = new Emscripten.WGPU.WGPULimits()
        {
            MinStorageBufferOffsetAlignment = 256,
            MinUniformBufferOffsetAlignment = 256,
            MaxBindGroups = 3,
            MaxDynamicUniformBuffersPerPipelineLayout = 1,
            MaxInterStageShaderComponents = 4294967295U,
        };
        var requiredLimits = new Emscripten.WGPU.WGPURequiredLimits()
        {
            Limits = limits
        };
        var requiredLimitsPtr = Unsafe.AsPointer(ref requiredLimits);

        var requiredFeatures = stackalloc Emscripten.WGPU.WGPUFeatureName[]
        {
            Emscripten.WGPU.WGPUFeatureName.IndirectFirstInstance,
        };

        var deviceDescriptor = new Emscripten.WGPU.WGPUDeviceDescriptor()
        {
            RequiredLimits = (Emscripten.WGPU.WGPURequiredLimits*)requiredLimitsPtr,
            RequiredFeatureCount = 1,
            RequiredFeatures = requiredFeatures,
        };

        wgpu.AdapterRequestDevice(adapter, deviceDescriptor, &HandleRequestDeviceCallback, (void*)nint.Zero);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    static void HandleRequestDeviceCallback(Emscripten.WGPU.WGPURequestDeviceStatus status, Emscripten.WGPU.WGPUDevice* device, byte* message, void* userdata)
    {
        if (status == Emscripten.WGPU.WGPURequestDeviceStatus.Success)
        {
            var instD = _instance;
            if (instD == null) return;
            Log.Debug("WGPU: Device acquired");
            instD.device = device;
            instD.state = SetupState.DeviceReady;
        }
        else
        {
            var msg = Marshal.PtrToStringAnsi((IntPtr)message);
            Log.Warn($"WGPU: Device not acquired: {status}, {msg}");
            if (_instance == null) return;
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

    public bool TryAcquireNextTextureView(in TextureViewDescriptor descriptor, out GPUTextureView textureView, out NativeHandle<TextureTag> textureHandle)
    {
        textureHandle = NativeHandle<TextureTag>.Null;
        var native = wgpu.SwapChainGetCurrentTextureView(swapChain);
        if (native == null)
        {
            textureView = default;
            return false;
        }

        textureView = new GPUTextureView(this, (nint)native, descriptor);
        return true;
    }
}