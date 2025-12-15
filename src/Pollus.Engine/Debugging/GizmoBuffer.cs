namespace Pollus.Debugging;

using Pollus.Engine.Rendering;
using Pollus.Graphics;
using Pollus.Graphics.Rendering;
using Pollus.Graphics.WGPU;
using Pollus.Utils;

record struct SortKey : IComparable<SortKey>
{
    public required float SortOrder;
    public required uint DrawIndex;
    public required GizmoMode Mode;
    public required GizmoType Type;
    public required Handle<Texture2D>? Texture;

    public int CompareTo(SortKey other)
    {
        var result = SortOrder.CompareTo(other.SortOrder);
        if (result == 0) result = Mode.CompareTo(other.Mode);
        if (result == 0) result = DrawIndex.CompareTo(other.DrawIndex);
        if (result == 0 && Texture is { } texture && other.Texture is { } otherTexture) result = texture.ID.CompareTo(otherTexture.ID);
        return result;
    }
}

record struct GizmoTexture
{
    public record struct Key
    {
        public required GizmoType Type;
        public required Handle<Texture2D> Texture;

        public int ID => Texture.ID;
    }

    public required Key TextureKey;
    public Handle<GPUBindGroup>? BindGroup;
}

class GizmoMaterialData
{
    public Handle<GPURenderPipeline> PipelineHandle = Handle<GPURenderPipeline>.Null;
    public Handle<GPUBindGroupLayout> BindGroupLayoutHandle = Handle<GPUBindGroupLayout>.Null;

    public void Cleanup(IRenderAssets renderAssets)
    {
        if (PipelineHandle != Handle<GPURenderPipeline>.Null) renderAssets.Unload(PipelineHandle);
        if (BindGroupLayoutHandle != Handle<GPUBindGroupLayout>.Null) renderAssets.Unload(BindGroupLayoutHandle);
    }
}

public class GizmoBuffer
{
    Handle<GPUBuffer> drawBufferHandle = Handle<GPUBuffer>.Null;
    Handle<GPUBuffer> vertexBufferHandle = Handle<GPUBuffer>.Null;

    Handle<GPURenderPipeline> outlinedPipelineHandle = Handle<GPURenderPipeline>.Null;
    Handle<GPURenderPipeline> filledPipelineHandle = Handle<GPURenderPipeline>.Null;
    Handle<GPUBindGroup> bindGroupHandle = Handle<GPUBindGroup>.Null;

    Dictionary<GizmoTexture.Key, GizmoTexture> textures = new();
    Handle<GPUSampler> samplerHandle = Handle<GPUSampler>.Null;
    GizmoMaterialData fontMaterial = new();
    GizmoMaterialData textureMaterial = new();

    int drawCount;
    int vertexCount;

    SortKey[] drawOrder = new SortKey[1024];
    IndirectBufferData[] draws = new IndirectBufferData[1024];
    GizmoVertex[] vertices = new GizmoVertex[1024];

    bool isSetup = false;

    public int VertexCount => vertexCount;
    public int DrawCount => drawCount;
    public bool IsSetup => isSetup;

    public void Cleanup(IRenderAssets renderAssets)
    {
        if (isSetup is false) return;

        foreach (var texture in textures.Values)
        {
            if (texture.BindGroup.HasValue) renderAssets.Unload(texture.BindGroup.Value);
        }

        textures.Clear();

        if (samplerHandle != Handle<GPUSampler>.Null) renderAssets.Unload(samplerHandle);
        if (bindGroupHandle != Handle<GPUBindGroup>.Null) renderAssets.Unload(bindGroupHandle);
        if (drawBufferHandle != Handle<GPUBuffer>.Null) renderAssets.Unload(drawBufferHandle);
        if (vertexBufferHandle != Handle<GPUBuffer>.Null) renderAssets.Unload(vertexBufferHandle);
        if (outlinedPipelineHandle != Handle<GPURenderPipeline>.Null) renderAssets.Unload(outlinedPipelineHandle);
        if (filledPipelineHandle != Handle<GPURenderPipeline>.Null) renderAssets.Unload(filledPipelineHandle);

        textureMaterial.Cleanup(renderAssets);
        fontMaterial.Cleanup(renderAssets);

        isSetup = false;
    }

