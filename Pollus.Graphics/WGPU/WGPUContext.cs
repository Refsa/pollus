namespace Pollus.Graphics.WGPU;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Mathematics;
using Pollus.Utils;
using Silk.NET.WebGPU;

#if NET8_0_BROWSER
using WebGPU = Pollus.Graphics.WGPU.WGPUBrowser;
#else
using WebGPU = Silk.NET.WebGPU.WebGPU;
#endif

unsafe public class WGPUContext : IDisposable
{
#if NET8_0_BROWSER
    static WGPUContext _instance;
#endif

    Window window;
    WGPUInstance instance;
    internal WebGPU wgpu;

    internal Surface* surface;
    internal Adapter* adapter;
    internal Device* device;
    internal Queue* queue;

    Limits deviceLimits;

    SurfaceConfiguration surfaceConfiguration;
    SurfaceCapabilities surfaceCapabilities;

    bool isPreparingAdapter;
    bool isAdapterReady;
    bool isPreparingDevice;
    bool isDeviceReady;
    bool isDisposed;

    List<WGPUResourceWrapper> resources = new();

    public Window Window => window;
    public bool IsReady => isAdapterReady && isDeviceReady;
    public bool IsAdapterReady => isAdapterReady;
    public bool IsDeviceReady => isDeviceReady;

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
    public bool Setup()
    {
#if NET8_0_BROWSER
        if (isPreparingAdapter is false && isAdapterReady is false)
        {
            CreateSurface();
            Console.WriteLine("WGPU: Surface created");
            CreateAdapter();
            isPreparingAdapter = true;
            return false;
        }
        if (isAdapterReady is false) return false;
        if (isPreparingDevice is false && isDeviceReady is false)
        {
            CreateDevice();
            isPreparingDevice = true;
            return false;
        }
        if (isDeviceReady is false) return false;

        isPreparingAdapter = false;
        isPreparingDevice = false; 

        CreateQueue();
        Console.WriteLine("WGPU: Queue created");
        ConfigureSurface();
        Console.WriteLine("WGPU: Surface configured");
        return true;
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
        return true;
#endif
    }

    [MemberNotNull(nameof(surface))]
    void CreateSurface()
    {
        using var selectorPtr = TemporaryPin.PinString("#canvas");
        SurfaceDescriptorFromCanvasHTMLSelector surfaceDescriptorFromCanvasHTMLSelector = new()
        {
            Chain = new ChainedStruct
            {
                Next = null,
                SType = SType.SurfaceDescriptorFromCanvasHtmlSelector
            },
            Selector = (byte*)selectorPtr.Ptr
        };
        using var surfaceDescriptorFromCanvasHTMLSelectorPtr = TemporaryPin.Pin(surfaceDescriptorFromCanvasHTMLSelector);

        SurfaceDescriptor descriptor = new()
        {
            NextInChain = (ChainedStruct*)surfaceDescriptorFromCanvasHTMLSelectorPtr.Ptr
        };
        using var descriptorPtr = TemporaryPin.Pin(descriptor);
        surface = wgpu.InstanceCreateSurface(instance.instance, (SurfaceDescriptor*)descriptorPtr.Ptr);
#if NET8_0_BROWSER
#else
        surface = WebGPUSurface.CreateWebGPUSurface(window, wgpu, instance.instance);
#endif
    }

    void ConfigureSurface()
    {
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
            usage: TextureUsage.RenderAttachment,
            presentMode: PresentMode.Fifo,
            width: (uint)Window.Size.X,
            height: (uint)Window.Size.Y
        );

