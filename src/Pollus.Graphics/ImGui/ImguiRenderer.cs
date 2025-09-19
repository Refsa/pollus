#pragma warning disable CS8618

namespace Pollus.Graphics.Imgui;

using System.Runtime.CompilerServices;
using ImGuiNET;
using Pollus.Debugging;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Mathematics;
using Pollus.Utils;

[ShaderType]
partial struct Uniforms
{
    public System.Numerics.Matrix4x4 MVP;
    public float Gamma;
}

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

    IWGPUContext gpuContext;
    IRenderAssets renderAssets;

    Handle<GPUBuffer> vertexBuffer;
    ImDrawVert[] hostVertexBuffer = new ImDrawVert[1000];
    Handle<GPUBuffer> indexBuffer;
    ushort[] hostIndexBuffer = new ushort[1000];
    Handle<GPUBuffer> uniformBuffer;

    Handle<GPUTexture> fontTexture = Handle<GPUTexture>.Null;
    Handle<GPUTextureView> fontTextureView = Handle<GPUTextureView>.Null;
    Handle<GPUSampler> fontSampler;

    Handle<GPUBindGroup> baseBindGroup;
    Handle<GPUBindGroupLayout> baseBindGroupLayout;
    Handle<GPUBindGroupLayout> textureBindGroupLayout;
    Handle<GPUBindGroup> fontTextureBindGroup;

    Handle<GPURenderPipeline> renderPipeline;

    Dictionary<Handle<GPUTextureView>, (nint imguiBindingId, Handle<GPUBindGroup> bindGroup)> setsByView = [];
    Dictionary<Handle<GPUTexture>, Handle<GPUTextureView>> autoViewsByTexture = [];
    Dictionary<nint, (nint imguiBindingId, Handle<GPUBindGroup> bindGroup)> viewsById = [];

    nint fontAtlasId = 1;
    nint lastAssignedId = 100;

    bool frameBegun = false;
    Vec2<uint> size;
    Vec2<int> scaleFactor = Vec2<int>.One;
    TextureFormat targetFormat;

    public ImguiRenderer(IWGPUContext gpuContext, IRenderAssets renderAssets, TextureFormat targetFormat, Vec2<uint> size)
    {
        this.size = size;
        this.targetFormat = targetFormat;
        this.gpuContext = gpuContext;
        this.renderAssets = renderAssets;

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
        GC.SuppressFinalize(this);
        ImGui.DestroyContext();

        renderAssets.Unload(renderPipeline);
        renderAssets.Unload(baseBindGroup);
        renderAssets.Unload(baseBindGroupLayout);
        renderAssets.Unload(textureBindGroupLayout);
        renderAssets.Unload(fontTextureBindGroup);
        renderAssets.Unload(fontSampler);
        if (fontTexture != Handle<GPUTexture>.Null)
        {
            renderAssets.Unload(fontTexture);
            renderAssets.Unload(fontTextureView);
        }
        renderAssets.Unload(uniformBuffer);
        renderAssets.Unload(indexBuffer);
        renderAssets.Unload(vertexBuffer);

        ClearCachedImageResources();
    }

    public void ClearCachedImageResources()
    {
        foreach (var (_, (_, bindGroup)) in setsByView)
        {
            renderAssets.Unload(bindGroup);
        }

        foreach (var (_, view) in autoViewsByTexture)
        {
            renderAssets.Unload(view);
        }

        viewsById.Clear();
        setsByView.Clear();
        autoViewsByTexture.Clear();
        lastAssignedId = 100;
    }

    public void Resized(Vec2<uint> size)
    {
        this.size = size;
    }

    void SetupRenderResources()
    {
        var vertexBufferValue = gpuContext.CreateBuffer(BufferDescriptor.Vertex(
            """ImGui Vertex Buffer""",
            Alignment.AlignedSize<ImDrawVert>((uint)hostVertexBuffer.Length, 4)
        ));
        vertexBuffer = renderAssets.Add(vertexBufferValue);
        var indexBufferValue = gpuContext.CreateBuffer(BufferDescriptor.Index(
            """ImGui Index Buffer""",
            Alignment.AlignedSize<ushort>((uint)hostIndexBuffer.Length, 4)
        ));
        indexBuffer = renderAssets.Add(indexBufferValue);

        var fontSamplerValue = gpuContext.CreateSampler(SamplerDescriptor.Default);
        fontSampler = renderAssets.Add(fontSamplerValue);
        RecreateFontTexture();

        var uniformBufferValue = gpuContext.CreateBuffer(BufferDescriptor.Uniform<Uniforms>(
            """ImGui Uniform Buffer""",
            Alignment.AlignedSize<Uniforms>(1)
        ));
        uniformBuffer = renderAssets.Add(uniformBufferValue);

        using var shader = gpuContext.CreateShaderModule(new()
        {
            Backend = ShaderBackend.WGSL,
            Label = "ImGui Shader",
            Content = SHADER,
        });

        var baseBindGroupLayoutValue = gpuContext.CreateBindGroupLayout(new()
        {
            Label = "ImGui Base Bind Group Layout",
            Entries = [
                BindGroupLayoutEntry.Uniform<Uniforms>(0, ShaderStage.Vertex | ShaderStage.Fragment),
                BindGroupLayoutEntry.SamplerEntry(1, ShaderStage.Fragment, SamplerBindingType.Filtering),
            ]
        });
        baseBindGroupLayout = renderAssets.Add(baseBindGroupLayoutValue);

        var textureBindGroupLayoutValue = gpuContext.CreateBindGroupLayout(new()
        {
            Label = "ImGui Texture Bind Group Layout",
            Entries = [
                BindGroupLayoutEntry.TextureEntry(0, ShaderStage.Fragment, TextureSampleType.Float, TextureViewDimension.Dimension2D),
            ]
        });
        textureBindGroupLayout = renderAssets.Add(textureBindGroupLayoutValue);

        renderPipeline = renderAssets.Add(gpuContext.CreateRenderPipeline(new()
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
                ColorTargets = [ColorTargetState.Default with {
                    Format = targetFormat,
                    Blend = BlendState.Default with
                    {
                        Alpha = new()
                        {
                            Operation = BlendOperation.Add,
                            SrcFactor = BlendFactor.SrcAlpha,
                            DstFactor = BlendFactor.OneMinusSrcAlpha,
                        },
                        Color = new()
                        {
                            Operation = BlendOperation.Add,
                            SrcFactor = BlendFactor.SrcAlpha,
                            DstFactor = BlendFactor.OneMinusSrcAlpha,
                        },
                    },
                }]
            },
            MultisampleState = MultisampleState.Default,
            PrimitiveState = PrimitiveState.Default with
            {
                Topology = PrimitiveTopology.TriangleList,
                FrontFace = FrontFace.CW,
                CullMode = CullMode.None,
            },
            PipelineLayout = gpuContext.CreatePipelineLayout(new()
            {
                Label = """ImGui Pipeline Layout""",
                Layouts = [baseBindGroupLayoutValue, textureBindGroupLayoutValue]
            })
        }));

        baseBindGroup = renderAssets.Add(gpuContext.CreateBindGroup(new()
        {
            Label = """ImGui Base Bind Group""",
            Layout = baseBindGroupLayoutValue,
            Entries = [
                BindGroupEntry.BufferEntry<Uniforms>(0, uniformBufferValue, 0),
                BindGroupEntry.SamplerEntry(1, fontSamplerValue)
            ]
        }));

        var fontTextureViewValue = renderAssets.Get(fontTextureView);
        fontTextureBindGroup = renderAssets.Add(gpuContext.CreateBindGroup(new()
        {
            Label = """ImGui Texture Bind Group""",
            Layout = textureBindGroupLayoutValue,
            Entries = [
                BindGroupEntry.TextureEntry(0, fontTextureViewValue)
            ]
        }));
    }

    public nint GetOrCreateImguiBinding(Handle<GPUTextureView> textureView)
    {
        if (!setsByView.TryGetValue(textureView, out var set))
        {
            var textureBindGroupLayoutValue = renderAssets.Get(textureBindGroupLayout);
            var textureViewValue = renderAssets.Get(textureView);

            var bindGroup = renderAssets.Add(gpuContext.CreateBindGroup(new()
            {
                Label = "ImGui Texture Bind Group",
                Layout = textureBindGroupLayoutValue,
                Entries = [
                    BindGroupEntry.TextureEntry(0, textureViewValue)
                ]
            }));

            var imguiBindingId = lastAssignedId++;
            set = (imguiBindingId, bindGroup);
            setsByView[textureView] = set;
            viewsById[imguiBindingId] = set;
        }

        return set.imguiBindingId;
    }

    public nint GetOrCreateImguiBinding(Handle<GPUTexture> texture)
    {
        if (!autoViewsByTexture.TryGetValue(texture, out var view))
        {
            var textureValue = renderAssets.Get(texture);
            view = renderAssets.Add(textureValue.GetTextureView());
            autoViewsByTexture[texture] = view;
        }

        return GetOrCreateImguiBinding(view);
    }

    public bool TryGetImguiBinding(nint imguiBindingId, out (nint imguiBindingId, Handle<GPUBindGroup> bindGroup) binding)
    {
        return viewsById.TryGetValue(imguiBindingId, out binding);
    }

    public (nint imguiBindingId, Handle<GPUBindGroup> bindGroup) GetImguiBinding(nint imguiBindingId)
    {
        return viewsById[imguiBindingId];
    }

    public void RecreateFontTexture()
    {
        if (fontTexture != Handle<GPUTexture>.Null)
        {
            renderAssets.Unload(fontTexture);
            renderAssets.Unload(fontTextureView);
        }

        var io = ImGui.GetIO();

        io.Fonts.GetTexDataAsRGBA32(out nint pixels, out var width, out var height, out var bytesPerPixel);
        io.Fonts.SetTexID(fontAtlasId);

        var fontTextureValue = gpuContext.CreateTexture(TextureDescriptor.D2(
            """ImGui Font Texture""",
            TextureUsage.CopyDst | TextureUsage.TextureBinding,
            TextureFormat.Rgba8Unorm,
            new Vec2<uint>((uint)width, (uint)height)
        ));
        fontTexture = renderAssets.Add(fontTextureValue);

        fontTextureValue.Write(pixels, width * height * bytesPerPixel);
        fontTextureView = renderAssets.Add(fontTextureValue.GetTextureView());

        io.Fonts.ClearTexData();
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

    public void Render(ref RenderCommands commands)
    {
        if (frameBegun)
        {
            frameBegun = false;
            ImGui.Render();
            RenderImDrawData(ImGui.GetDrawData(), ref commands);
        }
    }

    unsafe void RenderImDrawData(ImDrawDataPtr drawData, ref RenderCommands commands)
    {
        if (drawData.CmdListsCount == 0)
        {
            return;
        }

        if (drawData.TotalVtxCount > hostVertexBuffer.Length)
        {
            renderAssets.Unload(vertexBuffer);

            uint totalVtxSize = (uint)(drawData.TotalVtxCount * Unsafe.SizeOf<ImDrawVert>());
            vertexBuffer = renderAssets.Add(gpuContext.CreateBuffer(BufferDescriptor.Vertex(
                """ImGui Vertex Buffer""",
                Alignment.AlignedSize<ImDrawVert>(totalVtxSize, 4)
            )));
            hostVertexBuffer = new ImDrawVert[drawData.TotalVtxCount];
        }

        if (drawData.TotalIdxCount > hostIndexBuffer.Length)
        {
            renderAssets.Unload(indexBuffer);

            uint totalIdxSize = (uint)(drawData.TotalIdxCount * Unsafe.SizeOf<ushort>());
            indexBuffer = renderAssets.Add(gpuContext.CreateBuffer(BufferDescriptor.Index(
                """ImGui Index Buffer""",
                Alignment.AlignedSize<ushort>(totalIdxSize, 4)
            )));
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
        {
            var vertexBufferValue = renderAssets.Get(vertexBuffer);
            vertexBufferValue.Write<ImDrawVert>(hostVertexBuffer.AsSpan()[..drawData.TotalVtxCount], Alignment.AlignedSize<ImDrawVert>((uint)drawData.TotalVtxCount, 4));
            var indexBufferValue = renderAssets.Get(indexBuffer);
            indexBufferValue.Write<ushort>(hostIndexBuffer.AsSpan()[..drawData.TotalIdxCount], Alignment.AlignedSize<ushort>((uint)drawData.TotalIdxCount, 4));
        }

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
        {
            var uniformBufferValue = renderAssets.Get(uniformBuffer);
            uniformBufferValue.Write(uniform, 0);
        }

        commands.SetViewport(
            Vec2f.Zero,
            new Vec2f(drawData.FramebufferScale.X * drawData.DisplaySize.X, drawData.FramebufferScale.Y * drawData.DisplaySize.Y),
            0f, 1f
        );
        commands.SetBlendConstant(Vec4<double>.Zero);
        commands.SetPipeline(renderPipeline);
        commands.SetVertexBuffer(0, vertexBuffer);
        commands.SetIndexBuffer(indexBuffer, IndexFormat.Uint16);
        commands.SetBindGroup(0, baseBindGroup);

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
                    if (pcmd.TextureId == fontAtlasId)
                    {
                        commands.SetBindGroup(1, fontTextureBindGroup);
                    }
                    else
                    {
                        if (!TryGetImguiBinding(pcmd.TextureId, out var binding) || binding.bindGroup.IsNull())
                        {
                            Log.Info($"ImGui Texture Bind Group for texture {pcmd.TextureId} not found");
                            continue;
                        }
                        commands.SetBindGroup(1, binding.bindGroup);
                    }
                }

                commands.SetScissorRect(
                    (uint)pcmd.ClipRect.X, (uint)pcmd.ClipRect.Y,
                    (uint)(pcmd.ClipRect.Z - pcmd.ClipRect.X), (uint)(pcmd.ClipRect.W - pcmd.ClipRect.Y)
                );
                commands.DrawIndexed(
                    pcmd.ElemCount, 1, pcmd.IdxOffset + idxOffset, (int)(pcmd.VtxOffset + vtxOffset), 0
                );
            }

            vtxOffset += (uint)cmdList.VtxBuffer.Size;
            idxOffset += (uint)cmdList.IdxBuffer.Size;
        }
    }
}

#pragma warning restore CS8618