    public void Setup(IWGPUContext gpuContext, RenderAssets renderAssets)
    {
        if (isSetup) return;

        var filledMaterial = renderAssets.Get<MaterialRenderData>(new Handle<GizmoFilledMaterial>(0));
        var outlinedMaterial = renderAssets.Get<MaterialRenderData>(new Handle<GizmoOutlinedMaterial>(0));

        outlinedPipelineHandle = outlinedMaterial.Pipeline;
        filledPipelineHandle = filledMaterial.Pipeline;
        bindGroupHandle = filledMaterial.BindGroups[0];

        samplerHandle = renderAssets.Add(gpuContext.CreateSampler(SamplerDescriptor.Default));
        SetupFont(gpuContext, renderAssets);
        SetupTexture(gpuContext, renderAssets);

        drawBufferHandle = renderAssets.Add(gpuContext.CreateBuffer(
            BufferDescriptor.Indirect("gizmo::drawBuffer", (uint)drawCount)
        ));

        vertexBufferHandle = renderAssets.Add(gpuContext.CreateBuffer(
            BufferDescriptor.Vertex<GizmoVertex>("gizmo::vertexBuffer", (uint)vertexCount)
        ));

        isSetup = true;
    }

    private void SetupFont(IWGPUContext gpuContext, RenderAssets renderAssets)
    {
        var shaderModule = gpuContext.CreateShaderModule(new()
        {
            Label = """gizmo-font-shader""",
            Backend = ShaderBackend.WGSL,
            Content = GizmoShaders.GIZMO_FONT_SHADER,
        });
        renderAssets.Add(shaderModule);

        var bindGroupLayout = gpuContext.CreateBindGroupLayout(new()
        {
            Label = """gizmo::fontBindGroupLayout""",
            Entries = GizmoTextureMaterial.BindGroupLayoutEntries,
        });
        var bindGroupLayoutHandle = renderAssets.Add(bindGroupLayout);

        var pipelineLayout = gpuContext.CreatePipelineLayout(new()
        {
            Label = """gizmo::fontPipelineLayout""",
            Layouts = [bindGroupLayout]
        });
        renderAssets.Add(pipelineLayout);

        var texturedPipeline = gpuContext.CreateRenderPipeline(GizmoTextureMaterial.PipelineDescriptor(gpuContext, shaderModule, pipelineLayout));
        var pipelineHandle = renderAssets.Add(texturedPipeline);

        fontMaterial = new()
        {
            PipelineHandle = pipelineHandle,
            BindGroupLayoutHandle = bindGroupLayoutHandle,
        };
    }

    private void SetupTexture(IWGPUContext gpuContext, RenderAssets renderAssets)
    {
        var shaderModule = gpuContext.CreateShaderModule(new()
        {
            Label = """gizmo-texture-shader""",
            Backend = ShaderBackend.WGSL,
            Content = GizmoShaders.GIZMO_TEXTURE_SHADER,
        });
        renderAssets.Add(shaderModule);

        var bindGroupLayout = gpuContext.CreateBindGroupLayout(new()
        {
            Label = """gizmo::textureBindGroupLayout""",
            Entries = GizmoTextureMaterial.BindGroupLayoutEntries,
        });
        var bindGroupLayoutHandle = renderAssets.Add(bindGroupLayout);

        var pipelineLayout = gpuContext.CreatePipelineLayout(new()
        {
            Label = """gizmo::texturePipelineLayout""",
            Layouts = [bindGroupLayout]
        });
        renderAssets.Add(pipelineLayout);

        var texturedPipeline = gpuContext.CreateRenderPipeline(GizmoTextureMaterial.PipelineDescriptor(gpuContext, shaderModule, pipelineLayout));
        var pipelineHandle = renderAssets.Add(texturedPipeline);

        textureMaterial = new()
        {
            PipelineHandle = pipelineHandle,
            BindGroupLayoutHandle = bindGroupLayoutHandle,
        };
    }

    public void AddVertex(in GizmoVertex vertex)
    {
        if (vertexCount >= vertices.Length) Array.Resize(ref vertices, vertexCount * 2);
        vertices[vertexCount++] = vertex;
    }

    public void AddDraw(in ReadOnlySpan<GizmoVertex> drawVertices, GizmoType type, GizmoMode mode, float sortOrder, Handle<Texture2D>? texture = null)
    {
        if (vertexCount + drawVertices.Length > vertices.Length) Array.Resize(ref vertices, vertexCount + drawVertices.Length);
        drawVertices.CopyTo(vertices.AsSpan(vertexCount, drawVertices.Length));
        vertexCount += drawVertices.Length;

        if (drawCount >= draws.Length)
        {
            Array.Resize(ref draws, drawCount * 2);
            Array.Resize(ref drawOrder, drawCount * 2);
        }

        var index = drawCount++;

        ref var drawTarget = ref draws[index];
        drawTarget.FirstInstance = 0;
        drawTarget.InstanceCount = 1;
        drawTarget.FirstVertex = (uint)(vertexCount - drawVertices.Length);
        drawTarget.VertexCount = (uint)drawVertices.Length;

        ref var drawOrderTarget = ref drawOrder[index];
        drawOrderTarget.SortOrder = sortOrder;
        drawOrderTarget.DrawIndex = (uint)index;
        drawOrderTarget.Mode = mode;
        drawOrderTarget.Type = type;
        drawOrderTarget.Texture = texture;

        if (texture.HasValue)
        {
            var key = new GizmoTexture.Key { Type = type, Texture = texture.Value };
            textures.TryAdd(key, new GizmoTexture { TextureKey = key });
        }
    }

