namespace Pollus.Engine.Rendering;

using Pollus.Collections;
using Pollus.ECS;
using Pollus.Assets;
using Pollus.Graphics;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;
using Pollus.Debugging;

public class UITextPlugin : IPlugin
{
    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From<UIRenderPlugin>(),
        PluginDependency.From<FontPlugin>(),
    ];

    public void Apply(World world)
    {
        world.Resources.Add(new UIFontResources());

        world.Schedule.AddSystemSet<UITextSystemSet>();
    }
}

[SystemSet]
public partial class UITextSystemSet
{
    [System(nameof(EnsureTextMesh))]
    public static readonly SystemBuilderDescriptor EnsureTextMeshDescriptor = new()
    {
        Stage = CoreStage.First,
    };

    [System(nameof(PrepareUIFont))]
    public static readonly SystemBuilderDescriptor PrepareUIFontDescriptor = new()
    {
        Stage = CoreStage.First,
        RunCriteria = EventRunCriteria<AssetEvent<FontAsset>>.Create,
    };

    [System(nameof(PropagateUIFontAtlas))]
    public static readonly SystemBuilderDescriptor PropagateUIFontAtlasDescriptor = new()
    {
        Stage = CoreStage.PreRender,
        RunsAfter = [RenderingPlugin.BeginFrameSystem],
    };

    class TextMeasureContext
    {
        public NativeUtf8 Text;
        public float Size;
        public float LineHeight;
        public GlyphSet? Set;

        public Size<float> Measure(Size<float?> knownDimensions, Size<AvailableSpace> availableSpace)
        {
            Guard.IsNotNull(Set, "Set is null");

            float maxWidth = knownDimensions.Width ?? availableSpace.Width.AsDefinite() ?? float.MaxValue;
            float? lineHeightOverride = LineHeight > 0f ? Size * LineHeight : null;
            var measured = TextBuilder.MeasureText(Text, Set, Size, maxWidth, lineHeightOverride);
            return new Size<float>(
                knownDimensions.Width ?? measured.X,
                knownDimensions.Height ?? measured.Y);
        }
    }

    class TextMeasureContexts
    {
        public readonly Dictionary<Entity, TextMeasureContext> Contexts = [];
    }

