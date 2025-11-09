namespace Pollus.Graphics.Platform.SilkNetWgpu;

using System;
using Pollus.Graphics.Rendering;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Pollus.Graphics.Platform;

public unsafe class SilkWgpuBackend : IWgpuBackend
{
    public readonly Silk.NET.WebGPU.WebGPU wgpu;

    public SilkWgpuBackend(Silk.NET.WebGPU.WebGPU wgpu)
    {
        this.wgpu = wgpu;
    }

    public NativeHandle<InstanceTag> CreateInstance()
    {
        var desc = new Silk.NET.WebGPU.InstanceDescriptor();
        var instance = wgpu.CreateInstance(ref desc);
        return new NativeHandle<InstanceTag>((nint)instance);
    }

    public NativeHandle<SurfaceTag> CreateSurface(NativeHandle<InstanceTag> instance, SurfaceSource source)
    {
        return new NativeHandle<SurfaceTag>(source.Handle);
    }

    public void RequestAdapter(NativeHandle<InstanceTag> instance, in AdapterOptions options, Action<AdapterResult> callback)
    {
        var req = new Silk.NET.WebGPU.RequestAdapterOptions
        {
            CompatibleSurface = (Silk.NET.WebGPU.Surface*)options.CompatibleSurface.Ptr
        };
        var handle = GCHandle.Alloc(callback, GCHandleType.Normal);
        wgpu.InstanceRequestAdapter((Silk.NET.WebGPU.Instance*)instance.Ptr, ref req, new Silk.NET.WebGPU.PfnRequestAdapterCallback(OnRequestAdapter), (void*)GCHandle.ToIntPtr(handle));
    }

    public void RequestDevice(NativeHandle<AdapterTag> adapter, in DeviceOptions options, Action<DeviceResult> callback)
    {
        var limits = new Silk.NET.WebGPU.SupportedLimits();
        wgpu.AdapterGetLimits((Silk.NET.WebGPU.Adapter*)adapter.Ptr, ref limits);
        var requiredLimits = new Silk.NET.WebGPU.RequiredLimits
        {
            Limits = limits.Limits with
            {
                MinStorageBufferOffsetAlignment = options.MinStorageBufferOffsetAlignment,
                MinUniformBufferOffsetAlignment = options.MinUniformBufferOffsetAlignment,
                MaxBindGroups = options.MaxBindGroups
            }
        };
        var desc = new Silk.NET.WebGPU.DeviceDescriptor(requiredLimits: &requiredLimits, requiredFeatureCount: 0, requiredFeatures: null);
        var handle = GCHandle.Alloc(callback, GCHandleType.Normal);
        wgpu.AdapterRequestDevice((Silk.NET.WebGPU.Adapter*)adapter.Ptr, ref desc, new Silk.NET.WebGPU.PfnRequestDeviceCallback(OnRequestDevice), (void*)GCHandle.ToIntPtr(handle));
    }

    static void OnRequestAdapter(Silk.NET.WebGPU.RequestAdapterStatus status, Silk.NET.WebGPU.Adapter* adapter, byte* message, void* userdata)
    {
        var gch = GCHandle.FromIntPtr((nint)userdata);
        try
        {
            var cb = (Action<AdapterResult>)gch.Target!;
            var ok = status == Silk.NET.WebGPU.RequestAdapterStatus.Success;
            var msg = message != null ? Marshal.PtrToStringAnsi((nint)message) ?? string.Empty : string.Empty;
            cb(new AdapterResult(ok, new NativeHandle<AdapterTag>((nint)adapter), msg));
        }
        finally
        {
            gch.Free();
        }
    }

    static void OnRequestDevice(Silk.NET.WebGPU.RequestDeviceStatus status, Silk.NET.WebGPU.Device* device, byte* message, void* userdata)
    {
        var gch = GCHandle.FromIntPtr((nint)userdata);
        try
        {
            var cb = (Action<DeviceResult>)gch.Target!;
            var ok = status == Silk.NET.WebGPU.RequestDeviceStatus.Success;
            var msg = message != null ? Marshal.PtrToStringAnsi((nint)message) ?? string.Empty : string.Empty;
            cb(new DeviceResult(ok, new NativeHandle<DeviceTag>((nint)device), msg));
        }
        finally
        {
            gch.Free();
        }
    }

    public NativeHandle<QueueTag> GetQueue(NativeHandle<DeviceTag> device)
    {
        var queue = wgpu.DeviceGetQueue((Silk.NET.WebGPU.Device*)device.Ptr);
        return new NativeHandle<QueueTag>((nint)queue);
    }

    public NativeHandle<SwapChainTag> CreateSwapChain(NativeHandle<DeviceTag> device, NativeHandle<SurfaceTag> surface, in SwapChainOptions descriptor)
    {
        var cfg = new Silk.NET.WebGPU.SurfaceConfiguration(
            device: (Silk.NET.WebGPU.Device*)device.Ptr,
            format: (Silk.NET.WebGPU.TextureFormat)descriptor.Format,
            alphaMode: Silk.NET.WebGPU.CompositeAlphaMode.Premultiplied,
            usage: (Silk.NET.WebGPU.TextureUsage)descriptor.Usage,
            presentMode: descriptor.PresentMode switch
            {
                PlatformPresentMode.Fifo => Silk.NET.WebGPU.PresentMode.Fifo,
                PlatformPresentMode.Mailbox => Silk.NET.WebGPU.PresentMode.Mailbox,
                _ => Silk.NET.WebGPU.PresentMode.Immediate
            },
            width: descriptor.Width,
            height: descriptor.Height
        );
        wgpu.SurfaceConfigure((Silk.NET.WebGPU.Surface*)surface.Ptr, ref cfg);
        return new NativeHandle<SwapChainTag>(0);
    }

