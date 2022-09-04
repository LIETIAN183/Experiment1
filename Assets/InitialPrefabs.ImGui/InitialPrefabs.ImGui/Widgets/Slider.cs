using InitialPrefabs.NimGui.Collections;
using InitialPrefabs.NimGui.Common;
using InitialPrefabs.NimGui.Inputs;
using InitialPrefabs.NimGui.Render;
using InitialPrefabs.NimGui.Text;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Profiling;

namespace InitialPrefabs.NimGui {

    public static partial class ImGui {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe float HorizontalSliderInternal(
            ImWindow window, 
            in uint id, 
            in float min, 
            in float max, 
            in float tInitial,
            in float2 size, 
            in ImSliderStyle style, 
            out ImRect rect) {

            var marker = new ProfilerMarker("Horizontal_Float_Slider");
            marker.Begin();
            BoundsUtility.CheckMinMax(min, max);

            Mouse mouse = InputHelper.GetMouseState();

            ref UnmanagedImWindow unmanagedWindow = ref window.UnmanagedImWindow;
            ref ImScope lastScope = ref unmanagedWindow.LastScopeRef();

            rect = ImLayoutUtility.CreateRect(in lastScope, in size, in unmanagedWindow.ScrollOffset);

            float buttonXExtent   = math.min(size.x, size.y) * 0.5f;
            float halfHeight      = (rect.Min.y + rect.Max.y) * 0.5f;
            float2 quarterPadding = style.Padding * 0.25f;
            float2 left           = new float2(rect.Min.x + buttonXExtent, halfHeight);
            float2 right          = new float2(rect.Max.x - buttonXExtent, halfHeight);

            UnsafeParallelHashMap<uint, float2>* scrollOffset = unmanagedWindow.ImScrollOffsets;
            UnsafeParallelHashMap<uint, ImPaneOffset>* paneOffset = unmanagedWindow.ImPaneOffsets;

            // -------------------------------------------------------------------
            // Create the Slider Button
            // -------------------------------------------------------------------
            var sliderButtonRect = new ImRect(left, buttonXExtent - quarterPadding);

            if (!scrollOffset->ContainsKey(id) && tInitial > 0) {
                float2 distance = math.lerp(left, right, tInitial) - left;
                sliderButtonRect.Position = math.clamp(
                    sliderButtonRect.Position + distance, left, right);
            } else if (scrollOffset->TryGetValue(id, out float2 prevDistance)) {
                sliderButtonRect.Position = math.clamp(
                    sliderButtonRect.Position + prevDistance, left, right);
            }

            var buttonStyle = style.GetButtonStyle();
            var clicked = OnScrollButtonHold(
                id,
                in sliderButtonRect,
                in buttonStyle,
                in mouse,
                ref unmanagedWindow,
                out var finalColor);

            // Cache the drag offset
            if ((clicked & Mouse.State.Down) > 0) {
                paneOffset->Update(id, new ImPaneOffset {
                    Position = left,
                    Offset = mouse.Position - left
                });
            } else if ((clicked & Mouse.State.Held) > 0 && unmanagedWindow.TrackedItem == id) {
                var distanceVector = mouse.Position - left;
                scrollOffset->Update(id, new float2(distanceVector.x, 0));
            }

            window.PushSolidBox(in rect, in style.Background);
            window.PushSolidBox(in sliderButtonRect, in finalColor);
            ImLayoutUtility.UpdateScope(ref lastScope, in size);

            var t = math.distance(sliderButtonRect.Position, left) / math.distance(left, right);
            marker.End();
            return t;
        }

        /// <summary>
        /// Returns a value between two floating points. Uses the default style.
        /// <seealso cref="InitialPrefabs.NimGui.ImSliderStyle"/>
        /// </summary>
        /// <param name="min">The minimum value, must be less than the min</param>
        /// <param name="max">The maximum value, must be greater than the min</param>
        /// <param name="t">The initial value between 0 and 1 that describes the value between the min and max.</param>
        public static float Slider(float min, float max, float t = 0f) {
            var style = ImSliderStyle.New();
            return Slider(min, max, in style, t);
        }

