namespace Pollus.Graphics.WGPU;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Mathematics;
using Pollus.Utils;

#if NET8_0_BROWSER
using WebGPU = Browser.WGPUBrowser;
#else
using WebGPU = Silk.NET.WebGPU.WebGPU;
#endif

public interface IWGPUContext : IDisposable
{
    public bool IsReady { get; }
    void Setup();
}


unsafe public class WGPUContext : IWGPUContext
{
#if NET8_0_BROWSER
    static WGPUContext _instance;
#endif

    Window window;
    WGPUInstance instance;
    internal WebGPU wgpu;

    internal Silk.NET.WebGPU.Surface* surface;
    internal Silk.NET.WebGPU.Adapter* adapter;
    internal Silk.NET.WebGPU.Device* device;
    internal Silk.NET.WebGPU.Queue* queue;
    internal Browser.WGPUSwapChain_Browser* swapChain;

    Silk.NET.WebGPU.Limits deviceLimits;

#if NET8_0_BROWSER
    Silk.NET.WebGPU.TextureFormat preferredFormat;
#else
    Silk.NET.WebGPU.SurfaceConfiguration surfaceConfiguration;
    Silk.NET.WebGPU.SurfaceCapabilities surfaceCapabilities;
#endif

    bool isPreparingAdapter;
    bool isPreparingDevice;
    bool isDisposed;

    List<WGPUResourceWrapper> resources = new();

    public Window Window => window;
    public bool IsReady => surface != null && adapter != null && device != null && queue != null;

    public WGPUContext(Window window, WGPUInstance instance)
    {
        this.window = window;
        this.instance = instance;
        wgpu = instance.wgpu;

#if NET8_0_BROWSER
        _instance = this;
#endif
    }

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

    [MemberNotNull(nameof(surface), nameof(adapter), nameof(device), nameof(queue))]
    public void Setup()
    {
#if NET8_0_BROWSER
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
        return;
#else
        CreateSurface();
        Console.WriteLine("WGPU: Surface created");
        CreateAdapter();
        Console.WriteLine("WGPU: Adapter created");
        CreateDevice();
        Console.WriteLine("WGPU: Device created");
        CreateQueue();
        Console.WriteLine("WGPU: Queue created");

        ConfigureSurface();
        Console.WriteLine("WGPU: Surface configured");
        return;
#endif
    }

    [MemberNotNull(nameof(surface))]
    void CreateSurface()
    {
#if NET8_0_BROWSER
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
#else
        surface = Silk.NET.WebGPU.WebGPUSurface.CreateWebGPUSurface(window, wgpu, instance.instance);
#endif
    }

    void ConfigureSurface()
    {
#if !NET8_0_BROWSER
        wgpu.SurfaceGetCapabilities(surface, adapter, ref surfaceCapabilities);

        {
            Console.WriteLine("WGPU: Surface capabilities");
            Console.WriteLine("\tFormats");
            for (uint i = 0; i < surfaceCapabilities.FormatCount; i++)
            {
                Console.WriteLine("\t\tFormat: " + surfaceCapabilities.Formats[i]);
            }
            Console.WriteLine("\tAlpha Modes");
            for (uint i = 0; i < surfaceCapabilities.AlphaModeCount; i++)
            {
                Console.WriteLine("\t\tAlpha Mode: " + surfaceCapabilities.AlphaModes[i]);
            }
            Console.WriteLine("\tPresent Modes");
            for (uint i = 0; i < surfaceCapabilities.PresentModeCount; i++)
            {
                Console.WriteLine("\t\tPresent Mode: " + surfaceCapabilities.PresentModes[i]);
            }
        }

        surfaceConfiguration = new(
            device: device,
            format: surfaceCapabilities.Formats[0],
            alphaMode: surfaceCapabilities.AlphaModes[0],
            usage: Silk.NET.WebGPU.TextureUsage.RenderAttachment,
            presentMode: Silk.NET.WebGPU.PresentMode.Fifo,
            width: (uint)Window.Size.X,
            height: (uint)Window.Size.Y
        );

        wgpu.SurfaceConfigure(surface, surfaceConfiguration);
#endif
    }

    void CreateSwapChain()
    {
#if NET8_0_BROWSER
        var descriptor = new Browser.WGPUSwapChainDescriptor_Browser()
        {
            Format = preferredFormat,
            PresentMode = Silk.NET.WebGPU.PresentMode.Fifo,
            Usage = Silk.NET.WebGPU.TextureUsage.RenderAttachment,
        };
        swapChain = wgpu.DeviceCreateSwapChain(device, surface, descriptor);
#endif
    }

    struct CreateAdapterData
    {
        public Silk.NET.WebGPU.Adapter* Adapter;
    }

