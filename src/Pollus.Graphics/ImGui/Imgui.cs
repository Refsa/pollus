namespace Pollus.Graphics.Imgui;

using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using ImGuiNET;
using Pollus.Mathematics;
using System.Runtime.CompilerServices;
using Pollus.Debugging;
using Pollus.Utils;
using System.Drawing;

public class ImguiRenderer : IDisposable
{
    const string SHADER =
"""
struct VertexInput {
    @location(0) position: vec2<f32>,
    @location(1) uv: vec2<f32>,
    @location(2) color: vec4<f32>,
};

struct VertexOutput {
    @builtin(position) position: vec4<f32>,
    @location(0) color: vec4<f32>,
    @location(1) uv: vec2<f32>,
};

struct Uniforms {
    mvp: mat4x4<f32>,
    gamma: f32,
};

@group(0) @binding(0) var<uniform> uniforms: Uniforms;
@group(0) @binding(1) var s: sampler;
@group(1) @binding(0) var t: texture_2d<f32>;

@vertex
fn vs_main(in: VertexInput) -> VertexOutput {
    var out: VertexOutput;
    out.position = uniforms.mvp * vec4<f32>(in.position, 0.0, 1.0);
    out.color = in.color;
    out.uv = in.uv;
    return out;
}

@fragment
fn fs_main(in: VertexOutput) -> @location(0) vec4<f32> {
    let color = in.color * textureSample(t, s, in.uv);
    let corrected_color = pow(color.rgb, vec3<f32>(uniforms.gamma));
    return vec4<f32>(corrected_color, color.a);
}
""";


    struct Uniforms
    {
        public System.Numerics.Matrix4x4 MVP;
        public float Gamma;
    }

    IWGPUContext gpuContext;

    GPUBuffer vertexBuffer;
    ImDrawVert[] hostVertexBuffer = new ImDrawVert[1000];
    GPUBuffer indexBuffer;
    ushort[] hostIndexBuffer = new ushort[1000];
    GPUBuffer uniformBuffer;

    GPUTexture fontTexture;
    GPUTextureView? fontTextureView;
    GPUSampler fontSampler;

    GPUBindGroup baseBindGroup;
    GPUBindGroupLayout baseBindGroupLayout;
    GPUBindGroupLayout textureBindGroupLayout;
    GPUBindGroup fontTextureBindGroup;

    GPURenderPipeline renderPipeline;

    Dictionary<GPUTextureView, (nint imguiBindingId, GPUBindGroup bindGroup)> setsByView = [];
    Dictionary<GPUTexture, GPUTextureView> autoViewsByTexture = [];
    Dictionary<nint, (nint imguiBindingId, GPUBindGroup bindGroup)> viewsById = [];

    nint fontAtlasId = 1;
    nint lastAssignedId = 100;

    bool frameBegun = false;
    Vec2<int> size;
    Vec2<int> scaleFactor = Vec2<int>.One;
    TextureFormat targetFormat;

    public ImguiRenderer(IWGPUContext gpuContext, TextureFormat targetFormat, Vec2<int> size)
    {
        this.size = size;
        this.targetFormat = targetFormat;
        this.gpuContext = gpuContext;

        ImGui.CreateContext();
        var io = ImGui.GetIO();
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard | ImGuiConfigFlags.DockingEnable;
        io.Fonts.Flags |= ImFontAtlasFlags.NoBakedLines;

        SetupRenderResources();
        SetPerFrameData(60f.Rcp());

        ImGui.NewFrame();
        frameBegun = true;
    }

    public void Dispose()
    {

    }

    public void Resized(Vec2<int> size)
    {
        this.size = size;
    }

