namespace Pollus.Graphics.Platform.Emscripten;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.Platform;
using Pollus.Collections;
using Pollus.Utils;
using Pollus.Emscripten;

public unsafe class EmscriptenWgpuBackend : IWgpuBackend
{
    public readonly Pollus.Emscripten.WGPUBrowser wgpu;

    public EmscriptenWgpuBackend(Pollus.Emscripten.WGPUBrowser wgpu)
    {
        this.wgpu = wgpu;
    }

    static Pollus.Emscripten.WGPU.WGPUTextureFormat Map(TextureFormat value)
    {
        return value switch
        {
            TextureFormat.Rgba8Unorm => Pollus.Emscripten.WGPU.WGPUTextureFormat.RGBA8Unorm,
            TextureFormat.Rgba8UnormSrgb => Pollus.Emscripten.WGPU.WGPUTextureFormat.RGBA8UnormSrgb,
            TextureFormat.Bgra8Unorm => Pollus.Emscripten.WGPU.WGPUTextureFormat.BGRA8Unorm,
            TextureFormat.Bgra8UnormSrgb => Pollus.Emscripten.WGPU.WGPUTextureFormat.BGRA8UnormSrgb,
            TextureFormat.Depth24Plus => Pollus.Emscripten.WGPU.WGPUTextureFormat.Depth24Plus,
            TextureFormat.Depth24PlusStencil8 => Pollus.Emscripten.WGPU.WGPUTextureFormat.Depth24PlusStencil8,
            TextureFormat.R32Uint => Pollus.Emscripten.WGPU.WGPUTextureFormat.R32Uint,
            TextureFormat.R32Sint => Pollus.Emscripten.WGPU.WGPUTextureFormat.R32Sint,
            TextureFormat.R32float => Pollus.Emscripten.WGPU.WGPUTextureFormat.R32Float,
            TextureFormat.RG32float => Pollus.Emscripten.WGPU.WGPUTextureFormat.RG32Float,
            TextureFormat.Rgba16float => Pollus.Emscripten.WGPU.WGPUTextureFormat.RGBA16Float,
            TextureFormat.Rgba32float => Pollus.Emscripten.WGPU.WGPUTextureFormat.RGBA32Float,
            _ => (Pollus.Emscripten.WGPU.WGPUTextureFormat)(int)value
        };
    }

    static Pollus.Emscripten.WGPU.WGPUTextureUsage Map(TextureUsage value)
    {
        return (Pollus.Emscripten.WGPU.WGPUTextureUsage)(uint)value;
    }

    static Pollus.Emscripten.WGPU.WGPUTextureDimension Map(TextureDimension value)
    {
        return value switch
        {
            TextureDimension.Dimension1D => Pollus.Emscripten.WGPU.WGPUTextureDimension._1D,
            TextureDimension.Dimension2D => Pollus.Emscripten.WGPU.WGPUTextureDimension._2D,
            TextureDimension.Dimension3D => Pollus.Emscripten.WGPU.WGPUTextureDimension._3D,
            _ => Pollus.Emscripten.WGPU.WGPUTextureDimension._2D
        };
    }

    static Pollus.Emscripten.WGPU.WGPUVertexStepMode Map(VertexStepMode value)
    {
        return value switch
        {
            VertexStepMode.Vertex => Pollus.Emscripten.WGPU.WGPUVertexStepMode.Vertex,
            VertexStepMode.Instance => Pollus.Emscripten.WGPU.WGPUVertexStepMode.Instance,
            VertexStepMode.VertexBufferNotUsed => Pollus.Emscripten.WGPU.WGPUVertexStepMode.VertexBufferNotUsed,
            _ => Pollus.Emscripten.WGPU.WGPUVertexStepMode.Vertex
        };
    }

    static Pollus.Emscripten.WGPU.WGPUBlendOperation Map(BlendOperation value)
    {
        return value switch
        {
            BlendOperation.Add => Pollus.Emscripten.WGPU.WGPUBlendOperation.Add,
            BlendOperation.Subtract => Pollus.Emscripten.WGPU.WGPUBlendOperation.Subtract,
            BlendOperation.ReverseSubtract => Pollus.Emscripten.WGPU.WGPUBlendOperation.ReverseSubtract,
            BlendOperation.Min => Pollus.Emscripten.WGPU.WGPUBlendOperation.Min,
            BlendOperation.Max => Pollus.Emscripten.WGPU.WGPUBlendOperation.Max,
            _ => Pollus.Emscripten.WGPU.WGPUBlendOperation.Add
        };
    }

    static Pollus.Emscripten.WGPU.WGPUBlendFactor Map(BlendFactor value)
    {
        return value switch
        {
            BlendFactor.Zero => Pollus.Emscripten.WGPU.WGPUBlendFactor.Zero,
            BlendFactor.One => Pollus.Emscripten.WGPU.WGPUBlendFactor.One,
            BlendFactor.Src => Pollus.Emscripten.WGPU.WGPUBlendFactor.Src,
            BlendFactor.OneMinusSrc => Pollus.Emscripten.WGPU.WGPUBlendFactor.OneMinusSrc,
            BlendFactor.SrcAlpha => Pollus.Emscripten.WGPU.WGPUBlendFactor.SrcAlpha,
            BlendFactor.OneMinusSrcAlpha => Pollus.Emscripten.WGPU.WGPUBlendFactor.OneMinusSrcAlpha,
            BlendFactor.Dst => Pollus.Emscripten.WGPU.WGPUBlendFactor.Dst,
            BlendFactor.OneMinusDst => Pollus.Emscripten.WGPU.WGPUBlendFactor.OneMinusDst,
            BlendFactor.DstAlpha => Pollus.Emscripten.WGPU.WGPUBlendFactor.DstAlpha,
            BlendFactor.OneMinusDstAlpha => Pollus.Emscripten.WGPU.WGPUBlendFactor.OneMinusDstAlpha,
            BlendFactor.SrcAlphaSaturated => Pollus.Emscripten.WGPU.WGPUBlendFactor.SrcAlphaSaturated,
            BlendFactor.Constant => Pollus.Emscripten.WGPU.WGPUBlendFactor.Constant,
            BlendFactor.OneMinusConstant => Pollus.Emscripten.WGPU.WGPUBlendFactor.OneMinusConstant,
            _ => Pollus.Emscripten.WGPU.WGPUBlendFactor.One
        };
    }