    public NativeHandle<BufferTag> DeviceCreateBuffer(NativeHandle<DeviceTag> device, in BufferDescriptor descriptor, Utf8Name label)
    {
        var nativeDescriptor = new Silk.NET.WebGPU.BufferDescriptor(
            label: (byte*)label.Pointer,
            usage: (Silk.NET.WebGPU.BufferUsage)descriptor.Usage,
            size: descriptor.Size,
            mappedAtCreation: descriptor.MappedAtCreation
        );
        var buffer = wgpu.DeviceCreateBuffer((Silk.NET.WebGPU.Device*)device.Ptr, in nativeDescriptor);
        return new NativeHandle<BufferTag>((nint)buffer);
    }

    public void BufferDestroy(NativeHandle<BufferTag> buffer)
    {
        wgpu.BufferDestroy((Silk.NET.WebGPU.Buffer*)buffer.Ptr);
    }

    public void BufferRelease(NativeHandle<BufferTag> buffer)
    {
        wgpu.BufferRelease((Silk.NET.WebGPU.Buffer*)buffer.Ptr);
    }

    public void QueueWriteBuffer(NativeHandle<QueueTag> queue, NativeHandle<BufferTag> buffer, nuint offset, void* data, nuint size)
    {
        wgpu.QueueWriteBuffer((Silk.NET.WebGPU.Queue*)queue.Ptr, (Silk.NET.WebGPU.Buffer*)buffer.Ptr, offset, data, size);
    }

    public NativeHandle<TextureTag> DeviceCreateTexture(NativeHandle<DeviceTag> device, in TextureDescriptor descriptor, Utf8Name label)
    {
        var nativeDescriptor = new Silk.NET.WebGPU.TextureDescriptor(
            label: (byte*)label.Pointer,
            usage: (Silk.NET.WebGPU.TextureUsage)descriptor.Usage,
            dimension: (Silk.NET.WebGPU.TextureDimension)descriptor.Dimension,
            size: descriptor.Size,
            format: (Silk.NET.WebGPU.TextureFormat)descriptor.Format,
            mipLevelCount: descriptor.MipLevelCount,
            sampleCount: descriptor.SampleCount
        );
        int viewFormatCount = 0;
        foreach (var viewFormat in descriptor.ViewFormats)
        {
            if (viewFormat == TextureFormat.Undefined) break;
            viewFormatCount++;
        }
        var viewFormats = stackalloc Silk.NET.WebGPU.TextureFormat[viewFormatCount];
        for (int i = 0; i < viewFormatCount; i++) viewFormats[i] = (Silk.NET.WebGPU.TextureFormat)descriptor.ViewFormats[i];
        nativeDescriptor.ViewFormatCount = (nuint)viewFormatCount;
        nativeDescriptor.ViewFormats = viewFormats;
        var texture = wgpu.DeviceCreateTexture((Silk.NET.WebGPU.Device*)device.Ptr, in nativeDescriptor);
        return new NativeHandle<TextureTag>((nint)texture);
    }

    public void TextureDestroy(NativeHandle<TextureTag> texture)
    {
        wgpu.TextureDestroy((Silk.NET.WebGPU.Texture*)texture.Ptr);
    }

    public void TextureRelease(NativeHandle<TextureTag> texture)
    {
        wgpu.TextureRelease((Silk.NET.WebGPU.Texture*)texture.Ptr);
    }

    public void QueueWriteTexture(NativeHandle<QueueTag> queue, NativeHandle<TextureTag> texture, uint mipLevel, uint originX, uint originY, uint originZ, void* data, nuint dataSize, uint bytesPerRow, uint rowsPerImage, uint writeWidth, uint writeHeight, uint writeDepth)
    {
        var destination = new Silk.NET.WebGPU.ImageCopyTexture(
            texture: (Silk.NET.WebGPU.Texture*)texture.Ptr,
            mipLevel: mipLevel,
            origin: new Silk.NET.WebGPU.Origin3D(originX, originY, originZ)
        );
        var layout = new Silk.NET.WebGPU.TextureDataLayout(offset: 0, bytesPerRow: bytesPerRow, rowsPerImage: rowsPerImage);
        var writeSize = new Silk.NET.WebGPU.Extent3D(writeWidth, writeHeight, writeDepth);
        wgpu.QueueWriteTexture((Silk.NET.WebGPU.Queue*)queue.Ptr, in destination, data, dataSize, in layout, in writeSize);
    }

    public void Dispose()
    {
        wgpu.Dispose();
    }