    [MemberNotNull(nameof(adapter))]
    void CreateAdapter()
    {
        var requestAdapterOptions = new Silk.NET.WebGPU.RequestAdapterOptions
        {
            CompatibleSurface = surface
        };


#if NET8_0_BROWSER
        wgpu.InstanceRequestAdapter(instance.instance, requestAdapterOptions, &HandleRequestAdapterCallback, (void*)nint.Zero);
#else
        using var userData = TemporaryPin.Pin(new CreateAdapterData());
        wgpu.InstanceRequestAdapter(instance.instance, requestAdapterOptions, new Silk.NET.WebGPU.PfnRequestAdapterCallback(HandleRequestAdapterCallback), (void*)userData.Ptr);
        adapter = ((CreateAdapterData*)userData.Ptr)->Adapter;
        isPreparingAdapter = false;
#endif
    }

#if NET8_0_BROWSER
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#endif
    static void HandleRequestAdapterCallback(Silk.NET.WebGPU.RequestAdapterStatus status, Silk.NET.WebGPU.Adapter* adapter, byte* message, void* userdata)
    {
        if (status == Silk.NET.WebGPU.RequestAdapterStatus.Success)
        {
            Console.WriteLine("WGPU: Adapter acquired");
#if NET8_0_BROWSER
            _instance.adapter = adapter;
            _instance.isPreparingAdapter = false;
#else
            ((CreateAdapterData*)userdata)->Adapter = adapter;
#endif
        }
        else
        {
            Console.WriteLine("WGPU: Adapter not acquired");
        }
    }

    struct CreateDeviceData
    {
        public Silk.NET.WebGPU.Device* Device;
    }

    [MemberNotNull(nameof(device))]
    void CreateDevice()
    {
#if NET8_0_BROWSER
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
#else
        var supportedLimits = new Silk.NET.WebGPU.SupportedLimits();
        wgpu.AdapterGetLimits(adapter, ref supportedLimits);
        var requiredLimits = new Silk.NET.WebGPU.RequiredLimits()
        {
            Limits = supportedLimits.Limits with
            {
                MaxDynamicUniformBuffersPerPipelineLayout = 1,
            }
        };
        using var requiredLimitsPtr = TemporaryPin.Pin(requiredLimits);
        var deviceDescriptor = new Silk.NET.WebGPU.DeviceDescriptor(
            requiredLimits: &requiredLimits
        );

        using var userData = TemporaryPin.Pin(new CreateDeviceData());
        wgpu.AdapterRequestDevice(adapter, deviceDescriptor, new Silk.NET.WebGPU.PfnRequestDeviceCallback(HandleRequestDeviceCallback), (void*)userData.Ptr);
        device = ((CreateDeviceData*)userData.Ptr)->Device;
        isPreparingDevice = false;
        GetDeviceLimits();
#endif

    }

    void GetDeviceLimits()
    {
        var acquiredLimits = new Silk.NET.WebGPU.SupportedLimits();
        wgpu.DeviceGetLimits(device, ref acquiredLimits);
        deviceLimits = acquiredLimits.Limits;
        Console.WriteLine("WGPU: Device limits");
    }

#if NET8_0_BROWSER
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#endif
    static void HandleRequestDeviceCallback(Silk.NET.WebGPU.RequestDeviceStatus status, Silk.NET.WebGPU.Device* device, byte* message, void* userdata)
    {
        if (status == Silk.NET.WebGPU.RequestDeviceStatus.Success)
        {
            Console.WriteLine("WGPU: Device acquired");
#if NET8_0_BROWSER
            _instance.device = device;
            _instance.isPreparingDevice = false;
#else
            ((CreateDeviceData*)userdata)->Device = device;
#endif
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

    public WGPUCommandEncoder CreateCommandEncoder(string label)
    {
        return new(this, label);
    }

    public WGPUSurfaceTexture CreateSurfaceTexture()
    {
        return new(this);
    }

    public WGPURenderPipeline CreateRenderPipeline(WGPURenderPipelineDescriptor descriptor)
    {
        return new(this, descriptor);
    }

    public WGPUPipelineLayout CreatePipelineLayout(WGPUPipelineLayoutDescriptor descriptor)
    {
        return new(this, descriptor);
    }

    public WGPUShaderModule CreateShaderModule(WGPUShaderModuleDescriptor descriptor)
    {
        return new(this, descriptor);
    }

    public Silk.NET.WebGPU.TextureFormat GetSurfaceFormat()
    {
#if NET8_0_BROWSER
        return preferredFormat;
#else
        return surfaceConfiguration.Format;
#endif
    }

    public void Present()
    {
#if !NET8_0_BROWSER
        wgpu.SurfacePresent(surface);
#endif
    }

    public void ResizeSurface(Vector2<int> size)
    {
#if !NET8_0_BROWSER
        surfaceConfiguration.Width = (uint)size.X;
        surfaceConfiguration.Height = (uint)size.Y;
        wgpu.SurfaceConfigure(surface, surfaceConfiguration);
#endif
    }
}