namespace Pollus.Graphics.Rendering;

using Pollus.Graphics.WGPU;
using Pollus.Utils;

unsafe public class GPURenderPipeline : GPUResourceWrapper
{
    Silk.NET.WebGPU.RenderPipeline* native;

    public nint Native => (nint)native;

    public GPURenderPipeline(IWGPUContext context, RenderPipelineDescriptor descriptor) : base(context)
    {
        this.context = context;
        using var pins = new TemporaryPins();

        var nativeDescriptor = new Silk.NET.WebGPU.RenderPipelineDescriptor(
            label: (byte*)pins.PinString(descriptor.Label).AddrOfPinnedObject()
        );

        if (descriptor.VertexState is VertexState vertexState)
        {
            pins.Pin(vertexState.EntryPoint);
            nativeDescriptor.Vertex = new Silk.NET.WebGPU.VertexState(
                module: (Silk.NET.WebGPU.ShaderModule*)vertexState.ShaderModule.Native,
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
                        stepMode: vertexBufferLayout.StepMode,
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
                module: (Silk.NET.WebGPU.ShaderModule*)fragmentState.ShaderModule.Native,
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

        descriptor.MultisampleState ??= MultisampleState.Default;
        if (descriptor.MultisampleState is MultisampleState multisampleState)
        {
            nativeDescriptor.Multisample = new Silk.NET.WebGPU.MultisampleState(
                count: multisampleState.Count,
                mask: multisampleState.Mask,
                alphaToCoverageEnabled: multisampleState.AlphaToCoverageEnabled
            );
        }

        descriptor.PrimitiveState ??= PrimitiveState.Default;
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
            nativeDescriptor.Layout = (Silk.NET.WebGPU.PipelineLayout*)pipelineLayout.Native;
        }

        native = context.wgpu.DeviceCreateRenderPipeline(context.Device, nativeDescriptor);
    }

    protected override void Free()
    {
        context.wgpu.RenderPipelineRelease(native);
    }
}