    static Pollus.Emscripten.WGPU.WGPUPrimitiveTopology Map(PrimitiveTopology value)
    {
        return value switch
        {
            PrimitiveTopology.PointList => Pollus.Emscripten.WGPU.WGPUPrimitiveTopology.PointList,
            PrimitiveTopology.LineList => Pollus.Emscripten.WGPU.WGPUPrimitiveTopology.LineList,
            PrimitiveTopology.LineStrip => Pollus.Emscripten.WGPU.WGPUPrimitiveTopology.LineStrip,
            PrimitiveTopology.TriangleList => Pollus.Emscripten.WGPU.WGPUPrimitiveTopology.TriangleList,
            PrimitiveTopology.TriangleStrip => Pollus.Emscripten.WGPU.WGPUPrimitiveTopology.TriangleStrip,
            _ => Pollus.Emscripten.WGPU.WGPUPrimitiveTopology.TriangleList
        };
    }

    static Pollus.Emscripten.WGPU.WGPUAddressMode Map(Silk.NET.WebGPU.AddressMode value)
    {
        return value switch
        {
            Silk.NET.WebGPU.AddressMode.ClampToEdge => Pollus.Emscripten.WGPU.WGPUAddressMode.ClampToEdge,
            Silk.NET.WebGPU.AddressMode.Repeat => Pollus.Emscripten.WGPU.WGPUAddressMode.Repeat,
            Silk.NET.WebGPU.AddressMode.MirrorRepeat => Pollus.Emscripten.WGPU.WGPUAddressMode.MirrorRepeat,
            _ => Pollus.Emscripten.WGPU.WGPUAddressMode.ClampToEdge,
        };
    }

    static Pollus.Emscripten.WGPU.WGPUFilterMode Map(Silk.NET.WebGPU.FilterMode value)
    {
        return value switch
        {
            Silk.NET.WebGPU.FilterMode.Nearest => Pollus.Emscripten.WGPU.WGPUFilterMode.Nearest,
            Silk.NET.WebGPU.FilterMode.Linear => Pollus.Emscripten.WGPU.WGPUFilterMode.Linear,
            _ => Pollus.Emscripten.WGPU.WGPUFilterMode.Linear,
        };
    }

    static Pollus.Emscripten.WGPU.WGPUMipmapFilterMode Map(Silk.NET.WebGPU.MipmapFilterMode value)
    {
        return value switch
        {
            Silk.NET.WebGPU.MipmapFilterMode.Nearest => Pollus.Emscripten.WGPU.WGPUMipmapFilterMode.Nearest,
            Silk.NET.WebGPU.MipmapFilterMode.Linear => Pollus.Emscripten.WGPU.WGPUMipmapFilterMode.Linear,
            _ => Pollus.Emscripten.WGPU.WGPUMipmapFilterMode.Nearest,
        };
    }

    public NativeHandle<InstanceTag> CreateInstance()
    {
        var instance = wgpu.CreateInstance(null);
        return new NativeHandle<InstanceTag>((nint)instance);
    }

    public void ReleaseInstance(NativeHandle<InstanceTag> instance)
    {
        wgpu.InstanceRelease(instance.As<Pollus.Emscripten.WGPU.WGPUInstance>());
    }

    public NativeHandle<BufferTag> DeviceCreateBuffer(in NativeHandle<DeviceTag> device, in BufferDescriptor descriptor, in NativeUtf8 label)
    {
        var native = new Pollus.Emscripten.WGPU.WGPUBufferDescriptor
        {
            Label = (byte*)label.Pointer,
            Usage = (Pollus.Emscripten.WGPU.WGPUBufferUsage)descriptor.Usage,
            Size = descriptor.Size,
            MappedAtCreation = descriptor.MappedAtCreation
        };
        var buffer = wgpu.DeviceCreateBuffer(device.As<Pollus.Emscripten.WGPU.WGPUDevice>(), in native);
        return new NativeHandle<BufferTag>((nint)buffer);
    }

    public void BufferDestroy(in NativeHandle<BufferTag> buffer)
    {
        wgpu.BufferDestroy(buffer.As<Pollus.Emscripten.WGPU.WGPUBuffer>());
    }

    public void BufferRelease(in NativeHandle<BufferTag> buffer)
    {
        wgpu.BufferRelease(buffer.As<Pollus.Emscripten.WGPU.WGPUBuffer>());
    }

    public void QueueWriteBuffer(in NativeHandle<QueueTag> queue, in NativeHandle<BufferTag> buffer, nuint offset, ReadOnlySpan<byte> data, uint alignedSize)
    {
        fixed (byte* p = data)
        {
            wgpu.QueueWriteBuffer(queue.As<Pollus.Emscripten.WGPU.WGPUQueue>(), buffer.As<Pollus.Emscripten.WGPU.WGPUBuffer>(), offset, p, (nuint)alignedSize);
        }
    }

    public NativeHandle<TextureTag> DeviceCreateTexture(in NativeHandle<DeviceTag> device, in TextureDescriptor descriptor, in NativeUtf8 label)
    {
        var native = new Pollus.Emscripten.WGPU.WGPUTextureDescriptor
        {
            Label = (byte*)label.Pointer,
            Usage = Map(descriptor.Usage),
            Dimension = Map(descriptor.Dimension),
            Size = new Pollus.Emscripten.WGPU.WGPUExtent3D
            {
                Width = descriptor.Size.Width,
                Height = descriptor.Size.Height,
                DepthOrArrayLayers = descriptor.Size.DepthOrArrayLayers
            },
            Format = Map(descriptor.Format),
            MipLevelCount = descriptor.MipLevelCount,
            SampleCount = descriptor.SampleCount,
            ViewFormatCount = 0,
            ViewFormats = null
        };
        var texture = wgpu.DeviceCreateTexture(device.As<Pollus.Emscripten.WGPU.WGPUDevice>(), in native);
        return new NativeHandle<TextureTag>((nint)texture);
    }

    public void TextureDestroy(in NativeHandle<TextureTag> texture)
    {
        wgpu.TextureDestroy(texture.As<Pollus.Emscripten.WGPU.WGPUTexture>());
    }

    public void TextureRelease(in NativeHandle<TextureTag> texture)
    {
        wgpu.TextureRelease(texture.As<Pollus.Emscripten.WGPU.WGPUTexture>());
    }

