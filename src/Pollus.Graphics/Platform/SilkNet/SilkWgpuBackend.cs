namespace Pollus.Graphics.Platform.SilkNetWgpu;

using System;
using Pollus.Graphics.Rendering;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Pollus.Graphics.Platform;
using Pollus.Collections;

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
        var instance = wgpu.CreateInstance(in desc);
        return new NativeHandle<InstanceTag>((nint)instance);
    }

    public NativeHandle<BufferTag> DeviceCreateBuffer(in NativeHandle<DeviceTag> device, in BufferDescriptor descriptor, in NativeUtf8 label)
    {
        var nativeDescriptor = new Silk.NET.WebGPU.BufferDescriptor(
            label: label.Pointer,
            usage: (Silk.NET.WebGPU.BufferUsage)descriptor.Usage,
            size: descriptor.Size,
            mappedAtCreation: descriptor.MappedAtCreation
        );
        var buffer = wgpu.DeviceCreateBuffer(device.As<Silk.NET.WebGPU.Device>(), in nativeDescriptor);
        return new NativeHandle<BufferTag>((nint)buffer);
    }

    public void BufferDestroy(in NativeHandle<BufferTag> buffer)
    {
        wgpu.BufferDestroy(buffer.As<Silk.NET.WebGPU.Buffer>());
    }

    public void BufferRelease(in NativeHandle<BufferTag> buffer)
    {
        wgpu.BufferRelease(buffer.As<Silk.NET.WebGPU.Buffer>());
    }

    public void QueueWriteBuffer(in NativeHandle<QueueTag> queue, in NativeHandle<BufferTag> buffer, nuint offset, ReadOnlySpan<byte> data, uint alignedSize)
    {
        fixed (byte* p = &data[0])
        {
            wgpu.QueueWriteBuffer(queue.As<Silk.NET.WebGPU.Queue>(), buffer.As<Silk.NET.WebGPU.Buffer>(), offset, p, (nuint)alignedSize);
        }
    }

    public NativeHandle<TextureTag> DeviceCreateTexture(in NativeHandle<DeviceTag> device, in TextureDescriptor descriptor, in NativeUtf8 label)
    {
        var nativeDescriptor = new Silk.NET.WebGPU.TextureDescriptor(
            label: label.Pointer,
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
        var texture = wgpu.DeviceCreateTexture(device.As<Silk.NET.WebGPU.Device>(), in nativeDescriptor);
        return new NativeHandle<TextureTag>((nint)texture);
    }

    public void TextureDestroy(in NativeHandle<TextureTag> texture)
    {
        wgpu.TextureDestroy(texture.As<Silk.NET.WebGPU.Texture>());
    }

    public void TextureRelease(in NativeHandle<TextureTag> texture)
    {
        wgpu.TextureRelease(texture.As<Silk.NET.WebGPU.Texture>());
    }

    public void QueueWriteTexture(in NativeHandle<QueueTag> queue, in NativeHandle<TextureTag> texture, uint mipLevel, uint originX, uint originY, uint originZ, ReadOnlySpan<byte> data, uint bytesPerRow, uint rowsPerImage, uint writeWidth,
        uint writeHeight, uint writeDepth)
    {
        var destination = new Silk.NET.WebGPU.ImageCopyTexture(
            texture: texture.As<Silk.NET.WebGPU.Texture>(),
            mipLevel: mipLevel,
            origin: new Silk.NET.WebGPU.Origin3D(originX, originY, originZ)
        );
        var layout = new Silk.NET.WebGPU.TextureDataLayout(offset: 0, bytesPerRow: bytesPerRow, rowsPerImage: rowsPerImage);
        var writeSize = new Silk.NET.WebGPU.Extent3D(writeWidth, writeHeight, writeDepth);

        fixed (byte* p = data)
        {
            wgpu.QueueWriteTexture(queue.As<Silk.NET.WebGPU.Queue>(), in destination, p, (nuint)data.Length, in layout, in writeSize);
        }
    }

    public void Dispose()
    {
        wgpu.Dispose();
    }

    public NativeHandle<SamplerTag> DeviceCreateSampler(in NativeHandle<DeviceTag> device, in SamplerDescriptor descriptor, in NativeUtf8 label)
    {
        var native = new Silk.NET.WebGPU.SamplerDescriptor(
            label: label.Pointer,
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
        var sampler = wgpu.DeviceCreateSampler(device.As<Silk.NET.WebGPU.Device>(), in native);
        return new NativeHandle<SamplerTag>((nint)sampler);
    }

    public void SamplerRelease(in NativeHandle<SamplerTag> sampler)
    {
        wgpu.SamplerRelease(sampler.As<Silk.NET.WebGPU.Sampler>());
    }

    public NativeHandle<BindGroupLayoutTag> DeviceCreateBindGroupLayout(in NativeHandle<DeviceTag> device, in BindGroupLayoutDescriptor descriptor, in NativeUtf8 label)
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
                label: label.Pointer,
                entryCount: (uint)entries.Length,
                entries: entriesPtr
            );
            var handle = wgpu.DeviceCreateBindGroupLayout(device.As<Silk.NET.WebGPU.Device>(), in native);
            return new NativeHandle<BindGroupLayoutTag>((nint)handle);
        }
    }

    public void BindGroupLayoutRelease(in NativeHandle<BindGroupLayoutTag> layout)
    {
        wgpu.BindGroupLayoutRelease(layout.As<Silk.NET.WebGPU.BindGroupLayout>());
    }

    public NativeHandle<BindGroupTag> DeviceCreateBindGroup(in NativeHandle<DeviceTag> device, in BindGroupDescriptor descriptor, in NativeUtf8 label)
    {
        var native = new Silk.NET.WebGPU.BindGroupDescriptor(
            label: label.Pointer,
            layout: descriptor.Layout.Native.As<Silk.NET.WebGPU.BindGroupLayout>()
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
                var handle = wgpu.DeviceCreateBindGroup(device.As<Silk.NET.WebGPU.Device>(), in native);
                return new NativeHandle<BindGroupTag>((nint)handle);
            }
        }

        var defaultHandle = wgpu.DeviceCreateBindGroup(device.As<Silk.NET.WebGPU.Device>(), in native);
        return new NativeHandle<BindGroupTag>((nint)defaultHandle);
    }

    public void BindGroupRelease(in NativeHandle<BindGroupTag> bindGroup)
    {
        wgpu.BindGroupRelease(bindGroup.As<Silk.NET.WebGPU.BindGroup>());
    }

    public NativeHandle<CommandEncoderTag> DeviceCreateCommandEncoder(in NativeHandle<DeviceTag> device, in NativeUtf8 label)
    {
        var desc = new Silk.NET.WebGPU.CommandEncoderDescriptor(label: label.Pointer);
        var enc = wgpu.DeviceCreateCommandEncoder(device.As<Silk.NET.WebGPU.Device>(), in desc);
        return new NativeHandle<CommandEncoderTag>((nint)enc);
    }

    public void CommandEncoderRelease(in NativeHandle<CommandEncoderTag> encoder)
    {
        wgpu.CommandEncoderRelease(encoder.As<Silk.NET.WebGPU.CommandEncoder>());
    }

    public NativeHandle<CommandBufferTag> CommandEncoderFinish(in NativeHandle<CommandEncoderTag> encoder, in NativeUtf8 label)
    {
        var desc = new Silk.NET.WebGPU.CommandBufferDescriptor(label: label.Pointer);
        var buffer = wgpu.CommandEncoderFinish(encoder.As<Silk.NET.WebGPU.CommandEncoder>(), in desc);
        return new NativeHandle<CommandBufferTag>((nint)buffer);
    }

    public void CommandBufferRelease(in NativeHandle<CommandBufferTag> buffer)
    {
        wgpu.CommandBufferRelease(buffer.As<Silk.NET.WebGPU.CommandBuffer>());
    }

    public void QueueSubmit(in NativeHandle<QueueTag> queue, ReadOnlySpan<NativeHandle<CommandBufferTag>> commandBuffers)
    {
        if (commandBuffers.Length == 0) return;
        var ptrs = stackalloc Silk.NET.WebGPU.CommandBuffer*[commandBuffers.Length];
        for (int i = 0; i < commandBuffers.Length; i++) ptrs[i] = commandBuffers[i].As<Silk.NET.WebGPU.CommandBuffer>();
        wgpu.QueueSubmit(queue.As<Silk.NET.WebGPU.Queue>(), (nuint)commandBuffers.Length, ptrs);
    }

    public NativeHandle<ShaderModuleTag> DeviceCreateShaderModule(in NativeHandle<DeviceTag> device, ShaderBackend backend, in NativeUtf8 label, in NativeUtf8 code)
    {
        var nativeDescriptor = new Silk.NET.WebGPU.ShaderModuleDescriptor(label: label.Pointer);
        if (backend == ShaderBackend.WGSL)
        {
            var wgsl = new Silk.NET.WebGPU.ShaderModuleWGSLDescriptor(
                chain: new Silk.NET.WebGPU.ChainedStruct(sType: Silk.NET.WebGPU.SType.ShaderModuleWgslDescriptor),
                code: code.Pointer
            );
            nativeDescriptor.NextInChain = (Silk.NET.WebGPU.ChainedStruct*)&wgsl;
        }

        var handle = wgpu.DeviceCreateShaderModule(device.As<Silk.NET.WebGPU.Device>(), in nativeDescriptor);
        return new NativeHandle<ShaderModuleTag>((nint)handle);
    }

    public void ShaderModuleRelease(in NativeHandle<ShaderModuleTag> shaderModule)
    {
        wgpu.ShaderModuleRelease(shaderModule.As<Silk.NET.WebGPU.ShaderModule>());
    }

    public NativeHandle<PipelineLayoutTag> DeviceCreatePipelineLayout(in NativeHandle<DeviceTag> device, in PipelineLayoutDescriptor descriptor, in NativeUtf8 label)
    {
        var layouts = stackalloc Silk.NET.WebGPU.BindGroupLayout*[descriptor.Layouts.Length];
        for (int i = 0; i < descriptor.Layouts.Length; i++) layouts[i] = descriptor.Layouts[i].Native.As<Silk.NET.WebGPU.BindGroupLayout>();
        var native = new Silk.NET.WebGPU.PipelineLayoutDescriptor(
            label: label.Pointer,
            bindGroupLayoutCount: (uint)descriptor.Layouts.Length,
            bindGroupLayouts: layouts
        );
        var handle = wgpu.DeviceCreatePipelineLayout(device.As<Silk.NET.WebGPU.Device>(), in native);
        return new NativeHandle<PipelineLayoutTag>((nint)handle);
    }

    public void PipelineLayoutRelease(in NativeHandle<PipelineLayoutTag> layout)
    {
        wgpu.PipelineLayoutRelease(layout.As<Silk.NET.WebGPU.PipelineLayout>());
    }

    public NativeHandle<ComputePipelineTag> DeviceCreateComputePipeline(in NativeHandle<DeviceTag> device, in ComputePipelineDescriptor descriptor, in NativeUtf8 label)
    {
        var entryPoint = descriptor.Compute.EntryPoint;
        using var entry = new NativeUtf8(entryPoint);
        var native = new Silk.NET.WebGPU.ComputePipelineDescriptor(
            label: label.Pointer,
            layout: descriptor.Layout == null ? null : descriptor.Layout.Native.As<Silk.NET.WebGPU.PipelineLayout>(),
            compute: new(
                module: descriptor.Compute.Shader.Native.As<Silk.NET.WebGPU.ShaderModule>(),
                entryPoint: (byte*)entry.Pointer,
                constantCount: (nuint)descriptor.Compute.Constants.Length,
                constants: descriptor.Compute.Constants.Length == 0 ? null : (Silk.NET.WebGPU.ConstantEntry*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(descriptor.Compute.Constants))
            )
        );
        var handle = wgpu.DeviceCreateComputePipeline(device.As<Silk.NET.WebGPU.Device>(), in native);
        return new NativeHandle<ComputePipelineTag>((nint)handle);
    }

    public void ComputePipelineRelease(in NativeHandle<ComputePipelineTag> pipeline)
    {
        wgpu.ComputePipelineRelease(pipeline.As<Silk.NET.WebGPU.ComputePipeline>());
    }

    public NativeHandle<TextureViewTag> TextureCreateView(in NativeHandle<TextureTag> texture, in TextureViewDescriptor descriptor, in NativeUtf8 label)
    {
        var native = new Silk.NET.WebGPU.TextureViewDescriptor(
            label: label.Pointer,
            format: (Silk.NET.WebGPU.TextureFormat)descriptor.Format,
            dimension: (Silk.NET.WebGPU.TextureViewDimension)descriptor.Dimension,
            baseMipLevel: descriptor.BaseMipLevel,
            mipLevelCount: descriptor.MipLevelCount,
            baseArrayLayer: descriptor.BaseArrayLayer,
            arrayLayerCount: descriptor.ArrayLayerCount,
            aspect: (Silk.NET.WebGPU.TextureAspect?)descriptor.Aspect
        );
        var view = wgpu.TextureCreateView(texture.As<Silk.NET.WebGPU.Texture>(), in native);
        return new NativeHandle<TextureViewTag>((nint)view);
    }

    public void TextureViewRelease(in NativeHandle<TextureViewTag> view)
    {
        wgpu.TextureViewRelease(view.As<Silk.NET.WebGPU.TextureView>());
    }

    public NativeHandle<RenderPipelineTag> DeviceCreateRenderPipeline(in NativeHandle<DeviceTag> device, in RenderPipelineDescriptor descriptor, in NativeUtf8 label)
    {
        using var pins = new Pollus.Utils.TemporaryPins();
        var nativeDescriptor = new Silk.NET.WebGPU.RenderPipelineDescriptor(label: label.Pointer);
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

        var pipeline = wgpu.DeviceCreateRenderPipeline(device.As<Silk.NET.WebGPU.Device>(), in nativeDescriptor);
        return new NativeHandle<RenderPipelineTag>((nint)pipeline);
    }

    public void RenderPipelineRelease(in NativeHandle<RenderPipelineTag> pipeline)
    {
        wgpu.RenderPipelineRelease(pipeline.As<Silk.NET.WebGPU.RenderPipeline>());
    }

    public NativeHandle<RenderPassEncoderTag> CommandEncoderBeginRenderPass(in NativeHandle<CommandEncoderTag> encoder, in RenderPassDescriptor descriptor)
    {
        var colorAttachments = stackalloc Silk.NET.WebGPU.RenderPassColorAttachment[descriptor.ColorAttachments.Length];
        for (int i = 0; i < descriptor.ColorAttachments.Length; i++)
        {
            scoped ref readonly var ca = ref descriptor.ColorAttachments[i];
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
        var handle = wgpu.CommandEncoderBeginRenderPass(encoder.As<Silk.NET.WebGPU.CommandEncoder>(), in rp);
        return new NativeHandle<RenderPassEncoderTag>((nint)handle);
    }

    public void RenderPassEncoderEnd(in NativeHandle<RenderPassEncoderTag> pass)
    {
        wgpu.RenderPassEncoderEnd(pass.As<Silk.NET.WebGPU.RenderPassEncoder>());
        wgpu.RenderPassEncoderRelease(pass.As<Silk.NET.WebGPU.RenderPassEncoder>());
    }

    public void RenderPassEncoderRelease(in NativeHandle<RenderPassEncoderTag> pass)
    {
        wgpu.RenderPassEncoderRelease(pass.As<Silk.NET.WebGPU.RenderPassEncoder>());
    }

    public void RenderPassEncoderSetPipeline(in NativeHandle<RenderPassEncoderTag> pass, in NativeHandle<RenderPipelineTag> pipeline)
    {
        wgpu.RenderPassEncoderSetPipeline(pass.As<Silk.NET.WebGPU.RenderPassEncoder>(), pipeline.As<Silk.NET.WebGPU.RenderPipeline>());
    }

    public void RenderPassEncoderSetViewport(in NativeHandle<RenderPassEncoderTag> pass, float x, float y, float width, float height, float minDepth, float maxDepth)
    {
        wgpu.RenderPassEncoderSetViewport(pass.As<Silk.NET.WebGPU.RenderPassEncoder>(), x, y, width, height, minDepth, maxDepth);
    }

    public void RenderPassEncoderSetScissorRect(in NativeHandle<RenderPassEncoderTag> pass, uint x, uint y, uint width, uint height)
    {
        wgpu.RenderPassEncoderSetScissorRect(pass.As<Silk.NET.WebGPU.RenderPassEncoder>(), x, y, width, height);
    }

    public void RenderPassEncoderSetBlendConstant(in NativeHandle<RenderPassEncoderTag> pass, double r, double g, double b, double a)
    {
        var c = new Silk.NET.WebGPU.Color { R = r, G = g, B = b, A = a };
        wgpu.RenderPassEncoderSetBlendConstant(pass.As<Silk.NET.WebGPU.RenderPassEncoder>(), in c);
    }

    public void RenderPassEncoderSetBindGroup(in NativeHandle<RenderPassEncoderTag> pass, uint groupIndex, in NativeHandle<BindGroupTag> bindGroup, ReadOnlySpan<uint> dynamicOffsets)
    {
        if (dynamicOffsets.Length > 0)
        {
            fixed (uint* p = dynamicOffsets)
            {
                wgpu.RenderPassEncoderSetBindGroup(pass.As<Silk.NET.WebGPU.RenderPassEncoder>(), groupIndex, bindGroup.As<Silk.NET.WebGPU.BindGroup>(), (nuint)dynamicOffsets.Length, p);
            }
        }
        else
        {
            wgpu.RenderPassEncoderSetBindGroup(pass.As<Silk.NET.WebGPU.RenderPassEncoder>(), groupIndex, bindGroup.As<Silk.NET.WebGPU.BindGroup>(), 0, null);
        }
    }

    public void RenderPassEncoderSetVertexBuffer(in NativeHandle<RenderPassEncoderTag> pass, uint slot, in NativeHandle<BufferTag> buffer, ulong offset, ulong size)
    {
        wgpu.RenderPassEncoderSetVertexBuffer(pass.As<Silk.NET.WebGPU.RenderPassEncoder>(), slot, buffer.As<Silk.NET.WebGPU.Buffer>(), offset, size);
    }

    public void RenderPassEncoderSetIndexBuffer(in NativeHandle<RenderPassEncoderTag> pass, in NativeHandle<BufferTag> buffer, IndexFormat format, ulong offset, ulong size)
    {
        wgpu.RenderPassEncoderSetIndexBuffer(pass.As<Silk.NET.WebGPU.RenderPassEncoder>(), buffer.As<Silk.NET.WebGPU.Buffer>(), (Silk.NET.WebGPU.IndexFormat)format, offset, size);
    }

    public void RenderPassEncoderDraw(in NativeHandle<RenderPassEncoderTag> pass, uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance)
    {
        wgpu.RenderPassEncoderDraw(pass.As<Silk.NET.WebGPU.RenderPassEncoder>(), vertexCount, instanceCount, firstVertex, firstInstance);
    }

    public void RenderPassEncoderDrawIndirect(in NativeHandle<RenderPassEncoderTag> pass, in NativeHandle<BufferTag> buffer, uint offset)
    {
        wgpu.RenderPassEncoderDrawIndirect(pass.As<Silk.NET.WebGPU.RenderPassEncoder>(), buffer.As<Silk.NET.WebGPU.Buffer>(), offset);
    }

    public void RenderPassEncoderDrawIndexed(in NativeHandle<RenderPassEncoderTag> pass, uint indexCount, uint instanceCount, uint firstIndex, int baseVertex, uint firstInstance)
    {
        wgpu.RenderPassEncoderDrawIndexed(pass.As<Silk.NET.WebGPU.RenderPassEncoder>(), indexCount, instanceCount, firstIndex, baseVertex, firstInstance);
    }

    public void RenderPassEncoderDrawIndexedIndirect(in NativeHandle<RenderPassEncoderTag> pass, in NativeHandle<BufferTag> buffer, uint offset)
    {
        wgpu.RenderPassEncoderDrawIndexedIndirect(pass.As<Silk.NET.WebGPU.RenderPassEncoder>(), buffer.As<Silk.NET.WebGPU.Buffer>(), offset);
    }

    public void CommandEncoderCopyTextureToTexture(in NativeHandle<CommandEncoderTag> encoder, in NativeHandle<TextureTag> srcTexture, in NativeHandle<TextureTag> dstTexture, uint width, uint height, uint depthOrArrayLayers)
    {
        var size = new Silk.NET.WebGPU.Extent3D(width, height, depthOrArrayLayers);
        var src = new Silk.NET.WebGPU.ImageCopyTexture(texture: srcTexture.As<Silk.NET.WebGPU.Texture>());
        var dst = new Silk.NET.WebGPU.ImageCopyTexture(texture: dstTexture.As<Silk.NET.WebGPU.Texture>());
        wgpu.CommandEncoderCopyTextureToTexture(encoder.As<Silk.NET.WebGPU.CommandEncoder>(), in src, in dst, in size);
    }

    public NativeHandle<ComputePassEncoderTag> CommandEncoderBeginComputePass(in NativeHandle<CommandEncoderTag> encoder, in NativeUtf8 label)
    {
        var desc = new Silk.NET.WebGPU.ComputePassDescriptor(label: label.Pointer);
        var enc = wgpu.CommandEncoderBeginComputePass(encoder.As<Silk.NET.WebGPU.CommandEncoder>(), in desc);
        return new NativeHandle<ComputePassEncoderTag>((nint)enc);
    }

    public void ComputePassEncoderEnd(in NativeHandle<ComputePassEncoderTag> pass)
    {
        wgpu.ComputePassEncoderEnd(pass.As<Silk.NET.WebGPU.ComputePassEncoder>());
        wgpu.ComputePassEncoderRelease(pass.As<Silk.NET.WebGPU.ComputePassEncoder>());
    }

    public void ComputePassEncoderSetPipeline(in NativeHandle<ComputePassEncoderTag> pass, in NativeHandle<ComputePipelineTag> pipeline)
    {
        wgpu.ComputePassEncoderSetPipeline(pass.As<Silk.NET.WebGPU.ComputePassEncoder>(), pipeline.As<Silk.NET.WebGPU.ComputePipeline>());
    }

    public void ComputePassEncoderSetBindGroup(in NativeHandle<ComputePassEncoderTag> pass, uint groupIndex, in NativeHandle<BindGroupTag> bindGroup, ReadOnlySpan<uint> dynamicOffsets)
    {
        if (dynamicOffsets.Length > 0)
        {
            fixed (uint* p = dynamicOffsets)
            {
                wgpu.ComputePassEncoderSetBindGroup(pass.As<Silk.NET.WebGPU.ComputePassEncoder>(), groupIndex, bindGroup.As<Silk.NET.WebGPU.BindGroup>(), (nuint)dynamicOffsets.Length, p);
            }
        }
        else
        {
            wgpu.ComputePassEncoderSetBindGroup(pass.As<Silk.NET.WebGPU.ComputePassEncoder>(), groupIndex, bindGroup.As<Silk.NET.WebGPU.BindGroup>(), 0, null);
        }
    }

    public void ComputePassEncoderDispatchWorkgroups(in NativeHandle<ComputePassEncoderTag> pass, uint x, uint y, uint z)
    {
        wgpu.ComputePassEncoderDispatchWorkgroups(pass.As<Silk.NET.WebGPU.ComputePassEncoder>(), x, y, z);
    }
}


