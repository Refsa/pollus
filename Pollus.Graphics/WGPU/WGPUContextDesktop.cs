#if !BROWSER
namespace Pollus.Graphics.WGPU;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Pollus.Graphics.WGPU.Browser;
using Pollus.Mathematics;
using Pollus.Utils;
using Silk.NET.WebGPU;

unsafe public class WGPUContextDesktop : IWGPUContext
{
    Window window;
    WGPUInstance instance;

    internal Silk.NET.WebGPU.Surface* surface;
    internal Silk.NET.WebGPU.Adapter* adapter;
    internal Silk.NET.WebGPU.Device* device;
    internal Silk.NET.WebGPU.Queue* queue;

    Silk.NET.WebGPU.Limits deviceLimits;

    Silk.NET.WebGPU.SurfaceConfiguration surfaceConfiguration;
    Silk.NET.WebGPU.SurfaceCapabilities surfaceCapabilities;

    bool isDisposed;

    List<WGPUResourceWrapper> resources = new();

    public Window Window => window;
    public bool IsReady => surface != null && adapter != null && device != null && queue != null;

    public Silk.NET.WebGPU.WebGPU wgpu => instance.wgpu;
    public Surface* Surface => surface;
    public Adapter* Adapter => adapter;
    public Device* Device => device;
    public Queue* Queue => queue;

    public WGPUContextDesktop(Window window, WGPUInstance instance)
    {
        this.window = window;
        this.instance = instance;
    }

    ~WGPUContextDesktop() => Dispose();

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
    }

    [MemberNotNull(nameof(surface))]
    void CreateSurface()
    {
        surface = Silk.NET.WebGPU.WebGPUSurface.CreateWebGPUSurface(window, wgpu, instance.instance);
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
            usage: Silk.NET.WebGPU.TextureUsage.RenderAttachment,
            presentMode: Silk.NET.WebGPU.PresentMode.Fifo,
            width: (uint)Window.Size.X,
            height: (uint)Window.Size.Y
        );

        wgpu.SurfaceConfigure(surface, surfaceConfiguration);
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

        using var userData = TemporaryPin.Pin(new CreateAdapterData());
        wgpu.InstanceRequestAdapter(instance.instance, requestAdapterOptions, new Silk.NET.WebGPU.PfnRequestAdapterCallback(HandleRequestAdapterCallback), (void*)userData.Ptr);
        adapter = ((CreateAdapterData*)userData.Ptr)->Adapter;
    }

    static void HandleRequestAdapterCallback(Silk.NET.WebGPU.RequestAdapterStatus status, Silk.NET.WebGPU.Adapter* adapter, byte* message, void* userdata)
    {
        if (status == Silk.NET.WebGPU.RequestAdapterStatus.Success)
        {
            Console.WriteLine("WGPU: Adapter acquired");
            ((CreateAdapterData*)userdata)->Adapter = adapter;
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
        
        var acquiredLimits = new Silk.NET.WebGPU.SupportedLimits();
        wgpu.DeviceGetLimits(device, ref acquiredLimits);
        deviceLimits = acquiredLimits.Limits;
        Console.WriteLine("WGPU: Device limits");
    }

    static void HandleRequestDeviceCallback(Silk.NET.WebGPU.RequestDeviceStatus status, Silk.NET.WebGPU.Device* device, byte* message, void* userdata)
    {
        if (status == Silk.NET.WebGPU.RequestDeviceStatus.Success)
        {
            Console.WriteLine("WGPU: Device acquired");
            ((CreateDeviceData*)userdata)->Device = device;
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
#endif