    void SetupRenderResources()
    {
        vertexBuffer = gpuContext.CreateBuffer(BufferDescriptor.Vertex(
            """ImGui Vertex Buffer""",
            Alignment.GPUAlignedSize<ImDrawVert>((uint)hostVertexBuffer.Length, 4)
        ));
        indexBuffer = gpuContext.CreateBuffer(BufferDescriptor.Index(
            """ImGui Index Buffer""",
            Alignment.GPUAlignedSize<ushort>((uint)hostIndexBuffer.Length, 4)
        ));

        fontSampler = gpuContext.CreateSampler(SamplerDescriptor.Default);
        RecreateFontTexture();

        uniformBuffer = gpuContext.CreateBuffer(BufferDescriptor.Uniform<Uniforms>(
            """ImGui Uniform Buffer""",
            Alignment.GPUAlignedSize<Uniforms>(1)
        ));

        using var shader = gpuContext.CreateShaderModule(new()
        {
            Backend = ShaderBackend.WGSL,
            Label = "ImGui Shader",
            Content = SHADER,
        });

        baseBindGroupLayout = gpuContext.CreateBindGroupLayout(new()
        {
            Label = "ImGui Base Bind Group Layout",
            Entries = [
                BindGroupLayoutEntry.Uniform<Uniforms>(0, ShaderStage.Vertex | ShaderStage.Fragment),
                BindGroupLayoutEntry.SamplerEntry(1, ShaderStage.Fragment, SamplerBindingType.Filtering),
            ]
        });
        textureBindGroupLayout = gpuContext.CreateBindGroupLayout(new()
        {
            Label = "ImGui Texture Bind Group Layout",
            Entries = [
                BindGroupLayoutEntry.TextureEntry(0, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D),
            ]
        });

        renderPipeline = gpuContext.CreateRenderPipeline(new()
        {
            Label = """ImGui Render Pipeline""",
            VertexState = new()
            {
                EntryPoint = """vs_main""",
                ShaderModule = shader,
                Layouts = [
                    VertexBufferLayout.Vertex(0, [
                        VertexFormat.Float32x2,
                        VertexFormat.Float32x2,
                        VertexFormat.Unorm8x4,
                    ])
                ]
            },
            FragmentState = new()
            {
                EntryPoint = """fs_main""",
                ShaderModule = shader,
                ColorTargets = [ColorTargetState.Default with { Format = targetFormat }]
            },
            MultisampleState = MultisampleState.Default,
            PrimitiveState = PrimitiveState.Default with
            {
                Topology = Silk.NET.WebGPU.PrimitiveTopology.TriangleList,
                FrontFace = Silk.NET.WebGPU.FrontFace.CW,
                CullMode = Silk.NET.WebGPU.CullMode.None,
            },
            PipelineLayout = gpuContext.CreatePipelineLayout(new()
            {
                Label = """ImGui Pipeline Layout""",
                Layouts = [baseBindGroupLayout, textureBindGroupLayout]
            })
        });

        baseBindGroup = gpuContext.CreateBindGroup(new()
        {
            Label = """ImGui Base Bind Group""",
            Layout = baseBindGroupLayout,
            Entries = [
                BindGroupEntry.BufferEntry<Uniforms>(0, uniformBuffer, 0),
                BindGroupEntry.SamplerEntry(1, fontSampler)
            ]
        });

        fontTextureBindGroup = gpuContext.CreateBindGroup(new()
        {
            Label = """ImGui Texture Bind Group""",
            Layout = textureBindGroupLayout,
            Entries = [
                BindGroupEntry.TextureEntry(0, fontTextureView!.Value)
            ]
        });
    }

    public nint GetOrCreateImguiBinding(GPUTextureView textureView)
    {
        if (!setsByView.TryGetValue(textureView, out var set))
        {
            var bindGroup = gpuContext.CreateBindGroup(new()
            {
                Label = "ImGui Texture Bind Group",
                Layout = textureBindGroupLayout,
                Entries = [
                    BindGroupEntry.TextureEntry(0, textureView)
                ]
            });

            var imguiBindingId = lastAssignedId++;
            set = (imguiBindingId, bindGroup);
            setsByView[textureView] = set;
            viewsById[imguiBindingId] = set;
        }

        return set.imguiBindingId;
    }

    public nint GetOrCreateImguiBinding(GPUTexture texture)
    {
        if (!autoViewsByTexture.TryGetValue(texture, out var view))
        {
            view = texture.GetTextureView();
            autoViewsByTexture[texture] = view;
        }

        return GetOrCreateImguiBinding(view);
    }

    public (nint imguiBindingId, GPUBindGroup bindGroup) GetImguiBinding(nint imguiBindingId)
    {
        return viewsById[imguiBindingId];
    }

    public void ClearCachedImageResources()
    {
        viewsById.Clear();
        setsByView.Clear();
        autoViewsByTexture.Clear();
        lastAssignedId = 100;
    }

    public void RecreateFontTexture()
    {
        fontTexture?.Dispose();
        fontTextureView?.Dispose();

        var io = ImGui.GetIO();

        io.Fonts.GetTexDataAsRGBA32(out nint pixels, out var width, out var height, out var bytesPerPixel);
        io.Fonts.SetTexID(fontAtlasId);

        fontTexture = gpuContext.CreateTexture(TextureDescriptor.D2(
            """ImGui Font Texture""",
            TextureUsage.CopyDst | TextureUsage.TextureBinding,
            TextureFormat.Rgba8Unorm,
            new Vec2<uint>((uint)width, (uint)height)
        ));

        fontTexture.Write(pixels, width * height * bytesPerPixel);
        fontTextureView = fontTexture.GetTextureView();

        io.Fonts.ClearTexData();
    }

    public void Render(GPURenderPassEncoder encoder)
    {
        if (frameBegun)
        {
            frameBegun = false;
            ImGui.Render();
            RenderImDrawData(ImGui.GetDrawData(), encoder);
        }
    }

    public void Update(float deltaTime)
    {
        if (frameBegun)
        {
            ImGui.Render();
        }

        SetPerFrameData(deltaTime);

        frameBegun = true;
        ImGui.NewFrame();
    }

    void SetPerFrameData(float deltaTime)
    {
        var io = ImGui.GetIO();

        io.DisplaySize = new System.Numerics.Vector2(size.X / scaleFactor.X, size.Y / scaleFactor.Y);
        io.DisplayFramebufferScale = new System.Numerics.Vector2(scaleFactor.X, scaleFactor.Y);
        io.DeltaTime = deltaTime;
    }

