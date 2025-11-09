namespace Pollus.Graphics.Platform;

using System;
using Pollus.Graphics.Platform.Emscripten;
using Pollus.Graphics.Platform.SilkNetWgpu;

public static class WgpuBackendProvider
{
    static IWgpuBackend? backend;

    public static void Set(IWgpuBackend value)
    {
        backend = value;
    }

    public static IWgpuBackend Get()
    {
        if (backend != null) return backend;
        if (OperatingSystem.IsBrowser())
        {
            var wgpu = new Pollus.Emscripten.WGPUBrowser();
            backend = new EmscriptenWgpuBackend(wgpu);
        }
        else
        {
            var wgpu = Silk.NET.WebGPU.WebGPU.GetApi();
            backend = new SilkWgpuBackend(wgpu);
        }
        return backend;
    }
}


