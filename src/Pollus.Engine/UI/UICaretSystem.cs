namespace Pollus.Engine.UI;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Rendering;
using Pollus.Mathematics;
using Pollus.UI;
using Pollus.Utils;

public static class UICaretSystem
{
    public const string Label = "UICaretSystem::Update";

    public static SystemBuilder Create() => FnSystem.Create(
        new(Label) { RunsAfter = [UITextInputSystem.Label] },
        static (
            Time time,
            UIFocusState focusState,
            UITextBuffers textBuffers,
            Assets<FontAsset> fonts,
            Query query) =>
        {
            UpdateCaret(query, time, focusState, textBuffers, fonts);
        }
    );

    internal static void UpdateCaret(
        Query query,
        Time time,
        UIFocusState focusState,
        UITextBuffers textBuffers,
        Assets<FontAsset> fonts)
    {
        var focused = focusState.FocusedEntity;
        if (focused.IsNull) return;
        if (!query.Has<UITextInput>(focused)) return;

        ref var input = ref query.Get<UITextInput>(focused);

        // Tick blink timer
        input.CaretBlinkTimer += time.DeltaTimeF;
        if (input.CaretBlinkTimer >= input.CaretBlinkRate)
        {
            input.CaretBlinkTimer -= input.CaretBlinkRate;
            input.CaretVisible = !input.CaretVisible;
        }

        // Compute caret position from text up to cursor
        if (input.TextEntity.IsNull) return;
        if (!query.Has<UITextFont>(input.TextEntity)) return;

        ref readonly var textFont = ref query.Get<UITextFont>(input.TextEntity);
        if (textFont.Font == Handle<FontAsset>.Null) return;

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
}
