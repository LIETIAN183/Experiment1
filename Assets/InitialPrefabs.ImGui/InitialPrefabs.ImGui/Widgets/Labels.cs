using InitialPrefabs.NimGui.Collections;
using InitialPrefabs.NimGui.Render;
using InitialPrefabs.NimGui.Text;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Mathematics;

namespace InitialPrefabs.NimGui {

    public static partial class ImGui {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LabelInternal_Left(ImWindow window, string label, in ImTextStyle style) {
            // ------------------------------------------
            // Create the label
            // -----------------------------------------
            ImString content = window.Words.Request(label);
            ref ImScope lastScope = ref window.UnmanagedImWindow.LastScopeRef();

            ref ImFontFace fontFace = ref ImGuiRenderUtils.GetFontFace();
            UnsafeArray<ImGlyph> glyphs = ImGuiRenderUtils.GetGlyphs();
            float lastWidth = lastScope.Rect.Size.x;

            float2 size = TextUtils.CalculateSize(
                in content, 
                in fontFace, 
                in glyphs, 
                in lastWidth,
                in style.FontSize) + style.Padding;

            ImRect rect = ImLayoutUtility.CreateRect(
                in lastScope, 
                in size, 
                in window.UnmanagedImWindow.ScrollOffset);

            size.x -= style.Padding.x;

            window.PushTxt(content, rect, in style);
            ImLayoutUtility.UpdateScope(ref lastScope, in size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LabelInternal(
            ImWindow window, 
            in ImString content, 
            in ImTextStyle textStyle) {

            // TODO: Check if we need to have unique IDs with labels, generally I don't think so.
            ref var unmanagedImWindow = ref window.UnmanagedImWindow;
            ref ImScope lastScope = ref unmanagedImWindow.LastScopeRef();

            UnsafeArray<ImGlyph> glyphs = ImGuiRenderUtils.GetGlyphs();
            ref ImFontFace fontFace = ref ImGuiRenderUtils.GetFontFace();

            float2 size = TextUtils.CalculateSize(
                in content, 
                in fontFace, 
                glyphs, 
                lastScope.Rect.Size.x - textStyle.Padding.x * 2, 
                textStyle.FontSize) + textStyle.Padding;

            var rect = ImLayoutUtility.CreateRect(in lastScope, in size, in unmanagedImWindow.ScrollOffset);
            window.PushTxt(content, rect, in textStyle);
            ImLayoutUtility.UpdateScope(ref lastScope, in size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void LabelInternal(
            ImWindow window, 
            in ImString content, 
            in float2 size, 
            in ImTextStyle style) {

            ref UnmanagedImWindow unmanagedImWindow = ref window.UnmanagedImWindow;
            ref ImScope lastScope = ref unmanagedImWindow.LastScopeRef();
            var rect = ImLayoutUtility.CreateRect(in lastScope, in size, in unmanagedImWindow.ScrollOffset);
            window.PushTxt(content, rect, in style);
            ImLayoutUtility.UpdateScope(ref lastScope, in size);
        }

        /// <summary>
        /// Creates a label to display some text with the default style.
        /// </summary>
        /// <param name="label">Text to display</param>
        public static void Label(string label) {
            var style = ImTextStyle.New();
            style.WithColumn(HorizontalAlignment.Left);
            Label(label, in style);
        }
        
        /// <summary>
        /// Creates a label to display some text using a StringBuilder with 
        /// the default style.
        /// </summary>
        /// <param name="builder">The StringBuilder containing the text.</param>
        public static void Label(StringBuilder builder) {
            var style = ImTextStyle.New();
            style.WithColumn(HorizontalAlignment.Left);
            Label(builder, in style);
        }

        /// <summary>
        /// Creates a label from a floating point value.
        /// </summary>
        /// <param name="value">The floating point to display.</param>
        public static void Label(float value) {
            var style = ImTextStyle.New();
            Label(value, in style);
        }

        /// <summary>
        /// Creates a label from a floating point value with a custom style.
        /// </summary>
        /// <param name="value">The floating point to display.</param>
        /// <param name="style">The style of the text.</param>
        public static void Label(float value, in ImTextStyle style) {
            var window = ImGuiContext.GetCurrentWindow();
            ImString content = value.ToImString(ref window.Words);
            LabelInternal(window, in content, in style);
        }

        /// <summary>
        /// Creates a label from an integer.
        /// </summary>
        /// <param name="value">The integer to display.</param>
        public static void Label(int value) {
            var style = ImTextStyle.New();
            Label(value, style);
        }

        /// <summary>
        /// Creates a label from an integer.
        /// </summary>
        /// <param name="value">The integer to display.</param>
        /// <param name="style">The style of the text.</param>
        public static void Label(int value, in ImTextStyle style) {
            var window = ImGuiContext.GetCurrentWindow();
            ImString content = value.ToImString(ref window.Words);
            LabelInternal(window, in content, in style);
        }

        /// <summary>
        /// Creates a label to display some text with the default style.
        /// </summary>
        /// <param name="label">Text to display</param>
        /// <param name="style">The style of the text.</param>
        public static void Label(string label, in ImTextStyle style) {
            var window = ImGuiContext.GetCurrentWindow();
            ImString content = window.Words.Request(label);
            LabelInternal(window, in content, in style);
        }

        /// <summary>
        /// Creates a label using a StringBuilder to display some text with the default style.
        /// </summary>
        /// <param name="builder">The StringBuilder containing the label.</param>
        /// <param name="style">The style of the text.</param>
        public static void Label(StringBuilder builder, in ImTextStyle style) {
            var window = ImGuiContext.GetCurrentWindow();
            ImString content = window.Words.Request(builder);
            LabelInternal(window, in content, in style);
        }

        /// <summary>
        /// Creates a label to display text with a custom size, but using the default style.
        /// </summary>
        /// <param name="label">Text to display</param>
        /// <param name="size">The size of the label's area</param>
        public static void Label(string label, in float2 size) {
            var style = ImTextStyle.New();
            style.WithColumn(HorizontalAlignment.Left);
            Label(label, size, in style);
        }

        /// <summary>
        /// Creates a label to display text with a custom style.
        /// </summary>
        /// <param name="label">Text to display.</param>
        /// <param name="size">The size of the label's area.</param>
        /// <param name="textStyle">The style of the text.</param>
        public static void Label(string label, in float2 size, in ImTextStyle textStyle) {
            var window = ImGuiContext.GetCurrentWindow();
            ImString content = window.Words.Request(label);
            LabelInternal(window, in content, in size, in textStyle);
        }
        
        /// <summary>
        /// Create a label to display text using a StringBuilder and a custom style.
        /// </summary>
        /// <param name="builder">The StringBuilder containing the label.</param>
        /// <param name="size">The size of the label's area.</param>
        /// <param name="textStyle">The style of the text.</param>
        public static void Label(StringBuilder builder, float2 size, in ImTextStyle textStyle) {
            var window = ImGuiContext.GetCurrentWindow();
            ImString content = window.Words.Request(builder);
            LabelInternal(window, in content, in size, in textStyle);
        }
    }
}