        /// <summary>
        /// Returns a value between two integers. Uses the default style.
        /// <seealso cref="InitialPrefabs.NimGui.ImSliderStyle"/>
        /// </summary>
        /// <param name="min">The minimum value, must be less than the min</param>
        /// <param name="max">The maximum value, must be greater than the min</param>
        /// <param name="t">The initial value between 0 and 1 that describes the value between the min and max.</param>
        public static int Slider(int min, int max, float t = 0f) {
            var style = ImSliderStyle.New();
            return Slider(min, max, in style, t);
        }

        /// <summary>
        /// Returns a value between two floating points.
        /// </summary>
        /// <param name="min">The minimum value, must be less than the min</param>
        /// <param name="max">The maximum value, must be greater than the min</param>
        /// <param name="style">A custom slider style.</param>
        /// <param name="t">The initial value between 0 and 1 that describes the value between the min and max.</param>
        public static float Slider(float min, float max, in ImSliderStyle style, float t = 0f) {
            var window = ImGuiContext.GetCurrentWindow();

            // ------------------------------------------------
            // Generate the full progress bar rect
            // ------------------------------------------------
            var size = ImGui.CalculateRemainingLineSize(window, style.FontSize, in style.Padding);

            uint id = ImIdUtility.RequestId();

            // Find s, which will tell use how to lerp
            float s = HorizontalSliderInternal(
                window, 
                in id, 
                in min, 
                in max, 
                in t,
                in size, 
                in style, 
                out ImRect rect);

            // ------------------------------------------------
            // Push a command to generate the text.
            // ------------------------------------------------
            var final = math.lerp(min, max, t);
            var word = final.ToImString(ref window.Words);
            var textStyle = style.GetTextStyle();
            window.PushTxt(word, rect, in textStyle);
            return final;
        }

        /// <summary>
        /// Returns a value between two integers.
        /// </summary>
        /// <param name="min">The minimum value, must be less than the min</param>
        /// <param name="max">The maximum value, must be greater than the min</param>
        /// <param name="style">A custom slider style.</param>
        /// <param name="t">The initial value between 0 and 1 that describes the value between the min and max.</param>
        public static int Slider(int min, int max, in ImSliderStyle style, float t = 0f) {
            ImWindow window = ImGuiContext.GetCurrentWindow();

            // ------------------------------------------------
            // Generate the full progress bar rect
            // ------------------------------------------------
            ref ImFontFace fontFace = ref ImGuiRenderUtils.GetFontFace();

            var size = ImGui.CalculateRemainingLineSize(window, style.FontSize, in style.Padding);

            var id = ImIdUtility.RequestId();

            // Find s, which will tell use how to lerp
            var s = HorizontalSliderInternal(
                window, 
                in id, 
                min, 
                max,
                t,
                in size, 
                in style, 
                out var rect);

            // ------------------------------------------------
            // Push a command to generate the text.
            // ------------------------------------------------
            var final = (int)math.ceil(math.lerp(min, max, s));
            var word = final.ToImString(ref window.Words);
            var textStyle = style.GetTextStyle();
            window.PushTxt(word, rect, in textStyle);
            return final;
        }

        /// <summary>
        /// Returns a value between two floating points.
        /// </summary>
        /// <param name="label">A label to provide more context of the slider.</param>
        /// <param name="min">The minimum value, must be less than the min</param>
        /// <param name="max">The maximum value, must be greater than the min</param>
        /// <param name="style">A custom slider style</param>
        /// <param name="t">The initial value between 0 and 1 that describes the value between the min and max.</param>
        public static float Slider(string label, float min, float max, in ImSliderStyle style, float t = 0f) {
            ImWindow window = ImGuiContext.GetCurrentWindow();

            ImTextStyle textStyle = style.GetTextStyle();
            textStyle.WithColumn(HorizontalAlignment.Left);

            ImGui.LabelInternal_Left(window, label, in textStyle);
            ImGui.SameLine();

            // ------------------------------------------------
            // Generate the full progress bar rect
            // ------------------------------------------------
            float2 size = ImGui.CalculateRemainingLineSize(window, style.FontSize, in style.Padding);

            uint id = TextUtils.GetStringHash(label);
            // Find t, which will tell use how to lerp
            float s = HorizontalSliderInternal(
                window, 
                in id, 
                in min, 
                in max, 
                in t,
                in size, 
                in style, 
                out ImRect rect);

            // ------------------------------------------------
            // Push a command to generate the text.
            // ------------------------------------------------
            float final = math.lerp(min, max, s);
            ImString word = final.ToImString(ref window.Words);
            window.PushTxt(word, rect, in textStyle.WithColumn(HorizontalAlignment.Center));
            return final;
        }

