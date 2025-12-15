namespace Pollus.Debugging;

using Pollus.Collections;
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
    public required GizmoType Type;
    public required Handle<Texture2D> Texture;
    public Handle<GPUBindGroup>? BindGroup;
}

class GizmoMaterialData
{
    public Handle<GPURenderPipeline> PipelineHandle = Handle<GPURenderPipeline>.Null;
    public Handle<GPUBindGroupLayout> BindGroupLayoutHandle = Handle<GPUBindGroupLayout>.Null;
}

public class GizmoBuffer
{
    Handle<GPUBuffer> drawBufferHandle = Handle<GPUBuffer>.Null;
    Handle<GPUBuffer> vertexBufferHandle = Handle<GPUBuffer>.Null;

    Handle<GPURenderPipeline> outlinedPipelineHandle = Handle<GPURenderPipeline>.Null;
    Handle<GPURenderPipeline> filledPipelineHandle = Handle<GPURenderPipeline>.Null;
    Handle<GPUBindGroup> bindGroupHandle = Handle<GPUBindGroup>.Null;

    Dictionary<Handle<Texture2D>, GizmoTexture> textures = new();
    Handle<GPUSampler> samplerHandle = Handle<GPUSampler>.Null;
    GizmoMaterialData fontMaterial = new();

    int drawCount;
    int vertexCount;

    SortKey[] drawOrder = new SortKey[1024];
    IndirectBufferData[] draws = new IndirectBufferData[1024];
    GizmoVertex[] vertices = new GizmoVertex[1024];

    bool isSetup = false;

    public int VertexCount => vertexCount;
    public int DrawCount => drawCount;
    public bool IsSetup => isSetup;

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
            textures.TryAdd(texture.Value, new GizmoTexture
            {
                Texture = texture.Value,
                Type = type,
            });
        }
    }

    public void Setup(IWGPUContext gpuContext, RenderAssets renderAssets)
    {
        if (isSetup) return;

        var filledMaterial = renderAssets.Get<MaterialRenderData>(new Handle<GizmoFilledMaterial>(0));
        var outlinedMaterial = renderAssets.Get<MaterialRenderData>(new Handle<GizmoOutlinedMaterial>(0));

        this.outlinedPipelineHandle = outlinedMaterial.Pipeline;
        this.filledPipelineHandle = filledMaterial.Pipeline;
        this.bindGroupHandle = filledMaterial.BindGroups[0];

        samplerHandle = renderAssets.Add(gpuContext.CreateSampler(SamplerDescriptor.Default));
        this.SetupFont(gpuContext, renderAssets);

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
            Label = """gizmo-texture-shader""",
            Backend = ShaderBackend.WGSL,
            Content = GizmoShaders.GIZMO_FONT_SHADER,
        });
        renderAssets.Add(shaderModule);

        var bindGroupLayout = gpuContext.CreateBindGroupLayout(new()
        {
            Label = """gizmo::texturedBindGroupLayout""",
            Entries = GizmoFontMaterial.BindGroupLayoutEntries,
        });
        var bindGroupLayoutHandle = renderAssets.Add(bindGroupLayout);

        var pipelineLayout = gpuContext.CreatePipelineLayout(new()
        {
            Label = """gizmo::texturedPipelineLayout""",
            Layouts = [bindGroupLayout]
        });
        renderAssets.Add(pipelineLayout);

        var texturedPipeline = gpuContext.CreateRenderPipeline(GizmoFontMaterial.PipelineDescriptor(gpuContext, shaderModule, pipelineLayout));
        var pipelineHandle = renderAssets.Add(texturedPipeline);

        fontMaterial = new()
        {
            PipelineHandle = pipelineHandle,
            BindGroupLayoutHandle = bindGroupLayoutHandle,
        };
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

        foreach (var texture in textures)
        {
            if (texture.Value.BindGroup.HasValue) continue;

            var textureRenderData = renderAssets.Get<TextureRenderData>(texture.Value.Texture);
            var textureView = renderAssets.Get(textureRenderData.View);
            var sampler = renderAssets.Get(samplerHandle);

            var bindGroupLayoutHandle = texture.Value.Type switch
            {
                GizmoType.Text => fontMaterial.BindGroupLayoutHandle,
                _ => throw new InvalidOperationException("Invalid gizmo type"),
            };

            var bindGroup = gpuContext.CreateBindGroup(new()
            {
                Label = $"""gizmo::fontBindGroup_{texture.Value.Texture.ID}""",
                Layout = renderAssets.Get(bindGroupLayoutHandle),
                Entries =
                [
                    BindGroupEntry.BufferEntry<SceneUniform>(0, sceneUniformBuffer, 0),
                    BindGroupEntry.TextureEntry(1, textureView),
                    BindGroupEntry.SamplerEntry(2, sampler),
                ],
            });
            textures[texture.Value.Texture] = textures[texture.Value.Texture] with
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
                    GizmoMode.Text => fontMaterial.PipelineHandle,
                    _ => throw new InvalidOperationException("Invalid gizmo mode"),
                });
                prevMode = sortKey.Mode;
            }

            if (sortKey.Texture != prevTexture)
            {
                if (sortKey is { Mode: GizmoMode.Textured or GizmoMode.Text, Texture: { } textureHandle }
                    && textures[textureHandle] is { BindGroup: { } texturedBindGroupHandle })
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