    unsafe void RenderImDrawData(ImDrawDataPtr drawData, GPURenderPassEncoder encoder)
    {
        if (drawData.CmdListsCount == 0)
        {
            return;
        }

        if (drawData.TotalVtxCount >= hostVertexBuffer.Length)
        {
            uint totalVtxSize = (uint)(drawData.TotalVtxCount * Unsafe.SizeOf<ImDrawVert>());
            vertexBuffer.Dispose();
            vertexBuffer = gpuContext.CreateBuffer(BufferDescriptor.Vertex(
                """ImGui Vertex Buffer""",
                Alignment.GPUAlignedSize<ImDrawVert>(totalVtxSize, 4)
            ));
            hostVertexBuffer = new ImDrawVert[drawData.TotalVtxCount];
        }

        if (drawData.TotalIdxCount >= hostIndexBuffer.Length)
        {
            uint totalIdxSize = (uint)(drawData.TotalIdxCount * Unsafe.SizeOf<ushort>());
            indexBuffer.Dispose();
            indexBuffer = gpuContext.CreateBuffer(BufferDescriptor.Index(
                """ImGui Index Buffer""",
                Alignment.GPUAlignedSize<ushort>(totalIdxSize, 4)
            ));
            hostIndexBuffer = new ushort[drawData.TotalIdxCount];
        }

        uint vtxOffset = 0, idxOffset = 0;
        for (int i = 0; i < drawData.CmdListsCount; i++)
        {
            var cmdList = drawData.CmdLists[i];
            var vtxSpan = new Span<ImDrawVert>((void*)cmdList.VtxBuffer.Data, cmdList.VtxBuffer.Size);
            var idxSpan = new Span<ushort>((void*)cmdList.IdxBuffer.Data, cmdList.IdxBuffer.Size);

            vtxSpan.CopyTo(hostVertexBuffer.AsSpan((int)vtxOffset));
            idxSpan.CopyTo(hostIndexBuffer.AsSpan((int)idxOffset));

            vtxOffset += (uint)vtxSpan.Length;
            idxOffset += (uint)idxSpan.Length;
        }
        vertexBuffer.Write<ImDrawVert>(hostVertexBuffer.AsSpan()[..drawData.TotalVtxCount]);
        indexBuffer.Write<ushort>(hostIndexBuffer.AsSpan()[..drawData.TotalIdxCount]);

        var io = ImGui.GetIO();
        var uniform = new Uniforms
        {
            MVP = System.Numerics.Matrix4x4.CreateOrthographicOffCenter(
                0f, io.DisplaySize.X, io.DisplaySize.Y,
                0.0f, -1.0f, 1.0f
            ),
            Gamma = targetFormat switch
            {
                TextureFormat.Rgba8UnormSrgb => 2.2f,
                _ => 1.0f
            }
        };
        uniformBuffer.Write(uniform, 0);

        encoder.SetViewport(
            Vec2f.Zero,
            new Vec2f(drawData.FramebufferScale.X * drawData.DisplaySize.X, drawData.FramebufferScale.Y * drawData.DisplaySize.Y),
            0f, 1f
        );
        encoder.SetBlendConstant(Vec4<double>.Zero);
        encoder.SetPipeline(renderPipeline);
        encoder.SetVertexBuffer(0, vertexBuffer);
        encoder.SetIndexBuffer(indexBuffer, IndexFormat.Uint16);
        encoder.SetBindGroup(baseBindGroup, 0);

        drawData.ScaleClipRects(io.DisplayFramebufferScale);

        vtxOffset = 0; idxOffset = 0;
        for (int i = 0; i < drawData.CmdListsCount; i++)
        {
            var cmdList = drawData.CmdLists[i];
            for (int c = 0; c < cmdList.CmdBuffer.Size; c++)
            {
                var pcmd = cmdList.CmdBuffer[c];

                if (pcmd.UserCallback != nint.Zero)
                {
                    throw new NotImplementedException("User Callbacks are not supported");
                }

                if (pcmd.TextureId != nint.Zero)
                {
                    encoder.SetBindGroup(
                        pcmd.TextureId switch
                        {
                            var id when id == fontAtlasId => fontTextureBindGroup,
                            var id => GetImguiBinding(id).bindGroup
                        }, 1
                    );
                }
                
                encoder.SetScissorRect(
                    (uint)pcmd.ClipRect.X, (uint)pcmd.ClipRect.Y,
                    (uint)(pcmd.ClipRect.Z - pcmd.ClipRect.X), (uint)(pcmd.ClipRect.W - pcmd.ClipRect.Y)
                );
                encoder.DrawIndexed(
                    pcmd.ElemCount, 1, pcmd.IdxOffset + idxOffset, (int)(pcmd.VtxOffset + vtxOffset), 0
                );
            }

            vtxOffset += (uint)cmdList.VtxBuffer.Size;
            idxOffset += (uint)cmdList.IdxBuffer.Size;
        }
    }
}