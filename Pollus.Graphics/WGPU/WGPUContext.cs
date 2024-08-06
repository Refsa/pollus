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

    bool isReady;
    bool isDisposed;

    List<WGPUResourceWrapper> resources = new();

    public Window Window => window;
    public bool IsReady => isReady;

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

        isReady = true;
    }

    [MemberNotNull(nameof(surface))]
    void CreateSurface()
    {
#if NET8_0_BROWSER
        SurfaceDescriptor descriptor = default(SurfaceDescriptor);
        SurfaceDescriptorFromCanvasHTMLSelector surfaceDescriptorFromCanvasHTMLSelector = default(SurfaceDescriptorFromCanvasHTMLSelector);
        surfaceDescriptorFromCanvasHTMLSelector.Chain = new ChainedStruct
        {
            Next = null,
            SType = SType.SurfaceDescriptorFromCanvasHtmlSelector
        };
        surfaceDescriptorFromCanvasHTMLSelector.Selector = (byte*)Silk.NET.Core.Native.SilkMarshal.StringToPtr("canvas");
        SurfaceDescriptorFromCanvasHTMLSelector surfaceDescriptorFromCanvasHTMLSelector2 = surfaceDescriptorFromCanvasHTMLSelector;
        descriptor.NextInChain = (ChainedStruct*)(&surfaceDescriptorFromCanvasHTMLSelector2);
        surface = wgpu.InstanceCreateSurface(instance.instance, in descriptor);
        if (descriptor.NextInChain->SType == SType.SurfaceDescriptorFromCanvasHtmlSelector)
        {
            SurfaceDescriptorFromCanvasHTMLSelector* nextInChain = (SurfaceDescriptorFromCanvasHTMLSelector*)descriptor.NextInChain;
            Silk.NET.Core.Native.SilkMarshal.Free((IntPtr)nextInChain->Selector);
        }
#else
        surface = WebGPUSurface.CreateWebGPUSurface(window, wgpu, instance.instance);
#endif
    }

    void ConfigureSurface()
    {
        wgpu.SurfaceGetCapabilities(surface, adapter, ref surfaceCapabilities);

        LogSurfaceCapabilities();

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

        void LogSurfaceCapabilities()
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
    }

    [MemberNotNull(nameof(adapter))]
    void CreateAdapter()
    {
        var requestAdapterOptions = new RequestAdapterOptions
        {
            CompatibleSurface = surface
        };
#if NET8_0_BROWSER
        wgpu.InstanceRequestAdapter(instance.instance, requestAdapterOptions, &HandleRequestAdapterCallback, adapter);
#else
        wgpu.InstanceRequestAdapter(instance.instance, requestAdapterOptions, new PfnRequestAdapterCallback(HandleRequestAdapterCallback), adapter);
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

            userdata = adapter;
        }
        else
        {
            Console.WriteLine("WGPU: Adapter not acquired");
        }
    }

    [MemberNotNull(nameof(device))]
    void CreateDevice()
    {
        var supportedLimits = new SupportedLimits();
#if NET8_0_BROWSER
        supportedLimits.Limits.MinStorageBufferOffsetAlignment = 256;
        supportedLimits.Limits.MinUniformBufferOffsetAlignment = 256;
#else
        wgpu.AdapterGetLimits(adapter, ref supportedLimits);
#endif

        var requiredLimits = new RequiredLimits()
        {
            Limits = supportedLimits.Limits with
            {
                MaxDynamicUniformBuffersPerPipelineLayout = 1,
            }
        };
        using var requiredLimitsPtr = TemporaryPin.Pin(requiredLimits);
        var deviceDescriptor = new DeviceDescriptor(
            requiredLimits: (RequiredLimits*)requiredLimitsPtr.Ptr
        );

#if NET8_0_BROWSER
        wgpu.AdapterRequestDevice(adapter, deviceDescriptor, &HandleRequestDeviceCallback, (void*)nint.Zero);
#else
        wgpu.AdapterRequestDevice(adapter, deviceDescriptor, new PfnRequestDeviceCallback(HandleRequestDeviceCallback), device);
#endif

        GetDeviceLimits();
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

            userdata = device;
        }
        else
        {
            Console.WriteLine("WGPU: Device not acquired");
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
        wgpu.SurfacePresent(surface);
    }

    public void ResizeSurface(Vector2<int> size)
    {
        surfaceConfiguration.Width = (uint)size.X;
        surfaceConfiguration.Height = (uint)size.Y;
        wgpu.SurfaceConfigure(surface, surfaceConfiguration);
    }
}