    public void QueueWriteTexture(in NativeHandle<QueueTag> queue, in NativeHandle<TextureTag> texture, uint mipLevel, uint originX, uint originY, uint originZ, ReadOnlySpan<byte> data, uint bytesPerRow, uint rowsPerImage, uint writeWidth,
        uint writeHeight, uint writeDepth)
    {
        var dst = new Pollus.Emscripten.WGPU.WGPUImageCopyTexture
        {
            Texture = texture.As<Pollus.Emscripten.WGPU.WGPUTexture>(),
            MipLevel = mipLevel,
            Origin = new Pollus.Emscripten.WGPU.WGPUOrigin3D { X = originX, Y = originY, Z = originZ }
        };
        var layout = new Pollus.Emscripten.WGPU.WGPUTextureDataLayout
        {
            Offset = 0,
            BytesPerRow = bytesPerRow,
            RowsPerImage = rowsPerImage
        };
        var size = new Pollus.Emscripten.WGPU.WGPUExtent3D { Width = writeWidth, Height = writeHeight, DepthOrArrayLayers = writeDepth };
        fixed (byte* p = data)
        {
            wgpu.QueueWriteTexture(queue.As<Pollus.Emscripten.WGPU.WGPUQueue>(), in dst, p, (nuint)data.Length, in layout, in size);
        }
    }

    public void Dispose()
    {
    }

    public NativeHandle<SamplerTag> DeviceCreateSampler(in NativeHandle<DeviceTag> device, in SamplerDescriptor descriptor, in NativeUtf8 label)
    {
        var native = new Pollus.Emscripten.WGPU.WGPUSamplerDescriptor
        {
            Label = (byte*)label.Pointer,
            AddressModeU = Map(descriptor.AddressModeU),
            AddressModeV = Map(descriptor.AddressModeV),
            AddressModeW = Map(descriptor.AddressModeW),
            MagFilter = Map(descriptor.MagFilter),
            MinFilter = Map(descriptor.MinFilter),
            MipmapFilter = Map(descriptor.MipmapFilter),
            LodMinClamp = descriptor.LodMinClamp,
            LodMaxClamp = descriptor.LodMaxClamp,
            Compare = (Pollus.Emscripten.WGPU.WGPUCompareFunction)descriptor.Compare,
            MaxAnisotropy = descriptor.MaxAnisotropy
        };
        var handle = wgpu.DeviceCreateSampler(device.As<Pollus.Emscripten.WGPU.WGPUDevice>(), in native);
        return new NativeHandle<SamplerTag>((nint)handle);
    }

    public void SamplerRelease(in NativeHandle<SamplerTag> sampler)
    {
        wgpu.SamplerRelease(sampler.As<Pollus.Emscripten.WGPU.WGPUSampler>());
    }

    public NativeHandle<BindGroupLayoutTag> DeviceCreateBindGroupLayout(in NativeHandle<DeviceTag> device, in BindGroupLayoutDescriptor descriptor, in NativeUtf8 label)
    {
        var entriesArr = new Pollus.Emscripten.WGPU.WGPUBindGroupLayoutEntry[descriptor.Entries.Length];
        for (int i = 0; i < descriptor.Entries.Length; i++)
        {
            entriesArr[i] = new Pollus.Emscripten.WGPU.WGPUBindGroupLayoutEntry
            {
                Binding = descriptor.Entries[i].Binding,
                Visibility = (Pollus.Emscripten.WGPU.WGPUShaderStage)descriptor.Entries[i].Visibility,
                Buffer = new Pollus.Emscripten.WGPU.WGPUBufferBindingLayout
                {
                    Type = (Pollus.Emscripten.WGPU.WGPUBufferBindingType)descriptor.Entries[i].Buffer.Type,
                    MinBindingSize = descriptor.Entries[i].Buffer.MinBindingSize,
                    HasDynamicOffset = descriptor.Entries[i].Buffer.HasDynamicOffset
                },
                Sampler = new Pollus.Emscripten.WGPU.WGPUSamplerBindingLayout
                {
                    Type = (Pollus.Emscripten.WGPU.WGPUSamplerBindingType)descriptor.Entries[i].Sampler.Type
                },
                Texture = new Pollus.Emscripten.WGPU.WGPUTextureBindingLayout
                {
                    SampleType = (Pollus.Emscripten.WGPU.WGPUTextureSampleType)descriptor.Entries[i].Texture.SampleType,
                    ViewDimension = (Pollus.Emscripten.WGPU.WGPUTextureViewDimension)descriptor.Entries[i].Texture.ViewDimension,
                    Multisampled = descriptor.Entries[i].Texture.Multisampled
                },
                StorageTexture = new Pollus.Emscripten.WGPU.WGPUStorageTextureBindingLayout
                {
                    Access = (Pollus.Emscripten.WGPU.WGPUStorageTextureAccess)descriptor.Entries[i].StorageTexture.Access,
                    Format = (Pollus.Emscripten.WGPU.WGPUTextureFormat)descriptor.Entries[i].StorageTexture.Format,
                    ViewDimension = (Pollus.Emscripten.WGPU.WGPUTextureViewDimension)descriptor.Entries[i].StorageTexture.ViewDimension
                }
            };
        }

        var entriesSpan = entriesArr.AsSpan();
        fixed (Pollus.Emscripten.WGPU.WGPUBindGroupLayoutEntry* entriesPtr = entriesSpan)
        {
            var native = new Pollus.Emscripten.WGPU.WGPUBindGroupLayoutDescriptor
            {
                Label = (byte*)label.Pointer,
                EntryCount = (nuint)entriesArr.Length,
                Entries = entriesPtr
            };
            var handle = wgpu.DeviceCreateBindGroupLayout(device.As<Pollus.Emscripten.WGPU.WGPUDevice>(), in native);
            return new NativeHandle<BindGroupLayoutTag>((nint)handle);
        }
    }

    public void BindGroupLayoutRelease(in NativeHandle<BindGroupLayoutTag> layout)
    {
        wgpu.BindGroupLayoutRelease(layout.As<Pollus.Emscripten.WGPU.WGPUBindGroupLayout>());
    }