        wgpu.SurfaceConfigure(surface, surfaceConfiguration);
    }

    struct CreateAdapterData
    {
        public Adapter* Adapter;
    }

    [MemberNotNull(nameof(adapter))]
    void CreateAdapter()
    {
        var requestAdapterOptions = new RequestAdapterOptions
        {
            CompatibleSurface = surface
        };


#if NET8_0_BROWSER
        wgpu.InstanceRequestAdapter(instance.instance, requestAdapterOptions, &HandleRequestAdapterCallback, (void*)nint.Zero);
#else
        using var userData = TemporaryPin.Pin(new CreateAdapterData());
        wgpu.InstanceRequestAdapter(instance.instance, requestAdapterOptions, new PfnRequestAdapterCallback(HandleRequestAdapterCallback), (void*)userData.Ptr);
        adapter = ((CreateAdapterData*)userData.Ptr)->Adapter;
        isAdapterReady = true;
#endif
    }

#if NET8_0_BROWSER
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#endif
    static void HandleRequestAdapterCallback(RequestAdapterStatus status, Adapter* adapter, byte* message, void* userdata)
    {
        if (status == RequestAdapterStatus.Success)
        {
            Console.WriteLine("WGPU: Adapter acquired");
#if NET8_0_BROWSER
            _instance.adapter = adapter;
            _instance.isAdapterReady = true;
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
        public Device* Device;
    }

    [MemberNotNull(nameof(device))]
    void CreateDevice()
    {
#if NET8_0_BROWSER
        var limits = new WGPULimits()
        {
            MinStorageBufferOffsetAlignment = 256,
            MinUniformBufferOffsetAlignment = 256,
            MaxDynamicUniformBuffersPerPipelineLayout = 1,
        };
        var requiredLimits = new WGPURequiredLimits()
        {
            Limits = limits
        };
        using var requiredLimitsPtr = TemporaryPin.Pin(requiredLimits);
        var deviceDescriptor = new WGPUDeviceDescriptor()
        {
            RequiredLimits = (WGPURequiredLimits*)requiredLimitsPtr.Ptr
        };

        wgpu.AdapterRequestDevice(adapter, deviceDescriptor, &HandleRequestDeviceCallback, (void*)nint.Zero);
#else
        var supportedLimits = new SupportedLimits();
        wgpu.AdapterGetLimits(adapter, ref supportedLimits);
        var requiredLimits = new RequiredLimits()
        {
            Limits = supportedLimits.Limits with
            {
                MaxDynamicUniformBuffersPerPipelineLayout = 1,
            }
        };
        using var requiredLimitsPtr = TemporaryPin.Pin(requiredLimits);
        var deviceDescriptor = new DeviceDescriptor(
            requiredLimits: &requiredLimits
        );

        using var userData = TemporaryPin.Pin(new CreateDeviceData());
        wgpu.AdapterRequestDevice(adapter, deviceDescriptor, new PfnRequestDeviceCallback(HandleRequestDeviceCallback), (void*)userData.Ptr);
        device = ((CreateDeviceData*)userData.Ptr)->Device;
        isDeviceReady = true;
        GetDeviceLimits();
#endif

    }

    void GetDeviceLimits()
    {
        var acquiredLimits = new SupportedLimits();
        wgpu.DeviceGetLimits(device, ref acquiredLimits);
        deviceLimits = acquiredLimits.Limits;
        Console.WriteLine("WGPU: Device limits");
    }

#if NET8_0_BROWSER
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#endif
    static void HandleRequestDeviceCallback(RequestDeviceStatus status, Device* device, byte* message, void* userdata)
    {
        if (status == RequestDeviceStatus.Success)
        {
            Console.WriteLine("WGPU: Device acquired");
#if NET8_0_BROWSER
            _instance.device = device;
            _instance.isDeviceReady = true;
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
        return surfaceConfiguration.Format;
    }

    public void Present()
    {
#if !NET8_0_BROWSER
        wgpu.SurfacePresent(surface);
#endif
    }

    public void ResizeSurface(Vector2<int> size)
    {
        surfaceConfiguration.Width = (uint)size.X;
        surfaceConfiguration.Height = (uint)size.Y;
        wgpu.SurfaceConfigure(surface, surfaceConfiguration);
    }
}