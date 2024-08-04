namespace Pollus.Graphics.WGPU;

using System.Diagnostics.CodeAnalysis;
using Pollus.Mathematics;
using Silk.NET.WebGPU;

unsafe public class WGPUContext : IDisposable
{
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

    bool isDisposed;

    List<WGPUResouceWrapper> resources = new();

    public Window Window => window;

    public WGPUContext(Window window, WGPUInstance instance)
    {
        this.window = window;
        this.instance = instance;
        wgpu = instance.wgpu;
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
        CreateAdapter();
        CreateDevice();
        CreateQueue();

        ConfigureSurface();
    }

    [MemberNotNull(nameof(surface))]
    void CreateSurface()
    {
        surface = WebGPUSurface.CreateWebGPUSurface(window, wgpu, instance.instance);
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

        wgpu.InstanceRequestAdapter(instance.instance, requestAdapterOptions, new PfnRequestAdapterCallback(HandleRequestAdapter), adapter);

        void HandleRequestAdapter(RequestAdapterStatus status, Adapter* adapter, byte* message, void* userdata)
        {
            if (status == RequestAdapterStatus.Success)
            {
                this.adapter = adapter;
            }
        }
    }

    [MemberNotNull(nameof(device))]
    void CreateDevice()
    {
        var supportedLimits = new SupportedLimits();
        wgpu.AdapterGetLimits(adapter, ref supportedLimits);

        var requiredLimits = new RequiredLimits()
        {
            Limits = supportedLimits.Limits with
            {
                MaxDynamicUniformBuffersPerPipelineLayout = 1,
            }
        };
        var deviceDescriptor = new DeviceDescriptor(
            requiredLimits: &requiredLimits
        );
        wgpu.AdapterRequestDevice(adapter, deviceDescriptor, new PfnRequestDeviceCallback(HandleRequestDevice), device);

        var acquiredLimits = new SupportedLimits();
        wgpu.DeviceGetLimits(device, ref acquiredLimits);
        deviceLimits = acquiredLimits.Limits;

        void HandleRequestDevice(RequestDeviceStatus status, Device* device, byte* message, void* userdata)
        {
            if (status == RequestDeviceStatus.Success)
            {
                this.device = device;
            }
        }
    }

    [MemberNotNull(nameof(queue))]
    void CreateQueue()
    {
        queue = wgpu.DeviceGetQueue(device);
    }

    public void RegisterResource(WGPUResouceWrapper resource)
    {
        resources.Add(resource);
    }

    public void ReleaseResource(WGPUResouceWrapper resource)
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