    public NativeHandle<SamplerTag> DeviceCreateSampler(NativeHandle<DeviceTag> device, in SamplerDescriptor descriptor, Utf8Name label)
    {
        var native = new Silk.NET.WebGPU.SamplerDescriptor(
            label: (byte*)label.Pointer,
            addressModeU: descriptor.AddressModeU,
            addressModeV: descriptor.AddressModeV,
            addressModeW: descriptor.AddressModeW,
            magFilter: descriptor.MagFilter,
            minFilter: descriptor.MinFilter,
            mipmapFilter: descriptor.MipmapFilter,
            lodMinClamp: descriptor.LodMinClamp,
            lodMaxClamp: descriptor.LodMaxClamp,
            maxAnisotropy: descriptor.MaxAnisotropy
        );
        var sampler = wgpu.DeviceCreateSampler((Silk.NET.WebGPU.Device*)device.Ptr, in native);
        return new NativeHandle<SamplerTag>((nint)sampler);
    }

    public void SamplerRelease(NativeHandle<SamplerTag> sampler)
    {
        wgpu.SamplerRelease((Silk.NET.WebGPU.Sampler*)sampler.Ptr);
    }

    public NativeHandle<BindGroupLayoutTag> DeviceCreateBindGroupLayout(NativeHandle<DeviceTag> device, in BindGroupLayoutDescriptor descriptor, Utf8Name label)
    {
        var entries = new Silk.NET.WebGPU.BindGroupLayoutEntry[descriptor.Entries.Length];
        for (int i = 0; i < descriptor.Entries.Length; i++)
        {
            entries[i] = new Silk.NET.WebGPU.BindGroupLayoutEntry(
                binding: descriptor.Entries[i].Binding,
                visibility: (Silk.NET.WebGPU.ShaderStage)descriptor.Entries[i].Visibility,
                buffer: new Silk.NET.WebGPU.BufferBindingLayout(
                    type: (Silk.NET.WebGPU.BufferBindingType)descriptor.Entries[i].Buffer.Type,
                    minBindingSize: descriptor.Entries[i].Buffer.MinBindingSize,
                    hasDynamicOffset: descriptor.Entries[i].Buffer.HasDynamicOffset
                ),
                sampler: new Silk.NET.WebGPU.SamplerBindingLayout(
                    type: (Silk.NET.WebGPU.SamplerBindingType)descriptor.Entries[i].Sampler.Type
                ),
                texture: new Silk.NET.WebGPU.TextureBindingLayout(
                    sampleType: (Silk.NET.WebGPU.TextureSampleType)descriptor.Entries[i].Texture.SampleType,
                    viewDimension: (Silk.NET.WebGPU.TextureViewDimension)descriptor.Entries[i].Texture.ViewDimension,
                    multisampled: descriptor.Entries[i].Texture.Multisampled
                ),
                storageTexture: new Silk.NET.WebGPU.StorageTextureBindingLayout(
                    access: (Silk.NET.WebGPU.StorageTextureAccess)descriptor.Entries[i].StorageTexture.Access,
                    format: (Silk.NET.WebGPU.TextureFormat)descriptor.Entries[i].StorageTexture.Format,
                    viewDimension: (Silk.NET.WebGPU.TextureViewDimension)descriptor.Entries[i].StorageTexture.ViewDimension
                )
            );
        }
        var entriesSpan = entries.AsSpan();
        fixed (Silk.NET.WebGPU.BindGroupLayoutEntry* entriesPtr = entriesSpan)
        {
            var native = new Silk.NET.WebGPU.BindGroupLayoutDescriptor(
                label: (byte*)label.Pointer,
                entryCount: (uint)entries.Length,
                entries: entriesPtr
            );
            var handle = wgpu.DeviceCreateBindGroupLayout((Silk.NET.WebGPU.Device*)device.Ptr, in native);
            return new NativeHandle<BindGroupLayoutTag>((nint)handle);
        }
    }

    public void BindGroupLayoutRelease(NativeHandle<BindGroupLayoutTag> layout)
    {
        wgpu.BindGroupLayoutRelease((Silk.NET.WebGPU.BindGroupLayout*)layout.Ptr);
    }

    public NativeHandle<BindGroupTag> DeviceCreateBindGroup(NativeHandle<DeviceTag> device, in BindGroupDescriptor descriptor, Utf8Name label)
    {
        var native = new Silk.NET.WebGPU.BindGroupDescriptor(
            label: (byte*)label.Pointer,
            layout: (Silk.NET.WebGPU.BindGroupLayout*)descriptor.Layout.Native
        );
        if (descriptor.Entries is BindGroupEntry[] bindGroupEntries)
        {
            native.EntryCount = (uint)bindGroupEntries.Length;
            var entries = new Silk.NET.WebGPU.BindGroupEntry[bindGroupEntries.Length];
            for (int i = 0; i < bindGroupEntries.Length; i++)
            {
                var entry = bindGroupEntries[i];
                var silkEntry = new Silk.NET.WebGPU.BindGroupEntry
                {
                    Binding = entry.Binding
                };
                if (entry.Buffer is GPUBuffer buffer)
                {
                    silkEntry.Buffer = buffer.Native.As<Silk.NET.WebGPU.Buffer>();
                    silkEntry.Offset = entry.Offset;
                    silkEntry.Size = entry.Size;
                }
                if (entry.TextureView is GPUTextureView textureView)
                {
                    silkEntry.TextureView = textureView.Native.As<Silk.NET.WebGPU.TextureView>();
                }
                if (entry.Sampler is GPUSampler sampler)
                {
                    silkEntry.Sampler = sampler.Native.As<Silk.NET.WebGPU.Sampler>();
                }
                entries[i] = silkEntry;
            }
            var entriesSpan = entries.AsSpan();
            fixed (Silk.NET.WebGPU.BindGroupEntry* entriesPtr = entriesSpan)
            {
                native.Entries = entriesPtr;
                var handle = wgpu.DeviceCreateBindGroup((Silk.NET.WebGPU.Device*)device.Ptr, in native);
                return new NativeHandle<BindGroupTag>((nint)handle);
            }
        }
        var defaultHandle = wgpu.DeviceCreateBindGroup((Silk.NET.WebGPU.Device*)device.Ptr, in native);
        return new NativeHandle<BindGroupTag>((nint)defaultHandle);
    }