        /// <summary>
        /// Returns a value between two integers.
        /// </summary>
        /// <param name="min">The minimum value, must be less than the min.</param>
        /// <param name="max">The maximum value, must be greater than the min.</param>
        /// <param name="style">A custom slider style.</param>
        /// <param name="t">The initial value between 0 and 1 that describes the value between the min and max.</param>
        public static int Slider(string label, int min, int max, in ImSliderStyle style) {
            ImWindow window  = ImGuiContext.GetCurrentWindow();

            ImTextStyle textStyle = style.GetTextStyle();
            textStyle.WithColumn(HorizontalAlignment.Left);

            ImGui.LabelInternal_Left(window, label, in textStyle);
            ImGui.SameLine();

            // ------------------------------------------------
            // Generate the full progress bar rect
            // ------------------------------------------------
            float2 size = ImGui.CalculateRemainingLineSize(window, style.FontSize, in style.Padding);

            uint id = TextUtils.GetStringHash(label);
            // Find t, which will tell use how to lerp
            float t = HorizontalSliderInternal(
                window, 
                in id, 
                min, 
                max,
                0f,
                in size, 
                in style, 
                out ImRect rect);

            int final = (int)math.lerp(min, max, t);
            ImString word = final.ToImString(ref window.Words);
            window.PushTxt(word, rect, in textStyle);
            return final;
        }

        /// <summary>
        /// Returns a value between two integers.
        /// </summary>
        /// <param name="label">A label to provide more context</param>
        /// <param name="min">The minimum value, must be less than the min</param>
        /// <param name="max">The maximum value, must be greater than the min</param>
        /// <param name="t">The initial value between 0 and 1 that describes the value between the min and max.</param>
        public static int Slider(string label, int min, int max, float t = 0f) {
            var style = ImSliderStyle.New();

            var window = ImGuiContext.GetCurrentWindow();
            var content = window.Words.Request(label);
            ref var lastScope = ref window.UnmanagedImWindow.LastScopeRef();

            ref ImFontFace fontFace = ref ImGuiRenderUtils.GetFontFace();
            UnsafeArray<ImGlyph> glyphs = ImGuiRenderUtils.GetGlyphs();
            float2 lastSize = lastScope.Rect.Size;

            var size = TextUtils.CalculateSize(
                in content, 
                in fontFace, 
                in glyphs, 
                in lastSize.x, 
                in style.FontSize) + style.Padding;

            var textStyle = style.GetTextStyle();
            textStyle.WithColumn(HorizontalAlignment.Left);
            var rect = ImLayoutUtility.CreateRect(
                in lastScope, 
                in size, 
                in window.UnmanagedImWindow.ScrollOffset);

            size.x -= style.Padding.x;
            window.PushTxt(content, rect, in textStyle);
            ImLayoutUtility.UpdateScope(ref lastScope, in size);

            ImGui.SameLine();
            return Slider(min, max, in style, t);
        }

        /// <summary>
        /// Returns a value between two floating points.
        /// </summary>
        /// <param name="label">A label to provide more context</param>
        /// <param name="min">The minimum value, must be less than the min</param>
        /// <param name="max">The maximum value, must be greater than the min</param>
        /// <param name="t">The initial value between 0 and 1 that describes the value between the min and max.</param>
        public static float Slider(string label, float min, float max, float t = 0f) {
            var style = ImSliderStyle.New();
            return Slider(label, min, max, in style, t);
        }
    }
}