    public NativeHandle<BindGroupTag> DeviceCreateBindGroup(in NativeHandle<DeviceTag> device, in BindGroupDescriptor descriptor, in NativeUtf8 label)
    {
        var native = new Pollus.Emscripten.WGPU.WGPUBindGroupDescriptor
        {
            Label = (byte*)label.Pointer,
            Layout = descriptor.Layout.Native.As<Pollus.Emscripten.WGPU.WGPUBindGroupLayout>()
        };
        if (descriptor.Entries is BindGroupEntry[] bindGroupEntries)
        {
            native.EntryCount = (nuint)bindGroupEntries.Length;
            var entries = new Pollus.Emscripten.WGPU.WGPUBindGroupEntry[bindGroupEntries.Length];
            for (int i = 0; i < bindGroupEntries.Length; i++)
            {
                var entry = bindGroupEntries[i];
                var emsEntry = new Pollus.Emscripten.WGPU.WGPUBindGroupEntry
                {
                    Binding = entry.Binding
                };
                if (entry.Buffer is GPUBuffer buffer)
                {
                    emsEntry.Buffer = buffer.Native.As<Pollus.Emscripten.WGPU.WGPUBuffer>();
                    emsEntry.Offset = entry.Offset;
                    emsEntry.Size = entry.Size;
                }

                if (entry.TextureView is GPUTextureView textureView)
                {
                    emsEntry.TextureView = textureView.Native.As<Pollus.Emscripten.WGPU.WGPUTextureView>();
                }

                if (entry.Sampler is GPUSampler sampler)
                {
                    emsEntry.Sampler = sampler.Native.As<Pollus.Emscripten.WGPU.WGPUSampler>();
                }

                entries[i] = emsEntry;
            }

            var entriesSpan = entries.AsSpan();
            fixed (Pollus.Emscripten.WGPU.WGPUBindGroupEntry* entriesPtr = entriesSpan)
            {
                native.Entries = entriesPtr;
                var handle = wgpu.DeviceCreateBindGroup(device.As<Pollus.Emscripten.WGPU.WGPUDevice>(), in native);
                return new NativeHandle<BindGroupTag>((nint)handle);
            }
        }

        var defaultHandle = wgpu.DeviceCreateBindGroup(device.As<Pollus.Emscripten.WGPU.WGPUDevice>(), in native);
        return new NativeHandle<BindGroupTag>((nint)defaultHandle);
    }

    public void BindGroupRelease(in NativeHandle<BindGroupTag> bindGroup)
    {
        wgpu.BindGroupRelease(bindGroup.As<Pollus.Emscripten.WGPU.WGPUBindGroup>());
    }

    public NativeHandle<CommandEncoderTag> DeviceCreateCommandEncoder(in NativeHandle<DeviceTag> device, in NativeUtf8 label)
    {
        var desc = new Pollus.Emscripten.WGPU.WGPUCommandEncoderDescriptor
        {
            Label = (byte*)label.Pointer
        };
        var enc = wgpu.DeviceCreateCommandEncoder(device.As<Pollus.Emscripten.WGPU.WGPUDevice>(), in desc);
        return new NativeHandle<CommandEncoderTag>((nint)enc);
    }

    public void CommandEncoderRelease(in NativeHandle<CommandEncoderTag> encoder)
    {
        wgpu.CommandEncoderRelease(encoder.As<Pollus.Emscripten.WGPU.WGPUCommandEncoder>());
    }

    public NativeHandle<CommandBufferTag> CommandEncoderFinish(in NativeHandle<CommandEncoderTag> encoder, in NativeUtf8 label)
    {
        var desc = new Pollus.Emscripten.WGPU.WGPUCommandBufferDescriptor
        {
            Label = (byte*)label.Pointer
        };
        var buf = wgpu.CommandEncoderFinish(encoder.As<Pollus.Emscripten.WGPU.WGPUCommandEncoder>(), in desc);
        return new NativeHandle<CommandBufferTag>((nint)buf);
    }

    public void CommandBufferRelease(in NativeHandle<CommandBufferTag> buffer)
    {
        wgpu.CommandBufferRelease(buffer.As<Pollus.Emscripten.WGPU.WGPUCommandBuffer>());
    }

    public void QueueSubmit(in NativeHandle<QueueTag> queue, ReadOnlySpan<NativeHandle<CommandBufferTag>> commandBuffers)
    {
        if (commandBuffers.Length == 0) return;
        if (commandBuffers.Length == 1)
        {
            var one = commandBuffers[0].As<Pollus.Emscripten.WGPU.WGPUCommandBuffer>();
            wgpu.QueueSubmit(queue.As<Pollus.Emscripten.WGPU.WGPUQueue>(), 1, ref one);
            return;
        }

        var ptrs = stackalloc Pollus.Emscripten.WGPU.WGPUCommandBuffer*[commandBuffers.Length];
        for (int i = 0; i < commandBuffers.Length; i++) ptrs[i] = commandBuffers[i].As<Pollus.Emscripten.WGPU.WGPUCommandBuffer>();
        wgpu.QueueSubmit(queue.As<Pollus.Emscripten.WGPU.WGPUQueue>(), (nuint)commandBuffers.Length, ptrs);
    }

    public NativeHandle<ShaderModuleTag> DeviceCreateShaderModule(in NativeHandle<DeviceTag> device, ShaderBackend backend, in NativeUtf8 label, in NativeUtf8 code)
    {
        var descriptor = new Pollus.Emscripten.WGPU.WGPUShaderModuleDescriptor
        {
            Label = (byte*)label.Pointer
        };
        if (backend == ShaderBackend.WGSL)
        {
            var wgsl = new Pollus.Emscripten.WGPU.WGPUShaderModuleWGSLDescriptor
            {
                Chain = new Pollus.Emscripten.WGPU.WGPUChainedStruct
                {
                    Next = null,
                    SType = Pollus.Emscripten.WGPU.WGPUSType.ShaderModuleWGSLDescriptor
                },
                Code = (byte*)code.Pointer
            };
            descriptor.NextInChain = (Pollus.Emscripten.WGPU.WGPUChainedStruct*)&wgsl;
        }

        var handle = wgpu.DeviceCreateShaderModule(device.As<Pollus.Emscripten.WGPU.WGPUDevice>(), &descriptor);
        return new NativeHandle<ShaderModuleTag>((nint)handle);
    }

    public void ShaderModuleRelease(in NativeHandle<ShaderModuleTag> shaderModule)
    {
        wgpu.ShaderModuleRelease(shaderModule.As<Pollus.Emscripten.WGPU.WGPUShaderModule>());
    }

    public NativeHandle<PipelineLayoutTag> DeviceCreatePipelineLayout(in NativeHandle<DeviceTag> device, in PipelineLayoutDescriptor descriptor, in NativeUtf8 label)
    {
        var layoutPtrs = stackalloc nint[descriptor.Layouts.Length];
        for (int i = 0; i < descriptor.Layouts.Length; i++) layoutPtrs[i] = descriptor.Layouts[i].Native.Ptr;
        var native = new Pollus.Emscripten.WGPU.WGPUPipelineLayoutDescriptor
        {
            Label = (byte*)label.Pointer,
            BindGroupLayoutCount = (nuint)descriptor.Layouts.Length,
            BindGroupLayouts = (Pollus.Emscripten.WGPU.WGPUBindGroupLayout*)layoutPtrs
        };
        var handle = wgpu.DeviceCreatePipelineLayout(device.As<Pollus.Emscripten.WGPU.WGPUDevice>(), in native);
        return new NativeHandle<PipelineLayoutTag>((nint)handle);
    }