    public void BindGroupRelease(NativeHandle<BindGroupTag> bindGroup)
    {
        wgpu.BindGroupRelease((Silk.NET.WebGPU.BindGroup*)bindGroup.Ptr);
    }

    public NativeHandle<CommandEncoderTag> DeviceCreateCommandEncoder(NativeHandle<DeviceTag> device, Utf8Name label)
    {
        var desc = new Silk.NET.WebGPU.CommandEncoderDescriptor(label: (byte*)label.Pointer);
        var enc = wgpu.DeviceCreateCommandEncoder((Silk.NET.WebGPU.Device*)device.Ptr, in desc);
        return new NativeHandle<CommandEncoderTag>((nint)enc);
    }

    public void CommandEncoderRelease(NativeHandle<CommandEncoderTag> encoder)
    {
        wgpu.CommandEncoderRelease((Silk.NET.WebGPU.CommandEncoder*)encoder.Ptr);
    }

    public NativeHandle<CommandBufferTag> CommandEncoderFinish(NativeHandle<CommandEncoderTag> encoder, Utf8Name label)
    {
        var desc = new Silk.NET.WebGPU.CommandBufferDescriptor(label: (byte*)label.Pointer);
        var buffer = wgpu.CommandEncoderFinish((Silk.NET.WebGPU.CommandEncoder*)encoder.Ptr, in desc);
        return new NativeHandle<CommandBufferTag>((nint)buffer);
    }

    public void CommandBufferRelease(NativeHandle<CommandBufferTag> buffer)
    {
        wgpu.CommandBufferRelease((Silk.NET.WebGPU.CommandBuffer*)buffer.Ptr);
    }

    public void QueueSubmit(NativeHandle<QueueTag> queue, ReadOnlySpan<NativeHandle<CommandBufferTag>> commandBuffers)
    {
        if (commandBuffers.Length == 0) return;
        var span = commandBuffers;
        var ptrs = stackalloc Silk.NET.WebGPU.CommandBuffer*[span.Length];
        for (int i = 0; i < span.Length; i++) ptrs[i] = (Silk.NET.WebGPU.CommandBuffer*)span[i].Ptr;
        wgpu.QueueSubmit((Silk.NET.WebGPU.Queue*)queue.Ptr, (nuint)span.Length, ptrs);
    }

    public NativeHandle<ShaderModuleTag> DeviceCreateShaderModule(NativeHandle<DeviceTag> device, ShaderBackend backend, Utf8Name label, Utf8Name code)
    {
        var nativeDescriptor = new Silk.NET.WebGPU.ShaderModuleDescriptor(label: (byte*)label.Pointer);
        if (backend == ShaderBackend.WGSL)
        {
            var wgsl = new Silk.NET.WebGPU.ShaderModuleWGSLDescriptor(
                chain: new Silk.NET.WebGPU.ChainedStruct(sType: Silk.NET.WebGPU.SType.ShaderModuleWgslDescriptor),
                code: (byte*)code.Pointer
            );
            nativeDescriptor.NextInChain = (Silk.NET.WebGPU.ChainedStruct*)&wgsl;
        }
        var handle = wgpu.DeviceCreateShaderModule((Silk.NET.WebGPU.Device*)device.Ptr, &nativeDescriptor);
        return new NativeHandle<ShaderModuleTag>((nint)handle);
    }

    public void ShaderModuleRelease(NativeHandle<ShaderModuleTag> shaderModule)
    {
        wgpu.ShaderModuleRelease((Silk.NET.WebGPU.ShaderModule*)shaderModule.Ptr);
    }

    public NativeHandle<PipelineLayoutTag> DeviceCreatePipelineLayout(NativeHandle<DeviceTag> device, in PipelineLayoutDescriptor descriptor, Utf8Name label)
    {
        var layouts = stackalloc Silk.NET.WebGPU.BindGroupLayout*[descriptor.Layouts.Length];
        for (int i = 0; i < descriptor.Layouts.Length; i++) layouts[i] = (Silk.NET.WebGPU.BindGroupLayout*)descriptor.Layouts[i].Native;
        var native = new Silk.NET.WebGPU.PipelineLayoutDescriptor(
            label: (byte*)label.Pointer,
            bindGroupLayoutCount: (uint)descriptor.Layouts.Length,
            bindGroupLayouts: layouts
        );
        var handle = wgpu.DeviceCreatePipelineLayout((Silk.NET.WebGPU.Device*)device.Ptr, in native);
        return new NativeHandle<PipelineLayoutTag>((nint)handle);
    }

