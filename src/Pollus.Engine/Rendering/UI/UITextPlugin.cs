namespace Pollus.Engine.Rendering;

using Pollus.ECS;
using Pollus.Assets;
using Pollus.Graphics;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.UI.Layout;
using Pollus.Utils;

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

    [System(nameof(RegisterMeasureFuncs))]
    public static readonly SystemBuilderDescriptor RegisterMeasureFuncsDescriptor = new()
    {
        Stage = CoreStage.First,
        RunsAfter = [EnsureTextMeshDescriptor.Label, PrepareUIFontDescriptor.Label],
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
        RunsAfter = ["UICaretSystem::Update"],
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

    static void RegisterMeasureFuncs(UITreeAdapter adapter, Assets<FontAsset> fonts, Query<UIText, ContentSize, UITextFont> query)
    {
        foreach (var row in query)
        {
            ref var uiText = ref row.Component0;
            ref var textFont = ref row.Component2;
            var entity = row.Entity;

            if (!uiText.IsDirty && adapter.GetNodeId(entity) >= 0)
            {
                // Already registered and not dirty, skip
                continue;
            }

            if (textFont.Font.IsNull()) continue;
            var fontAsset = fonts.Get(textFont.Font);
            if (fontAsset is null) continue;

            // Capture current text/size/font for the measure delegate
            var capturedText = uiText.Text;
            var capturedSize = uiText.Size;
            var capturedFont = fontAsset;
            adapter.SetMeasureFunc(entity, (knownDimensions, availableSpace) =>
            {
                float maxWidth = knownDimensions.Width ?? availableSpace.Width.AsDefinite() ?? float.MaxValue;
                var measured = TextBuilder.MeasureText(capturedText, capturedFont, capturedSize, maxWidth);
                return new Size<float>(
                    knownDimensions.Width ?? measured.X,
                    knownDimensions.Height ?? measured.Y);
            });

            uiText.IsDirty = false;
        }
    }

    static void BuildUITextMesh(Assets<FontAsset> fonts, Assets<TextMeshAsset> meshes, Query<UIText, TextMesh, UITextFont, ComputedNode> query)
    {
        query.ForEach((fonts, meshes), static (in data, ref uiText, ref mesh, ref textFont, ref computed) =>
        {
            float maxWidth = computed.Size.X - computed.PaddingLeft - computed.PaddingRight
                             - computed.BorderLeft - computed.BorderRight;
            if (maxWidth < 0f) maxWidth = 0f;

            bool widthChanged = uiText.LastBuildMaxWidth != maxWidth;
            if (!uiText.IsDirty && !widthChanged) return;

            var fontAsset = data.fonts.Get(textFont.Font);
            if (fontAsset == null) return;

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
            var result = TextBuilder.BuildMesh(uiText.Text, fontAsset, Vec2f.Zero, Vec4f.One, uiText.Size, tma.Vertices, tma.Indices, TextCoordinateMode.YDown, maxWidth);
            tma.Bounds = result.Bounds;

            uiText.LastBuildMaxWidth = maxWidth;
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

        var text = textBuffers.Get(focused);
        var cursorPos = Math.Min(input.CursorPosition, text.Length);

        // Use same sizing logic as TextBuilder.BuildMesh
        var sizePow = ((uint)fontSize).Clamp(8u, 128u).Snap(4u);
        var scale = fontSize / sizePow;
        var glyphKey = new GlyphKey(fontAsset.Handle, sizePow, '\0');

        // Compute X offset matching BuildMesh rounding behavior
        float cursorX = 0f;
        float lineHeight = 0f;
        for (int i = 0; i < cursorPos; i++)
        {
            glyphKey.Character = text[i];
            if (!fontAsset.Glyphs.TryGetValue(glyphKey, out var glyph))
                continue;
            if (lineHeight == 0f) lineHeight = glyph.LineHeight * scale;
            cursorX = float.Round(cursorX + glyph.Advance * scale);
        }

        // Get line height if we had no characters to measure
        if (lineHeight == 0f)
        {
            glyphKey.Character = ' ';
            if (fontAsset.Glyphs.TryGetValue(glyphKey, out var spaceGlyph))
                lineHeight = spaceGlyph.LineHeight * scale;
            else
                lineHeight = fontSize;
        }

        input.CaretXOffset = cursorX;
        input.CaretHeight = lineHeight;
    }

    static void Cleanup(UITreeAdapter adapter, RemovedTracker<UIText> tracker)
    {
        foreach (var removed in tracker)
        {
            removed.Component.Text.Dispose();
            adapter.RemoveMeasureFunc(new Entity(removed.Entity));
        }
    }
}
