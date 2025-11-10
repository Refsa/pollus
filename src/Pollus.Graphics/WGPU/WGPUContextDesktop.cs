#pragma warning disable CS8774

namespace Pollus.Graphics.WGPU;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Debugging;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.Windowing;
using Pollus.Mathematics;
using Pollus.Graphics.Platform;
using Pollus.Graphics.Platform.SilkNetWgpu;

unsafe public class WGPUContextDesktop : IWGPUContext
{
    bool isDisposed;

    IWindow window;
    WGPUInstance instance;
    IWgpuBackend backend;
    List<IGPUResourceWrapper> resources = new();

    internal Silk.NET.WebGPU.Surface* surface;
    internal Silk.NET.WebGPU.Adapter* adapter;
    internal Silk.NET.WebGPU.Device* device;
    internal Silk.NET.WebGPU.Queue* queue;

    Silk.NET.WebGPU.Limits deviceLimits;

    Silk.NET.WebGPU.SurfaceConfiguration surfaceConfiguration;
    Silk.NET.WebGPU.SurfaceCapabilities surfaceCapabilities;

    public IWindow Window => window;
    public bool IsReady => surface != null && adapter != null && device != null && queue != null;

    public Silk.NET.WebGPU.WebGPU wgpu => (backend as SilkWgpuBackend)!.wgpu;
    public IWgpuBackend Backend => backend;
    public NativeHandle<DeviceTag> DeviceHandle => new((nint)device);
    public NativeHandle<QueueTag> QueueHandle => new((nint)queue);

    public WGPUContextDesktop(IWindow window, WGPUInstance instance)
    {
        this.window = window;
        this.instance = instance;
        backend = WgpuBackendProvider.Get();
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
        if (IsReady) return;

        CreateSurface();
        Log.Info("WGPU: Surface created");
        CreateAdapter();
        Log.Info("WGPU: Adapter created");
        CreateDevice();
        Log.Info("WGPU: Device created");
        CreateQueue();
        Log.Info("WGPU: Queue created");

        ConfigureSurface();
        Log.Info("WGPU: Surface configured");
    }

    [MemberNotNull(nameof(surface))]
    void CreateSurface()
    {
        surface = Silk.NET.WebGPU.WebGPUSurface.CreateWebGPUSurface(window, wgpu, instance.Instance.As<Silk.NET.WebGPU.Instance>());
    }

    void ConfigureSurface()
    {
        wgpu.SurfaceGetCapabilities(surface, adapter, ref surfaceCapabilities);

        {
            Log.Info("WGPU: Surface capabilities");
            Log.Info("\tFormats");
            for (uint i = 0; i < surfaceCapabilities.FormatCount; i++)
            {
                Log.Info("\t\tFormat: " + surfaceCapabilities.Formats[i]);
            }
            Log.Info("\tAlpha Modes");
            for (uint i = 0; i < surfaceCapabilities.AlphaModeCount; i++)
            {
                Log.Info("\t\tAlpha Mode: " + surfaceCapabilities.AlphaModes[i]);
            }
            Log.Info("\tPresent Modes");
            for (uint i = 0; i < surfaceCapabilities.PresentModeCount; i++)
            {
                Log.Info("\t\tPresent Mode: " + surfaceCapabilities.PresentModes[i]);
            }
        }

        surfaceConfiguration = new(
            device: device,
            format: surfaceCapabilities.Formats[0],
            alphaMode: surfaceCapabilities.AlphaModes[0],
            usage: Silk.NET.WebGPU.TextureUsage.RenderAttachment,
            presentMode: Silk.NET.WebGPU.PresentMode.Immediate,
            width: (uint)Window.Size.X,
            height: (uint)Window.Size.Y
        );

        wgpu.SurfaceConfigure(surface, ref surfaceConfiguration);
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

        var userData = new CreateAdapterData();
        wgpu.InstanceRequestAdapter(instance.Instance.As<Silk.NET.WebGPU.Instance>(), ref requestAdapterOptions, new Silk.NET.WebGPU.PfnRequestAdapterCallback(HandleRequestAdapterCallback), Unsafe.AsPointer(ref userData));
        adapter = userData.Adapter;
    }

    static void HandleRequestAdapterCallback(Silk.NET.WebGPU.RequestAdapterStatus status, Silk.NET.WebGPU.Adapter* adapter, byte* message, void* userdata)
    {
        if (status == Silk.NET.WebGPU.RequestAdapterStatus.Success)
        {
            Log.Info("WGPU: Adapter acquired");
            ((CreateAdapterData*)userdata)->Adapter = adapter;
        }
        else
        {
            Log.Info($"WGPU: Adapter not acquired: {status} | {Marshal.PtrToStringAnsi((IntPtr)message)}");
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
                MinStorageBufferOffsetAlignment = 256,
                MinUniformBufferOffsetAlignment = 256,
                MaxBindGroups = 3,
                MaxDynamicUniformBuffersPerPipelineLayout = 1,
            },
        };

        var requiredFeatures = stackalloc Silk.NET.WebGPU.FeatureName[]
        {
            Silk.NET.WebGPU.FeatureName.IndirectFirstInstance,
        };

        var deviceDescriptor = new Silk.NET.WebGPU.DeviceDescriptor(
            requiredLimits: &requiredLimits,
            requiredFeatureCount: (nuint)1,
            requiredFeatures: requiredFeatures
        );

        var userData = new CreateDeviceData();
        wgpu.AdapterRequestDevice(adapter, ref deviceDescriptor, new Silk.NET.WebGPU.PfnRequestDeviceCallback(HandleRequestDeviceCallback), Unsafe.AsPointer(ref userData));
        device = userData.Device;

        var acquiredLimits = new Silk.NET.WebGPU.SupportedLimits();
        wgpu.DeviceGetLimits(device, ref acquiredLimits);
        deviceLimits = acquiredLimits.Limits;
        Log.Info("WGPU: Device limits");
    }

    static void HandleRequestDeviceCallback(Silk.NET.WebGPU.RequestDeviceStatus status, Silk.NET.WebGPU.Device* device, byte* message, void* userdata)
    {
        if (status == Silk.NET.WebGPU.RequestDeviceStatus.Success)
        {
            Log.Info("WGPU: Device acquired");
            ((CreateDeviceData*)userdata)->Device = device;
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
        return (TextureFormat)surfaceConfiguration.Format;
    }

    public void Present()
    {
        wgpu.SurfacePresent(surface);
    }

    public void ResizeSurface(Vec2<uint> size)
    {
        surfaceConfiguration.Width = size.X;
        surfaceConfiguration.Height = size.Y;
        wgpu.SurfaceConfigure(surface, ref surfaceConfiguration);
    }
    public bool TryAcquireNextTextureView(out GPUTextureView textureView, TextureViewDescriptor descriptor)
    {
        var st = new Silk.NET.WebGPU.SurfaceTexture();
        wgpu.SurfaceGetCurrentTexture(surface, ref st);
        if (st.Status != Silk.NET.WebGPU.SurfaceGetCurrentTextureStatus.Success)
        {
            textureView = default;
            return false;
        }
        textureView = new GPUTextureView(this, st.Texture, descriptor);
        return true;
    }
}

#pragma warning restore CS8774