    public void PipelineLayoutRelease(NativeHandle<PipelineLayoutTag> layout)
    {
        wgpu.PipelineLayoutRelease((Silk.NET.WebGPU.PipelineLayout*)layout.Ptr);
    }

    public NativeHandle<ComputePipelineTag> DeviceCreateComputePipeline(NativeHandle<DeviceTag> device, in ComputePipelineDescriptor descriptor, Utf8Name label)
    {
        var entryPoint = descriptor.Compute.EntryPoint;
        using var entry = new Pollus.Collections.NativeUtf8(entryPoint);
        var native = new Silk.NET.WebGPU.ComputePipelineDescriptor(
            label: (byte*)label.Pointer,
            layout: descriptor.Layout == null ? null : descriptor.Layout.Native.As<Silk.NET.WebGPU.PipelineLayout>(),
            compute: new(
                module: descriptor.Compute.Shader.Native.As<Silk.NET.WebGPU.ShaderModule>(),
                entryPoint: entry.Pointer,
                constantCount: (nuint)descriptor.Compute.Constants.Length,
                constants: descriptor.Compute.Constants.Length == 0 ? null : (Silk.NET.WebGPU.ConstantEntry*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(descriptor.Compute.Constants))
            )
        );
        var handle = wgpu.DeviceCreateComputePipeline((Silk.NET.WebGPU.Device*)device.Ptr, in native);
        return new NativeHandle<ComputePipelineTag>((nint)handle);
    }

    public void ComputePipelineRelease(NativeHandle<ComputePipelineTag> pipeline)
    {
        wgpu.ComputePipelineRelease((Silk.NET.WebGPU.ComputePipeline*)pipeline.Ptr);
    }

    public NativeHandle<TextureViewTag> TextureCreateView(NativeHandle<TextureTag> texture, in TextureViewDescriptor descriptor, Utf8Name label)
    {
        var native = new Silk.NET.WebGPU.TextureViewDescriptor(
            label: (byte*)label.Pointer,
            format: (Silk.NET.WebGPU.TextureFormat)descriptor.Format,
            dimension: (Silk.NET.WebGPU.TextureViewDimension)descriptor.Dimension,
            baseMipLevel: descriptor.BaseMipLevel,
            mipLevelCount: descriptor.MipLevelCount,
            baseArrayLayer: descriptor.BaseArrayLayer,
            arrayLayerCount: descriptor.ArrayLayerCount,
            aspect: descriptor.Aspect
        );
        var view = wgpu.TextureCreateView((Silk.NET.WebGPU.Texture*)texture.Ptr, in native);
        return new NativeHandle<TextureViewTag>((nint)view);
    }

    public void TextureViewRelease(NativeHandle<TextureViewTag> view)
    {
        wgpu.TextureViewRelease((Silk.NET.WebGPU.TextureView*)view.Ptr);
    }