    public void PipelineLayoutRelease(in NativeHandle<PipelineLayoutTag> layout)
    {
        wgpu.PipelineLayoutRelease(layout.As<Pollus.Emscripten.WGPU.WGPUPipelineLayout>());
    }

    public NativeHandle<ComputePipelineTag> DeviceCreateComputePipeline(in NativeHandle<DeviceTag> device, in ComputePipelineDescriptor descriptor, in NativeUtf8 label)
    {
        using var entry = new NativeUtf8(descriptor.Compute.EntryPoint);
        var stage = new Pollus.Emscripten.WGPU.WGPUProgrammableStageDescriptor
        {
            Module = descriptor.Compute.Shader.Native.As<Pollus.Emscripten.WGPU.WGPUShaderModule>(),
            EntryPoint = (byte*)entry.Pointer,
            ConstantCount = (nuint)descriptor.Compute.Constants.Length,
            Constants = descriptor.Compute.Constants.Length == 0 ? null : (Pollus.Emscripten.WGPU.WGPUConstantEntry*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(descriptor.Compute.Constants)),
        };
        var native = new Pollus.Emscripten.WGPU.WGPUComputePipelineDescriptor
        {
            Label = (byte*)label.Pointer,
            Layout = descriptor.Layout == null ? null : descriptor.Layout.Native.As<Pollus.Emscripten.WGPU.WGPUPipelineLayout>(),
            Compute = stage
        };
        var handle = wgpu.DeviceCreateComputePipeline(device.As<Pollus.Emscripten.WGPU.WGPUDevice>(), in native);
        return new NativeHandle<ComputePipelineTag>((nint)handle);
    }

    public void ComputePipelineRelease(in NativeHandle<ComputePipelineTag> pipeline)
    {
        wgpu.ComputePipelineRelease(pipeline.As<Pollus.Emscripten.WGPU.WGPUComputePipeline>());
    }

    public NativeHandle<TextureViewTag> TextureCreateView(in NativeHandle<TextureTag> texture, in TextureViewDescriptor descriptor, in NativeUtf8 label)
    {
        var native = new Pollus.Emscripten.WGPU.WGPUTextureViewDescriptor
        {
            Label = (byte*)label.Pointer,
            Format = Map(descriptor.Format),
            Dimension = (Pollus.Emscripten.WGPU.WGPUTextureViewDimension)descriptor.Dimension,
            BaseMipLevel = descriptor.BaseMipLevel,
            MipLevelCount = descriptor.MipLevelCount,
            BaseArrayLayer = descriptor.BaseArrayLayer,
            ArrayLayerCount = descriptor.ArrayLayerCount,
            Aspect = (Pollus.Emscripten.WGPU.WGPUTextureAspect)descriptor.Aspect
        };
        var view = wgpu.TextureCreateView(texture.As<Pollus.Emscripten.WGPU.WGPUTexture>(), in native);
        return new NativeHandle<TextureViewTag>((nint)view);
    }

    public void TextureViewRelease(in NativeHandle<TextureViewTag> view)
    {
        wgpu.TextureViewRelease(view.As<Pollus.Emscripten.WGPU.WGPUTextureView>());
    }