    [System(nameof(RegisterMeasureFuncs))]
    public static readonly SystemBuilderDescriptor RegisterMeasureFuncsDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsBefore = ["UILayoutSystem::SyncTree"],
        Locals = [Local.From(new TextMeasureContexts())],
    };

    [System(nameof(BuildUITextMesh))]
    public static readonly SystemBuilderDescriptor BuildUITextMeshDescriptor = new()
    {
        Stage = CoreStage.Last,
    };

    [System(nameof(CaretMeasure))]
    public static readonly SystemBuilderDescriptor CaretMeasureDescriptor = new()
    {
        Stage = CoreStage.PostUpdate,
        RunsAfter = [UICaretSystem.UpdateDescriptor.Label],
        RunsBefore = [UICaretSystem.UpdateVisualDescriptor.Label],
    };

    [System(nameof(Cleanup))]
    public static readonly SystemBuilderDescriptor CleanupDescriptor = new()
    {
        Stage = CoreStage.Last,
        RunsAfter = [BuildUITextMeshDescriptor.Label],
    };

    static void EnsureTextMesh(Commands commands, Query<UITextFont>.Filter<None<TextMesh>> query)
    {
        foreach (var row in query)
            commands.AddComponent(row.Entity, TextMesh.Default);
    }

    static void PrepareUIFont(AssetServer assetServer, UIFontResources uiFontResources,
        EventReader<AssetEvent<FontAsset>> fontEvents, Assets<FontAsset> fonts)
    {
        var linearSampler = assetServer.Load<SamplerAsset>("internal://samplers/linear");
        foreach (scoped ref readonly var fontEvent in fontEvents.Read())
        {
            if (fontEvent.Type is not (AssetEventType.Loaded or AssetEventType.Changed)) continue;
            var font = fonts.Get(fontEvent.Handle);
            if (font is null) continue;
            uiFontResources.SetFontData(fontEvent.Handle, font.Atlas, linearSampler);
        }
    }

    static void PropagateUIFontAtlas(UIFontResources uiFontResources, Query<UITextFont> query)
    {
        query.ForEach(uiFontResources, static (in res, ref textFont) =>
        {
            if (textFont.Font.IsNull()) return;
            var data = res.GetFontData(textFont.Font);
            if (data is { } fontData)
            {
                textFont.Atlas = fontData.Atlas;
                textFont.Sampler = fontData.Sampler;
            }
        });
    }

    static void RegisterMeasureFuncs(Local<TextMeasureContexts> local, UITreeAdapter adapter, Assets<FontAsset> fonts,
        RemovedTracker<UIText> removedTracker, Query<UIText, ContentSize, UITextFont> query)
    {
        var contexts = local.Value.Contexts;

        foreach (var removed in removedTracker)
            contexts.Remove(new Entity(removed.Entity));

        foreach (var row in query)
        {
            ref var uiText = ref row.Component0;
            ref var textFont = ref row.Component2;
            var entity = row.Entity;

            if (!uiText.IsDirty && adapter.GetNodeId(entity) >= 0)
                continue;

            if (textFont.Font.IsNull()) continue;
            var fontAsset = fonts.Get(textFont.Font);
            if (fontAsset is null) continue;

            if (!contexts.TryGetValue(entity, out var ctx))
            {
                ctx = new TextMeasureContext();
                contexts[entity] = ctx;
                adapter.SetMeasureFunc(entity, ctx.Measure);
            }

            ctx.Text = uiText.Text;
            ctx.Size = uiText.Size;
            ctx.LineHeight = uiText.LineHeight;
            ctx.Set = fontAsset.GetSetForSize(uiText.Size);

            adapter.MarkEntityDirty(entity);
            uiText.IsDirty = false;
        }
    }

    static void BuildUITextMesh(Assets<FontAsset> fonts, Assets<TextMeshAsset> meshes, Query<UIText, TextMesh, UITextFont, ComputedNode> query)
    {
        query.ForEach((fonts, meshes), static (in data, ref uiText, ref mesh, ref textFont, ref computed) =>
        {
            float maxWidth = computed.UnroundedSize.X - computed.PaddingLeft - computed.PaddingRight
                             - computed.BorderLeft - computed.BorderRight;
            if (maxWidth < 0f) maxWidth = 0f;

            float contentHeight = computed.Size.Y - computed.PaddingTop - computed.PaddingBottom
                                  - computed.BorderTop - computed.BorderBottom;

            bool widthChanged = uiText.LastBuildMaxWidth != maxWidth;
            bool heightChanged = uiText.LastBuildContentHeight != contentHeight;
            if (!uiText.IsDirty && !widthChanged && !heightChanged) return;

            var fontAsset = data.fonts.Get(textFont.Font);
            if (fontAsset == null) return;

            var set = fontAsset.GetSetForSize(uiText.Size);

            TextMeshAsset tma;
            if (mesh.Mesh == Handle<TextMeshAsset>.Null)
            {
                tma = new TextMeshAsset
                {
                    Name = $"UITextMesh-{Guid.NewGuid()}",
                    Vertices = new(),
                    Indices = new(),
                };
                mesh.Mesh = data.meshes.Add(tma);
            }
            else
            {
                tma = data.meshes.Get(mesh.Mesh) ?? throw new InvalidOperationException("Text mesh asset not found");
            }

            tma.Vertices.Clear();
            tma.Indices.Clear();
            float lineHeightPx = uiText.LineHeight > 0f ? uiText.Size * uiText.LineHeight : 0f;
            var result = TextBuilder.BuildMesh(uiText.Text, set, Vec2f.Zero, Vec4f.One, uiText.Size, tma.Vertices, tma.Indices, TextCoordinateMode.YDown, maxWidth, lineHeightPx);

            // Normalize glyph vertices to start at Y=0 and center when there is free space.
            if (tma.Vertices.Count > 0)
            {
                var verts = tma.Vertices.AsSpan();
                float minY = float.MaxValue;
                float maxY = float.MinValue;
                for (int i = 0; i < verts.Length; i++)
                {
                    float y = verts[i].Position.Y;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
                float glyphExtent = maxY - minY;
                float freeSpace = contentHeight - glyphExtent;
                float offset = -minY; // normalize to Y=0
                if (freeSpace > 0f)
                    offset += freeSpace / 2f; // center only when there is room
                for (int i = 0; i < verts.Length; i++)
                    verts[i].Position.Y += offset;
            }

            tma.Bounds = result.Bounds;

            uiText.LastBuildMaxWidth = maxWidth;
            uiText.LastBuildContentHeight = contentHeight;
            data.meshes.Set(mesh.Mesh, tma);
        });
    }

    static void CaretMeasure(UIFocusState focusState, UITextBuffers textBuffers, Assets<FontAsset> fonts, Query query)
    {
        var focused = focusState.FocusedEntity;
        if (focused.IsNull) return;
        if (!query.Has<UITextInput>(focused)) return;

        ref var input = ref query.Get<UITextInput>(focused);

        if (input.TextEntity.IsNull) return;
        if (!query.Has<UITextFont>(input.TextEntity)) return;

        ref readonly var textFont = ref query.Get<UITextFont>(input.TextEntity);
        if (textFont.Font.IsNull()) return;

        var fontAsset = fonts.Get(textFont.Font);
        if (fontAsset is null) return;

        float fontSize = 14f;
        if (query.Has<UIText>(input.TextEntity))
        {
            fontSize = query.Get<UIText>(input.TextEntity).Size;
        }

        var set = fontAsset.GetSetForSize(fontSize);

        var text = textBuffers.Get(focused);
        var cursorPos = Math.Min(input.CursorPosition, text.Length);

        // Use same sizing logic as TextBuilder.BuildMesh
        var scale = fontSize / set.SdfRenderSize;
        var glyphKey = new GlyphKey(set.FontHandle, '\0');

        // Compute X offset matching BuildMesh rounding behavior
        float cursorX = 0f;
        float lineHeight = 0f;
        for (int i = 0; i < cursorPos; i++)
        {
            glyphKey.Character = text[i];
            if (!set.Glyphs.TryGetValue(glyphKey, out var glyph))
                continue;
            if (lineHeight == 0f) lineHeight = glyph.LineHeight * scale;
            cursorX = float.Round(cursorX + glyph.Advance * scale);
        }

        // Get line height if we had no characters to measure
        if (lineHeight == 0f)
        {
            glyphKey.Character = ' ';
            if (set.Glyphs.TryGetValue(glyphKey, out var spaceGlyph))
                lineHeight = spaceGlyph.LineHeight * scale;
            else
                lineHeight = fontSize;
        }

        input.CaretXOffset = cursorX;
        input.CaretHeight = lineHeight;
    }

    static void Cleanup(UITreeAdapter adapter, UITextBuffers textBuffers, RemovedTracker<UIText> tracker)
    {
        foreach (var removed in tracker)
        {
            var entity = new Entity(removed.Entity);
            removed.Component.Text.Dispose();
            adapter.RemoveMeasureFunc(entity);
            textBuffers.Remove(entity);
        }
    }
}