    public NativeHandle<RenderPipelineTag> DeviceCreateRenderPipeline(NativeHandle<DeviceTag> device, in RenderPipelineDescriptor descriptor, Utf8Name label)
    {
        using var pins = new Pollus.Utils.TemporaryPins();
        var nativeDescriptor = new Silk.NET.WebGPU.RenderPipelineDescriptor(label: (byte*)label.Pointer);
        if (descriptor.VertexState is VertexState vertexState)
        {
            pins.Pin(vertexState.EntryPoint);
            nativeDescriptor.Vertex = new Silk.NET.WebGPU.VertexState(
                module: vertexState.ShaderModule.Native.As<Silk.NET.WebGPU.ShaderModule>(),
                entryPoint: (byte*)pins.PinString(vertexState.EntryPoint).AddrOfPinnedObject()
            );
            if (vertexState.Constants is ConstantEntry[] constantEntries)
            {
                nativeDescriptor.Vertex.ConstantCount = (uint)constantEntries.Length;
                var constants = new Silk.NET.WebGPU.ConstantEntry[constantEntries.Length];
                for (int i = 0; i < constantEntries.Length; i++)
                {
                    pins.Pin(constantEntries[i].Key);
                    constants[i] = new(
                        key: (byte*)pins.PinString(constantEntries[i].Key).AddrOfPinnedObject(),
                        value: constantEntries[i].Value
                    );
                }
                nativeDescriptor.Vertex.Constants = (Silk.NET.WebGPU.ConstantEntry*)pins.Pin(constants).AddrOfPinnedObject();
            }
            if (vertexState.Layouts is VertexBufferLayout[] vertexBufferLayouts)
            {
                nativeDescriptor.Vertex.BufferCount = (uint)vertexBufferLayouts.Length;
                var layouts = new Silk.NET.WebGPU.VertexBufferLayout[vertexBufferLayouts.Length];
                for (int i = 0; i < vertexBufferLayouts.Length; i++)
                {
                    var vertexBufferLayout = vertexBufferLayouts[i];
                    layouts[i] = new(
                        arrayStride: vertexBufferLayout.Stride,
                        stepMode: (Silk.NET.WebGPU.VertexStepMode)vertexBufferLayout.StepMode,
                        attributes: (Silk.NET.WebGPU.VertexAttribute*)pins.Pin(vertexBufferLayout.Attributes).AddrOfPinnedObject(),
                        attributeCount: (uint)vertexBufferLayout.Attributes.Length
                    );
                }
                nativeDescriptor.Vertex.Buffers = (Silk.NET.WebGPU.VertexBufferLayout*)pins.Pin(layouts).AddrOfPinnedObject();
            }
        }
        if (descriptor.FragmentState is FragmentState fragmentState)
        {
            var fragment = new Silk.NET.WebGPU.FragmentState(
                module: fragmentState.ShaderModule.Native.As<Silk.NET.WebGPU.ShaderModule>(),
                entryPoint: (byte*)pins.PinString(fragmentState.EntryPoint).AddrOfPinnedObject()
            );
            if (fragmentState.Constants is ConstantEntry[] constantEntries)
            {
                fragment.ConstantCount = (uint)constantEntries.Length;
                var constants = new Silk.NET.WebGPU.ConstantEntry[constantEntries.Length];
                for (int i = 0; i < constantEntries.Length; i++)
                {
                    pins.Pin(constantEntries[i].Key);
                    constants[i] = new(
                        key: (byte*)pins.PinString(constantEntries[i].Key).AddrOfPinnedObject(),
                        value: constantEntries[i].Value
                    );
                }
                fragment.Constants = (Silk.NET.WebGPU.ConstantEntry*)pins.Pin(constants).AddrOfPinnedObject();
            }
            if (fragmentState.ColorTargets is ColorTargetState[] colorTargetStates)
            {
                fragment.TargetCount = (uint)colorTargetStates.Length;
                var targets = new Silk.NET.WebGPU.ColorTargetState[colorTargetStates.Length];
                for (int i = 0; i < colorTargetStates.Length; i++)
                {
                    var colorTargetState = colorTargetStates[i];
                    targets[i] = new(
                        format: (Silk.NET.WebGPU.TextureFormat)colorTargetState.Format,
                        blend: (Silk.NET.WebGPU.BlendState*)nint.Zero,
                        writeMask: colorTargetState.WriteMask
                    );
                    if (colorTargetState.Blend != null)
                    {
                        var temp = colorTargetState.Blend.Value;
                        targets[i].Blend = (Silk.NET.WebGPU.BlendState*)&temp;
                    }
                }
                fragment.Targets = (Silk.NET.WebGPU.ColorTargetState*)pins.Pin(targets).AddrOfPinnedObject();
            }
            nativeDescriptor.Fragment = &fragment;
        }
        if (descriptor.DepthStencilState is DepthStencilState depthStencilState)
        {
            var temp = new Silk.NET.WebGPU.DepthStencilState(
                format: (Silk.NET.WebGPU.TextureFormat)depthStencilState.Format,
                depthWriteEnabled: depthStencilState.DepthWriteEnabled,
                depthCompare: depthStencilState.DepthCompare,
                depthBias: depthStencilState.DepthBias,
                depthBiasSlopeScale: depthStencilState.DepthBiasSlopeScale,
                depthBiasClamp: depthStencilState.DepthBiasClamp,
                stencilFront: new Silk.NET.WebGPU.StencilFaceState(
                    compare: depthStencilState.StencilFront.Compare,
                    failOp: depthStencilState.StencilFront.FailOp,
                    depthFailOp: depthStencilState.StencilFront.DepthFailOp,
                    passOp: depthStencilState.StencilFront.PassOp
                ),
                stencilBack: new Silk.NET.WebGPU.StencilFaceState(
                    compare: depthStencilState.StencilBack.Compare,
                    failOp: depthStencilState.StencilBack.FailOp,
                    depthFailOp: depthStencilState.StencilBack.DepthFailOp,
                    passOp: depthStencilState.StencilBack.PassOp
                )
            );
            nativeDescriptor.DepthStencil = &temp;
        }
        if (descriptor.MultisampleState is MultisampleState multisampleState)
        {
            nativeDescriptor.Multisample = new Silk.NET.WebGPU.MultisampleState(
                count: multisampleState.Count,
                mask: multisampleState.Mask,
                alphaToCoverageEnabled: multisampleState.AlphaToCoverageEnabled
            );
        }
        if (descriptor.PrimitiveState is PrimitiveState primitiveState)
        {
            nativeDescriptor.Primitive = new Silk.NET.WebGPU.PrimitiveState(
                topology: (Silk.NET.WebGPU.PrimitiveTopology)primitiveState.Topology,
                stripIndexFormat: (Silk.NET.WebGPU.IndexFormat)primitiveState.IndexFormat,
                frontFace: (Silk.NET.WebGPU.FrontFace)primitiveState.FrontFace,
                cullMode: (Silk.NET.WebGPU.CullMode)primitiveState.CullMode
            );
        }
        if (descriptor.PipelineLayout is GPUPipelineLayout pipelineLayout)
        {
            nativeDescriptor.Layout = pipelineLayout.Native.As<Silk.NET.WebGPU.PipelineLayout>();
        }
        var pipeline = wgpu.DeviceCreateRenderPipeline((Silk.NET.WebGPU.Device*)device.Ptr, in nativeDescriptor);
        return new NativeHandle<RenderPipelineTag>((nint)pipeline);
    }