    public NativeHandle<RenderPipelineTag> DeviceCreateRenderPipeline(in NativeHandle<DeviceTag> device, in RenderPipelineDescriptor descriptor, in NativeUtf8 label)
    {
        using var pins = new TemporaryPins();
        var native = new Pollus.Emscripten.WGPU.WGPURenderPipelineDescriptor
        {
            Label = (byte*)label.Pointer
        };
        if (descriptor.VertexState is VertexState vertexState)
        {
            var entry = pins.PinString(vertexState.EntryPoint);
            var v = new Pollus.Emscripten.WGPU.WGPUVertexState
            {
                Module = vertexState.ShaderModule.Native.As<Pollus.Emscripten.WGPU.WGPUShaderModule>(),
                EntryPoint = (byte*)entry.AddrOfPinnedObject(),
                ConstantCount = 0,
                Constants = null,
                BufferCount = 0,
                Buffers = null
            };
            if (vertexState.Constants is ConstantEntry[] vconsts && vconsts.Length > 0)
            {
                var temp = stackalloc Pollus.Emscripten.WGPU.WGPUConstantEntry[vconsts.Length];
                for (int i = 0; i < vconsts.Length; i++)
                {
                    var key = pins.PinString(vconsts[i].Key);
                    temp[i] = new Pollus.Emscripten.WGPU.WGPUConstantEntry
                    {
                        NextInChain = null,
                        Key = (byte*)key.AddrOfPinnedObject(),
                        Value = vconsts[i].Value
                    };
                }

                v.ConstantCount = (nuint)vconsts.Length;
                v.Constants = temp;
            }

            if (vertexState.Layouts is VertexBufferLayout[] layouts && layouts.Length > 0)
            {
                var vbufs = stackalloc Pollus.Emscripten.WGPU.WGPUVertexBufferLayout[layouts.Length];
                for (int i = 0; i < layouts.Length; i++)
                {
                    var l = layouts[i];
                    var attrsHandle = pins.Pin(l.Attributes);
                    vbufs[i] = new Pollus.Emscripten.WGPU.WGPUVertexBufferLayout
                    {
                        ArrayStride = l.Stride,
                        StepMode = Map(l.StepMode),
                        AttributeCount = (nuint)l.Attributes.Length,
                        Attributes = (Pollus.Emscripten.WGPU.WGPUVertexAttribute*)attrsHandle.AddrOfPinnedObject()
                    };
                }

                v.BufferCount = (nuint)layouts.Length;
                v.Buffers = vbufs;
            }

            native.Vertex = v;
        }

        var targets = stackalloc Pollus.Emscripten.WGPU.WGPUColorTargetState[8];
        var blends = stackalloc Pollus.Emscripten.WGPU.WGPUBlendState[8];
        int targetsCount = 0;
        int blendsCount = 0;
        Pollus.Emscripten.WGPU.WGPUFragmentState f = default;
        if (descriptor.FragmentState is FragmentState fragmentState)
        {
            var entry = pins.PinString(fragmentState.EntryPoint);
            f = new Pollus.Emscripten.WGPU.WGPUFragmentState
            {
                Module = fragmentState.ShaderModule.Native.As<Pollus.Emscripten.WGPU.WGPUShaderModule>(),
                EntryPoint = (byte*)entry.AddrOfPinnedObject(),
                ConstantCount = 0,
                Constants = null,
                TargetCount = 0,
                Targets = null
            };
            if (fragmentState.Constants is ConstantEntry[] fconsts && fconsts.Length > 0)
            {
                var temp = stackalloc Pollus.Emscripten.WGPU.WGPUConstantEntry[fconsts.Length];
                for (int i = 0; i < fconsts.Length; i++)
                {
                    var key = pins.PinString(fconsts[i].Key);
                    temp[i] = new Pollus.Emscripten.WGPU.WGPUConstantEntry
                    {
                        NextInChain = null,
                        Key = (byte*)key.AddrOfPinnedObject(),
                        Value = fconsts[i].Value
                    };
                }

                f.ConstantCount = (nuint)fconsts.Length;
                f.Constants = temp;
            }

            if (fragmentState.ColorTargets is ColorTargetState[] colorTargets && colorTargets.Length > 0)
            {
                for (int i = 0; i < colorTargets.Length; i++)
                {
                    var targetIdx = targetsCount++;
                    var blendIdx = blendsCount++;

                    var ct = colorTargets[i];
                    targets[targetIdx] = new Pollus.Emscripten.WGPU.WGPUColorTargetState
                    {
                        NextInChain = null,
                        Format = (Pollus.Emscripten.WGPU.WGPUTextureFormat)ct.Format,
                        Blend = null,
                        WriteMask = (Pollus.Emscripten.WGPU.WGPUColorWriteMask)ct.WriteMask
                    };
                    if (ct.Blend is BlendState b)
                    {
                        blends[blendIdx] = new Pollus.Emscripten.WGPU.WGPUBlendState
                        {
                            Color = new Pollus.Emscripten.WGPU.WGPUBlendComponent
                            {
                                Operation = Map(b.Color.Operation),
                                SrcFactor = Map(b.Color.SrcFactor),
                                DstFactor = Map(b.Color.DstFactor)
                            },
                            Alpha = new Pollus.Emscripten.WGPU.WGPUBlendComponent
                            {
                                Operation = Map(b.Alpha.Operation),
                                SrcFactor = Map(b.Alpha.SrcFactor),
                                DstFactor = Map(b.Alpha.DstFactor)
                            }
                        };
                        targets[targetIdx].Blend = &blends[blendIdx];
                    }
                }

                f.TargetCount = (nuint)colorTargets.Length;
                f.Targets = targets;
            }

            native.Fragment = &f;
        }

        Pollus.Emscripten.WGPU.WGPUDepthStencilState depthStencilStateTmp;
        if (descriptor.DepthStencilState is DepthStencilState ds)
        {
            depthStencilStateTmp = new Pollus.Emscripten.WGPU.WGPUDepthStencilState
            {
                NextInChain = null,
                Format = (Pollus.Emscripten.WGPU.WGPUTextureFormat)ds.Format,
                DepthWriteEnabled = ds.DepthWriteEnabled,
                DepthCompare = (Pollus.Emscripten.WGPU.WGPUCompareFunction)ds.DepthCompare,
                StencilFront = new Pollus.Emscripten.WGPU.WGPUStencilFaceState
                {
                    Compare = (Pollus.Emscripten.WGPU.WGPUCompareFunction)ds.StencilFront.Compare,
                    FailOp = (Pollus.Emscripten.WGPU.WGPUStencilOperation)ds.StencilFront.FailOp,
                    DepthFailOp = (Pollus.Emscripten.WGPU.WGPUStencilOperation)ds.StencilFront.DepthFailOp,
                    PassOp = (Pollus.Emscripten.WGPU.WGPUStencilOperation)ds.StencilFront.PassOp
                },
                StencilBack = new Pollus.Emscripten.WGPU.WGPUStencilFaceState
                {
                    Compare = (Pollus.Emscripten.WGPU.WGPUCompareFunction)ds.StencilBack.Compare,
                    FailOp = (Pollus.Emscripten.WGPU.WGPUStencilOperation)ds.StencilBack.FailOp,
                    DepthFailOp = (Pollus.Emscripten.WGPU.WGPUStencilOperation)ds.StencilBack.DepthFailOp,
                    PassOp = (Pollus.Emscripten.WGPU.WGPUStencilOperation)ds.StencilBack.PassOp
                },
                StencilReadMask = ds.StencilReadMask,
                StencilWriteMask = ds.StencilWriteMask,
                DepthBias = ds.DepthBias,
                DepthBiasSlopeScale = ds.DepthBiasSlopeScale,
                DepthBiasClamp = ds.DepthBiasClamp
            };
            native.DepthStencil = &depthStencilStateTmp;
        }

        if (descriptor.MultisampleState is MultisampleState ms)
        {
            native.Multisample = new Pollus.Emscripten.WGPU.WGPUMultisampleState
            {
                NextInChain = null,
                Count = ms.Count,
                Mask = ms.Mask,
                AlphaToCoverageEnabled = ms.AlphaToCoverageEnabled
            };
        }

        if (descriptor.PrimitiveState is PrimitiveState ps)
        {
            native.Primitive = new Pollus.Emscripten.WGPU.WGPUPrimitiveState
            {
                NextInChain = null,
                Topology = Map(ps.Topology),
                StripIndexFormat = (Pollus.Emscripten.WGPU.WGPUIndexFormat)ps.IndexFormat,
                FrontFace = (Pollus.Emscripten.WGPU.WGPUFrontFace)ps.FrontFace,
                CullMode = (Pollus.Emscripten.WGPU.WGPUCullMode)ps.CullMode
            };
        }

        if (descriptor.PipelineLayout is GPUPipelineLayout pl)
        {
            native.Layout = pl.Native.As<Pollus.Emscripten.WGPU.WGPUPipelineLayout>();
        }

        var handle = wgpu.DeviceCreateRenderPipeline(device.As<Pollus.Emscripten.WGPU.WGPUDevice>(), in native);
        return new NativeHandle<RenderPipelineTag>((nint)handle);
    }

    public void RenderPipelineRelease(in NativeHandle<RenderPipelineTag> pipeline)
    {
        wgpu.RenderPipelineRelease(pipeline.As<Pollus.Emscripten.WGPU.WGPURenderPipeline>());
    }

