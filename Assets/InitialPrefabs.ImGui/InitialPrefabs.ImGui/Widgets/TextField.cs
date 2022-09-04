using System.Text;
using InitialPrefabs.NimGui.Collections;
using InitialPrefabs.NimGui.Common;
using InitialPrefabs.NimGui.Inputs;
using InitialPrefabs.NimGui.Render;
using InitialPrefabs.NimGui.Text;
using Unity.Mathematics;
using UnityEngine;

namespace InitialPrefabs.NimGui {

    public static partial class ImGui {
    
        internal unsafe static void TextFieldInternal(
            ImWindow window, 
            string label, 
            StringBuilder builder,
            in uint id,
            in ImTextFieldStyle style) {

            ImTextStyle textStyle = style.GetTextStyle();
            ImGui.LabelInternal_Left(window, label, in textStyle);

            ImGui.SameLine();
            float2 size = ImGui.CalculateRemainingLineSize(
                window, 
                style.FontSize, 
                in style.Padding);

            ref UnmanagedImWindow unmanagedWindow = ref window.UnmanagedImWindow;
            ref ImScope lastScope = ref window.UnmanagedImWindow.LastScopeRef();
            ImRect rect = ImLayoutUtility.CreateRect(
                in lastScope, 
                in size, 
                in unmanagedWindow.ScrollOffset);

            Mouse mouseState = InputHelper.GetMouseState();
            ImButtonStyle buttonStyle = style.GetButtonStyle();

            if (ImGui.OnButtonRelease_TextField(
                id, 
                in rect,
                in buttonStyle, 
                in mouseState, 
                ref unmanagedWindow,
                out Color32 finalColor)) {
                unmanagedWindow.TrackedItem = id;
            } else if (
                mouseState.IsAny(Mouse.State.Down | Mouse.State.Held) && 
                !rect.Contains(mouseState.Position) && 
                unmanagedWindow.TrackedItem == id) {

                unmanagedWindow.LastTrackedItem = unmanagedWindow.TrackedItem;
                unmanagedWindow.TrackedItem = 0;
            }

            window.PushSolidBox(in rect, in finalColor);
            rect.Extents -= style.Padding.x * 0.5f;

            ref InputText inputTxtHelper = ref InputHelper.GetInputTextHelper();
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER

#elif ENABLE_LEGACY_INPUT_MANAGER
            inputTxtHelper.Collect();
#endif

            var comparer = new GlyphComparer();
            UnsafeArray<ImGlyph> glyphs = ImGuiRenderUtils.GetGlyphs();
            ref ImFontFace fontFace = ref ImGuiRenderUtils.GetFontFace();
            float scale = style.FontSize / fontFace.PointSize;

            int boxGlyphIdx = glyphs.BinarySearch(ImGuiRenderUtils.Box, comparer);
            ref ImGlyph boxGlyph = ref glyphs.ElementAt(boxGlyphIdx);

            float offset = 0f;
            float totalWidth = rect.Size.x - style.Padding.x * 2 - (boxGlyph.Advance - boxGlyph.Bearings.x) * scale;
            int startIdx = 0;

            var marker = new Unity.Profiling.ProfilerMarker("Determine_Slice");
            marker.Begin();
            for (int i = builder.Length - 1; i >= 0 && builder.Length > 0; i--) {
                int idx = glyphs.BinarySearch(builder[i], comparer);
                ref ImGlyph glyph = ref glyphs.ElementAt(idx);
                float width = (glyph.Advance - glyph.Bearings.x) * scale;

                if (offset + width < totalWidth) {
                    offset += width;
                    startIdx = i;
                }
            }
            marker.End();

            ImString convertedString = window.Words.Request(builder, (ushort)startIdx);
            window.PushTxt(convertedString, rect, in textStyle);

            // Check if this is the most active widget
            if (id == unmanagedWindow.TrackedItem) {
                if (inputTxtHelper.IsBackspaced && builder.Length > 0) {
                    builder.Length -= 1;
                } else {
                    inputTxtHelper.AppendTo(builder);
                }

                float2 extents = boxGlyph.MetricsSize * scale * 0.5f;
                ImRect boxRect = new ImRect(
                    rect.Min + extents + new float2(offset, -style.Padding.y * 0.5f), 
                    extents);

                window.PushSolidBox(in boxRect, in style.Text);
            }

            ImLayoutUtility.UpdateScope(ref lastScope, size);
        }
        
        /// <summary>
        /// Fills a StringBuilder with inputs given from the keyboard.
        /// </summary>
        /// <param name="label">The label to describe the textfield's purpose</param>
        /// <param name="builder">The StringBuilder which will be filled by the TextField.</param>
        public static void TextField(string label, StringBuilder builder) {
            ImTextFieldStyle textFieldStyle = ImTextFieldStyle.New();
            TextField(label, builder, in textFieldStyle);
        }

        /// <summary>
        /// Fills a StringBuilder with inputs given from the keyboard.
        /// </summary>
        /// <param name="label">The label to describe the textfield's purpose</param>
        /// <param name="builder">The StringBuilder which will be filled by the TextField.</param>
        /// <param name="style">The style of the TextField.</param>
        public static void TextField(string label, StringBuilder builder, in ImTextFieldStyle style) {
            ImWindow window = ImGuiContext.GetCurrentWindow();
            float2 size = ImGui.CalculateRemainingLineSize(window, style.FontSize, in style.Padding);

            uint id = ImIdUtility.RequestId();
            TextFieldInternal(window, label, builder, in id, in style);
        }
    }
}