    public void RenderPipelineRelease(NativeHandle<RenderPipelineTag> pipeline)
    {
        wgpu.RenderPipelineRelease((Silk.NET.WebGPU.RenderPipeline*)pipeline.Ptr);
    }

    public NativeHandle<RenderPassEncoderTag> CommandEncoderBeginRenderPass(NativeHandle<CommandEncoderTag> encoder, in RenderPassDescriptor descriptor)
    {
        var colorAttachments = stackalloc Silk.NET.WebGPU.RenderPassColorAttachment[descriptor.ColorAttachments.Length];
        for (int i = 0; i < descriptor.ColorAttachments.Length; i++)
        {
            var ca = descriptor.ColorAttachments[i];
            colorAttachments[i] = new Silk.NET.WebGPU.RenderPassColorAttachment(
                view: ca.View.As<Silk.NET.WebGPU.TextureView>(),
                resolveTarget: ca.ResolveTarget.HasValue ? ca.ResolveTarget.Value.As<Silk.NET.WebGPU.TextureView>() : null,
                loadOp: (Silk.NET.WebGPU.LoadOp)ca.LoadOp,
                storeOp: (Silk.NET.WebGPU.StoreOp)ca.StoreOp,
                clearValue: new Silk.NET.WebGPU.Color { R = ca.ClearValue.X, G = ca.ClearValue.Y, B = ca.ClearValue.Z, A = ca.ClearValue.W }
            );
        }

        var rp = new Silk.NET.WebGPU.RenderPassDescriptor
        {
            Label = null,
            ColorAttachmentCount = (uint)descriptor.ColorAttachments.Length,
            ColorAttachments = colorAttachments
        };
        var handle = wgpu.CommandEncoderBeginRenderPass((Silk.NET.WebGPU.CommandEncoder*)encoder.Ptr, in rp);
        return new NativeHandle<RenderPassEncoderTag>((nint)handle);
    }

    public void RenderPassEncoderEnd(NativeHandle<RenderPassEncoderTag> pass)
    {
        wgpu.RenderPassEncoderEnd((Silk.NET.WebGPU.RenderPassEncoder*)pass.Ptr);
        wgpu.RenderPassEncoderRelease((Silk.NET.WebGPU.RenderPassEncoder*)pass.Ptr);
    }

    public void RenderPassEncoderSetPipeline(NativeHandle<RenderPassEncoderTag> pass, NativeHandle<RenderPipelineTag> pipeline)
    {
        wgpu.RenderPassEncoderSetPipeline((Silk.NET.WebGPU.RenderPassEncoder*)pass.Ptr, (Silk.NET.WebGPU.RenderPipeline*)pipeline.Ptr);
    }

    public void RenderPassEncoderSetViewport(NativeHandle<RenderPassEncoderTag> pass, float x, float y, float width, float height, float minDepth, float maxDepth)
    {
        wgpu.RenderPassEncoderSetViewport((Silk.NET.WebGPU.RenderPassEncoder*)pass.Ptr, x, y, width, height, minDepth, maxDepth);
    }

    public void RenderPassEncoderSetScissorRect(NativeHandle<RenderPassEncoderTag> pass, uint x, uint y, uint width, uint height)
    {
        wgpu.RenderPassEncoderSetScissorRect((Silk.NET.WebGPU.RenderPassEncoder*)pass.Ptr, x, y, width, height);
    }

    public void RenderPassEncoderSetBlendConstant(NativeHandle<RenderPassEncoderTag> pass, double r, double g, double b, double a)
    {
        var c = new Silk.NET.WebGPU.Color { R = r, G = g, B = b, A = a };
        wgpu.RenderPassEncoderSetBlendConstant((Silk.NET.WebGPU.RenderPassEncoder*)pass.Ptr, in c);
    }

    public void RenderPassEncoderSetBindGroup(NativeHandle<RenderPassEncoderTag> pass, uint groupIndex, NativeHandle<BindGroupTag> bindGroup, uint dynamicOffsetCount, uint* dynamicOffsets)
    {
        if (dynamicOffsetCount > 0)
        {
            wgpu.RenderPassEncoderSetBindGroup((Silk.NET.WebGPU.RenderPassEncoder*)pass.Ptr, groupIndex, (Silk.NET.WebGPU.BindGroup*)bindGroup.Ptr, dynamicOffsetCount, dynamicOffsets);
        }
        else
        {
            wgpu.RenderPassEncoderSetBindGroup((Silk.NET.WebGPU.RenderPassEncoder*)pass.Ptr, groupIndex, (Silk.NET.WebGPU.BindGroup*)bindGroup.Ptr, 0, null);
        }
    }

    public void RenderPassEncoderSetVertexBuffer(NativeHandle<RenderPassEncoderTag> pass, uint slot, NativeHandle<BufferTag> buffer, ulong offset, ulong size)
    {
        wgpu.RenderPassEncoderSetVertexBuffer((Silk.NET.WebGPU.RenderPassEncoder*)pass.Ptr, slot, (Silk.NET.WebGPU.Buffer*)buffer.Ptr, offset, size);
    }