    public NativeHandle<RenderPassEncoderTag> CommandEncoderBeginRenderPass(in NativeHandle<CommandEncoderTag> encoder, in RenderPassDescriptor descriptor)
    {
        var colorAttachments = stackalloc Pollus.Emscripten.WGPU.WGPURenderPassColorAttachment[descriptor.ColorAttachments.Length];
        for (int i = 0; i < descriptor.ColorAttachments.Length; i++)
        {
            var ca = descriptor.ColorAttachments[i];
            var clear = ca.ClearValue;
            colorAttachments[i] = new Pollus.Emscripten.WGPU.WGPURenderPassColorAttachment
            {
                NextInChain = null,
                View = ca.View.As<Pollus.Emscripten.WGPU.WGPUTextureView>(),
                DepthSlice = ca.DepthSlice ?? WGPUBrowser.WGPU_DEPTH_SLICE_UNDEFINED,
                ResolveTarget = ca.ResolveTarget.HasValue ? ca.ResolveTarget.Value.As<Pollus.Emscripten.WGPU.WGPUTextureView>() : null,
                LoadOp = (Pollus.Emscripten.WGPU.WGPULoadOp)ca.LoadOp,
                StoreOp = (Pollus.Emscripten.WGPU.WGPUStoreOp)ca.StoreOp,
                ClearValue = new Pollus.Emscripten.WGPU.WGPUColor { R = clear.X, G = clear.Y, B = clear.Z, A = clear.W }
            };
        }

        var rp = new Pollus.Emscripten.WGPU.WGPURenderPassDescriptor
        {
            Label = null,
            ColorAttachmentCount = (uint)descriptor.ColorAttachments.Length,
            ColorAttachments = colorAttachments
        };
        if (descriptor.DepthStencilAttachment.HasValue)
        {
            var dsa = descriptor.DepthStencilAttachment.Value;
            var nativeDsa = new Pollus.Emscripten.WGPU.WGPURenderPassDepthStencilAttachment
            {
                View = dsa.View.Native.As<Pollus.Emscripten.WGPU.WGPUTextureView>(),
                DepthLoadOp = (Pollus.Emscripten.WGPU.WGPULoadOp)dsa.DepthLoadOp,
                DepthStoreOp = (Pollus.Emscripten.WGPU.WGPUStoreOp)dsa.DepthStoreOp,
                DepthClearValue = dsa.DepthClearValue,
                DepthReadOnly = dsa.DepthReadOnly,
                StencilLoadOp = (Pollus.Emscripten.WGPU.WGPULoadOp)dsa.StencilLoadOp,
                StencilStoreOp = (Pollus.Emscripten.WGPU.WGPUStoreOp)dsa.StencilStoreOp,
                StencilClearValue = dsa.StencilClearValue,
                StencilReadOnly = dsa.StencilReadOnly
            };
            rp.DepthStencilAttachment = &nativeDsa;
        }

        var handle = wgpu.CommandEncoderBeginRenderPass(encoder.As<Pollus.Emscripten.WGPU.WGPUCommandEncoder>(), rp);
        return new NativeHandle<RenderPassEncoderTag>((nint)handle);
    }

    public void RenderPassEncoderEnd(in NativeHandle<RenderPassEncoderTag> pass)
    {
        wgpu.RenderPassEncoderEnd(pass.As<Pollus.Emscripten.WGPU.WGPURenderPassEncoder>());
        wgpu.RenderPassEncoderRelease(pass.As<Pollus.Emscripten.WGPU.WGPURenderPassEncoder>());
    }

    public void RenderPassEncoderRelease(in NativeHandle<RenderPassEncoderTag> pass)
    {
        wgpu.RenderPassEncoderRelease(pass.As<Pollus.Emscripten.WGPU.WGPURenderPassEncoder>());
    }

    public void RenderPassEncoderSetPipeline(in NativeHandle<RenderPassEncoderTag> pass, in NativeHandle<RenderPipelineTag> pipeline)
    {
        wgpu.RenderPassEncoderSetPipeline(pass.As<Pollus.Emscripten.WGPU.WGPURenderPassEncoder>(), pipeline.As<Pollus.Emscripten.WGPU.WGPURenderPipeline>());
    }

    public void RenderPassEncoderSetViewport(in NativeHandle<RenderPassEncoderTag> pass, float x, float y, float width, float height, float minDepth, float maxDepth)
    {
        wgpu.RenderPassEncoderSetViewport(pass.As<Pollus.Emscripten.WGPU.WGPURenderPassEncoder>(), x, y, width, height, minDepth, maxDepth);
    }

    public void RenderPassEncoderSetScissorRect(in NativeHandle<RenderPassEncoderTag> pass, uint x, uint y, uint width, uint height)
    {
        wgpu.RenderPassEncoderSetScissorRect(pass.As<Pollus.Emscripten.WGPU.WGPURenderPassEncoder>(), x, y, width, height);
    }

    public void RenderPassEncoderSetBlendConstant(in NativeHandle<RenderPassEncoderTag> pass, double r, double g, double b, double a)
    {
        var c = new Pollus.Emscripten.WGPU.WGPUColor { R = r, G = g, B = b, A = a };
        wgpu.RenderPassEncoderSetBlendConstant(pass.As<Pollus.Emscripten.WGPU.WGPURenderPassEncoder>(), in c);
    }

    public void RenderPassEncoderSetBindGroup(in NativeHandle<RenderPassEncoderTag> pass, uint groupIndex, in NativeHandle<BindGroupTag> bindGroup, ReadOnlySpan<uint> dynamicOffsets)
    {
        if (dynamicOffsets.Length > 0)
        {
            fixed (uint* p = dynamicOffsets)
            {
                wgpu.RenderPassEncoderSetBindGroup(pass.As<Pollus.Emscripten.WGPU.WGPURenderPassEncoder>(), groupIndex, bindGroup.As<Pollus.Emscripten.WGPU.WGPUBindGroup>(), (nuint)dynamicOffsets.Length, p);
            }
        }
        else
        {
            wgpu.RenderPassEncoderSetBindGroup(pass.As<Pollus.Emscripten.WGPU.WGPURenderPassEncoder>(), groupIndex, bindGroup.As<Pollus.Emscripten.WGPU.WGPUBindGroup>(), 0, null);
        }
    }

    public void RenderPassEncoderSetVertexBuffer(in NativeHandle<RenderPassEncoderTag> pass, uint slot, in NativeHandle<BufferTag> buffer, ulong offset, ulong size)
    {
        wgpu.RenderPassEncoderSetVertexBuffer(pass.As<Pollus.Emscripten.WGPU.WGPURenderPassEncoder>(), slot, buffer.As<Pollus.Emscripten.WGPU.WGPUBuffer>(), offset, size);
    }