    public void PrepareFrame(IWGPUContext gpuContext, RenderAssets renderAssets)
    {
        var drawBuffer = renderAssets.Get(drawBufferHandle);
        var vertexBuffer = renderAssets.Get(vertexBufferHandle);

        drawOrder.AsSpan(0, drawCount).Sort(static (a, b) => a.CompareTo(b));

        drawBuffer.Resize<IndirectBufferData>((uint)drawCount);
        drawBuffer.Write(draws.AsSpan(0, drawCount));

        vertexBuffer.Resize<GizmoVertex>((uint)vertexCount);
        vertexBuffer.Write(vertices.AsSpan(0, vertexCount));

        var sceneUniformRenderData = renderAssets.Get<UniformRenderData>(new Handle<Uniform<SceneUniform>>(0));
        var sceneUniformBuffer = renderAssets.Get(sceneUniformRenderData.UniformBuffer);

        foreach (var texture in textures.Values)
        {
            if (texture.BindGroup.HasValue) continue;

            var textureRenderData = renderAssets.Get<TextureRenderData>(texture.TextureKey.Texture);
            var textureView = renderAssets.Get(textureRenderData.View);
            var sampler = renderAssets.Get(samplerHandle);

            var bindGroupLayoutHandle = texture.TextureKey.Type switch
            {
                GizmoType.Text => fontMaterial.BindGroupLayoutHandle,
                GizmoType.Texture => textureMaterial.BindGroupLayoutHandle,
                _ => throw new InvalidOperationException("Invalid gizmo type"),
            };

            var bindGroup = gpuContext.CreateBindGroup(new()
            {
                Label = $"""gizmo::textureBindGroup_{texture.TextureKey.ID}""",
                Layout = renderAssets.Get(bindGroupLayoutHandle),
                Entries =
                [
                    BindGroupEntry.BufferEntry<SceneUniform>(0, sceneUniformBuffer, 0),
                    BindGroupEntry.TextureEntry(1, textureView),
                    BindGroupEntry.SamplerEntry(2, sampler),
                ],
            });
            textures[texture.TextureKey] = textures[texture.TextureKey] with
            {
                BindGroup = renderAssets.Add(bindGroup),
            };
        }
    }

    public void DrawFrame(CommandList commandList)
    {
        var commands = RenderCommands.Builder
            .SetBindGroup(0, bindGroupHandle)
            .SetVertexBuffer(0, vertexBufferHandle, 0, Alignment.AlignedSize<GizmoVertex>((uint)vertexCount));

        GizmoMode? prevMode = null;
        Handle<Texture2D>? prevTexture = null;

        for (uint i = 0; i < drawCount; i++)
        {
            scoped ref readonly var sortKey = ref drawOrder[i];
            if (sortKey.Mode != prevMode)
            {
                commands.SetPipeline(sortKey.Mode switch
                {
                    GizmoMode.Filled => filledPipelineHandle,
                    GizmoMode.Outlined => outlinedPipelineHandle,
                    GizmoMode.Font => fontMaterial.PipelineHandle,
                    GizmoMode.Texture => textureMaterial.PipelineHandle,
                    _ => throw new InvalidOperationException("Invalid gizmo mode"),
                });
                prevMode = sortKey.Mode;
            }

            if (sortKey.Texture != prevTexture)
            {
                if (sortKey is { Mode: GizmoMode.Texture or GizmoMode.Font, Texture: { } textureHandle }
                    && textures[new() { Type = sortKey.Type, Texture = textureHandle }] is { BindGroup: { } texturedBindGroupHandle })
                {
                    commands.SetBindGroup(0, texturedBindGroupHandle);
                }
                else
                {
                    commands.SetBindGroup(0, bindGroupHandle);
                }
            }

            commands.DrawIndirect(drawBufferHandle, sortKey.DrawIndex * IndirectBufferData.SizeOf);
        }

        commandList.Add(commands);
    }

    public void Clear()
    {
        drawCount = 0;
        vertexCount = 0;
    }
}