    public void RenderPassEncoderSetIndexBuffer(NativeHandle<RenderPassEncoderTag> pass, NativeHandle<BufferTag> buffer, IndexFormat format, ulong offset, ulong size)
    {
        wgpu.RenderPassEncoderSetIndexBuffer((Silk.NET.WebGPU.RenderPassEncoder*)pass.Ptr, (Silk.NET.WebGPU.Buffer*)buffer.Ptr, (Silk.NET.WebGPU.IndexFormat)format, offset, size);
    }

    public void RenderPassEncoderDraw(NativeHandle<RenderPassEncoderTag> pass, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance)
    {
        wgpu.RenderPassEncoderDraw((Silk.NET.WebGPU.RenderPassEncoder*)pass.Ptr, vertexCount, instanceCount, firstVertex, firstInstance);
    }

    public void RenderPassEncoderDrawIndirect(NativeHandle<RenderPassEncoderTag> pass, NativeHandle<BufferTag> buffer, uint offset)
    {
        wgpu.RenderPassEncoderDrawIndirect((Silk.NET.WebGPU.RenderPassEncoder*)pass.Ptr, (Silk.NET.WebGPU.Buffer*)buffer.Ptr, offset);
    }

    public void RenderPassEncoderDrawIndexed(NativeHandle<RenderPassEncoderTag> pass, uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance)
    {
        wgpu.RenderPassEncoderDrawIndexed((Silk.NET.WebGPU.RenderPassEncoder*)pass.Ptr, indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
    }

    public void RenderPassEncoderDrawIndexedIndirect(NativeHandle<RenderPassEncoderTag> pass, NativeHandle<BufferTag> buffer, uint offset)
    {
        wgpu.RenderPassEncoderDrawIndexedIndirect((Silk.NET.WebGPU.RenderPassEncoder*)pass.Ptr, (Silk.NET.WebGPU.Buffer*)buffer.Ptr, offset);
    }

    public void CommandEncoderCopyTextureToTexture(NativeHandle<CommandEncoderTag> encoder, NativeHandle<TextureTag> srcTexture, NativeHandle<TextureTag> dstTexture, uint width, uint height, uint depthOrArrayLayers)
    {
        var size = new Silk.NET.WebGPU.Extent3D(width, height, depthOrArrayLayers);
        var src = new Silk.NET.WebGPU.ImageCopyTexture(texture: (Silk.NET.WebGPU.Texture*)srcTexture.Ptr);
        var dst = new Silk.NET.WebGPU.ImageCopyTexture(texture: (Silk.NET.WebGPU.Texture*)dstTexture.Ptr);
        wgpu.CommandEncoderCopyTextureToTexture((Silk.NET.WebGPU.CommandEncoder*)encoder.Ptr, in src, in dst, in size);
    }

    public NativeHandle<ComputePassEncoderTag> CommandEncoderBeginComputePass(NativeHandle<CommandEncoderTag> encoder, Utf8Name label)
    {
        var desc = new Silk.NET.WebGPU.ComputePassDescriptor(label: (byte*)label.Pointer);
        var enc = wgpu.CommandEncoderBeginComputePass((Silk.NET.WebGPU.CommandEncoder*)encoder.Ptr, in desc);
        return new NativeHandle<ComputePassEncoderTag>((nint)enc);
    }

    public void ComputePassEncoderEnd(NativeHandle<ComputePassEncoderTag> pass)
    {
        wgpu.ComputePassEncoderEnd((Silk.NET.WebGPU.ComputePassEncoder*)pass.Ptr);
        wgpu.ComputePassEncoderRelease((Silk.NET.WebGPU.ComputePassEncoder*)pass.Ptr);
    }

    public void ComputePassEncoderSetPipeline(NativeHandle<ComputePassEncoderTag> pass, NativeHandle<ComputePipelineTag> pipeline)
    {
        wgpu.ComputePassEncoderSetPipeline((Silk.NET.WebGPU.ComputePassEncoder*)pass.Ptr, (Silk.NET.WebGPU.ComputePipeline*)pipeline.Ptr);
    }

    public void ComputePassEncoderSetBindGroup(NativeHandle<ComputePassEncoderTag> pass, uint groupIndex, NativeHandle<BindGroupTag> bindGroup, uint dynamicOffsetCount, uint* dynamicOffsets)
    {
        if (dynamicOffsetCount > 0)
        {
            wgpu.ComputePassEncoderSetBindGroup((Silk.NET.WebGPU.ComputePassEncoder*)pass.Ptr, groupIndex, (Silk.NET.WebGPU.BindGroup*)bindGroup.Ptr, dynamicOffsetCount, dynamicOffsets);
        }
        else
        {
            wgpu.ComputePassEncoderSetBindGroup((Silk.NET.WebGPU.ComputePassEncoder*)pass.Ptr, groupIndex, (Silk.NET.WebGPU.BindGroup*)bindGroup.Ptr, 0, null);
        }
    }

    public void ComputePassEncoderDispatchWorkgroups(NativeHandle<ComputePassEncoderTag> pass, uint x, uint y, uint z)
    {
        wgpu.ComputePassEncoderDispatchWorkgroups((Silk.NET.WebGPU.ComputePassEncoder*)pass.Ptr, x, y, z);
    }
}