    public void RenderPassEncoderSetIndexBuffer(in NativeHandle<RenderPassEncoderTag> pass, in NativeHandle<BufferTag> buffer, IndexFormat format, ulong offset, ulong size)
    {
        wgpu.RenderPassEncoderSetIndexBuffer(pass.As<Pollus.Emscripten.WGPU.WGPURenderPassEncoder>(), buffer.As<Pollus.Emscripten.WGPU.WGPUBuffer>(), (Pollus.Emscripten.WGPU.WGPUIndexFormat)format, offset, size);
    }

    public void RenderPassEncoderDraw(in NativeHandle<RenderPassEncoderTag> pass, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance)
    {
        wgpu.RenderPassEncoderDraw(pass.As<Pollus.Emscripten.WGPU.WGPURenderPassEncoder>(), vertexCount, instanceCount, firstVertex, firstInstance);
    }

    public void RenderPassEncoderDrawIndirect(in NativeHandle<RenderPassEncoderTag> pass, in NativeHandle<BufferTag> buffer, uint offset)
    {
        wgpu.RenderPassEncoderDrawIndirect(pass.As<Pollus.Emscripten.WGPU.WGPURenderPassEncoder>(), buffer.As<Pollus.Emscripten.WGPU.WGPUBuffer>(), offset);
    }

    public void RenderPassEncoderDrawIndexed(in NativeHandle<RenderPassEncoderTag> pass, uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance)
    {
        wgpu.RenderPassEncoderDrawIndexed(pass.As<Pollus.Emscripten.WGPU.WGPURenderPassEncoder>(), indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
    }

    public void RenderPassEncoderDrawIndexedIndirect(in NativeHandle<RenderPassEncoderTag> pass, in NativeHandle<BufferTag> buffer, uint offset)
    {
        wgpu.RenderPassEncoderDrawIndexedIndirect(pass.As<Pollus.Emscripten.WGPU.WGPURenderPassEncoder>(), buffer.As<Pollus.Emscripten.WGPU.WGPUBuffer>(), offset);
    }

    public void CommandEncoderCopyTextureToTexture(in NativeHandle<CommandEncoderTag> encoder, in NativeHandle<TextureTag> srcTexture, in NativeHandle<TextureTag> dstTexture, uint width, uint height, uint depthOrArrayLayers)
    {
        var size = new Pollus.Emscripten.WGPU.WGPUExtent3D { Width = width, Height = height, DepthOrArrayLayers = depthOrArrayLayers };
        var src = new Pollus.Emscripten.WGPU.WGPUImageCopyTexture { Texture = srcTexture.As<Pollus.Emscripten.WGPU.WGPUTexture>() };
        var dst = new Pollus.Emscripten.WGPU.WGPUImageCopyTexture { Texture = dstTexture.As<Pollus.Emscripten.WGPU.WGPUTexture>() };
        wgpu.CommandEncoderCopyTextureToTexture(encoder.As<Pollus.Emscripten.WGPU.WGPUCommandEncoder>(), in src, in dst, in size);
    }

    public NativeHandle<ComputePassEncoderTag> CommandEncoderBeginComputePass(in NativeHandle<CommandEncoderTag> encoder, in NativeUtf8 label)
    {
        var desc = new Pollus.Emscripten.WGPU.WGPUComputePassDescriptor
        {
            Label = (byte*)label.Pointer
        };
        var enc = wgpu.CommandEncoderBeginComputePass(encoder.As<Pollus.Emscripten.WGPU.WGPUCommandEncoder>(), in desc);
        return new NativeHandle<ComputePassEncoderTag>((nint)enc);
    }

    public void ComputePassEncoderEnd(in NativeHandle<ComputePassEncoderTag> pass)
    {
        wgpu.ComputePassEncoderEnd(pass.As<Pollus.Emscripten.WGPU.WGPUComputePassEncoder>());
        wgpu.ComputePassEncoderRelease(pass.As<Pollus.Emscripten.WGPU.WGPUComputePassEncoder>());
    }

    public void ComputePassEncoderSetPipeline(in NativeHandle<ComputePassEncoderTag> pass, in NativeHandle<ComputePipelineTag> pipeline)
    {
        wgpu.ComputePassEncoderSetPipeline(pass.As<Pollus.Emscripten.WGPU.WGPUComputePassEncoder>(), pipeline.As<Pollus.Emscripten.WGPU.WGPUComputePipeline>());
    }

    public void ComputePassEncoderSetBindGroup(in NativeHandle<ComputePassEncoderTag> pass, uint groupIndex, in NativeHandle<BindGroupTag> bindGroup, ReadOnlySpan<uint> dynamicOffsets)
    {
        if (dynamicOffsets.Length > 0)
        {
            fixed (uint* p = dynamicOffsets)
            {
                wgpu.ComputePassEncoderSetBindGroup(pass.As<Pollus.Emscripten.WGPU.WGPUComputePassEncoder>(), groupIndex, bindGroup.As<Pollus.Emscripten.WGPU.WGPUBindGroup>(), (nuint)dynamicOffsets.Length, p);
            }
        }
        else
        {
            wgpu.ComputePassEncoderSetBindGroup(pass.As<Pollus.Emscripten.WGPU.WGPUComputePassEncoder>(), groupIndex, bindGroup.As<Pollus.Emscripten.WGPU.WGPUBindGroup>(), 0, null);
        }
    }

    public void ComputePassEncoderDispatchWorkgroups(in NativeHandle<ComputePassEncoderTag> pass, uint x, uint y, uint z)
    {
        wgpu.ComputePassEncoderDispatchWorkgroups(pass.As<Pollus.Emscripten.WGPU.WGPUComputePassEncoder>(), x, y, z);
    }

    public void CommandEncoderPushDebugGroup(in NativeHandle<CommandEncoderTag> encoder, in NativeUtf8 label)
    {
        wgpu.CommandEncoderPushDebugGroup(encoder.As<Pollus.Emscripten.WGPU.WGPUCommandEncoder>(), label.Pointer);
    }

    public void CommandEncoderPopDebugGroup(in NativeHandle<CommandEncoderTag> encoder)
    {
        wgpu.CommandEncoderPopDebugGroup(encoder.As<Pollus.Emscripten.WGPU.WGPUCommandEncoder>());
    }

    public void CommandEncoderInsertDebugMarker(in NativeHandle<CommandEncoderTag> encoder, in NativeUtf8 label)
    {
        wgpu.CommandEncoderInsertDebugMarker(encoder.As<Pollus.Emscripten.WGPU.WGPUCommandEncoder>(), label.Pointer);
    }
}


