using InitialPrefabs.NimGui.Render;
using InitialPrefabs.NimGui.Text;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.Profiling;

namespace InitialPrefabs.NimGui {

    public static partial class ImGui {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void ProgressBarInternal(
            ImWindow window, int num, int denom, float2 size, in ImProgressBarStyle style) {
            
            var marker = new ProfilerMarker("Build_Fraction_Progress_Bar");
            marker.Begin();

            float ratio = (float)num / denom;

            ref UnmanagedImWindow unmanagedWindow = ref window.UnmanagedImWindow;
            ref ImScope lastScope = ref unmanagedWindow.LastScopeRef();

            var rect = ImLayoutUtility.CreateRect(in lastScope, in size, in unmanagedWindow.ScrollOffset);
            var left = new float2(rect.Min.x, (rect.Min.y + rect.Max.y) * 0.5f);
            float width = size.x * ratio;

            var fgRect = new ImRect {
                Position = left + new float2(width * 0.5f, 0f),
                Extents = new float2(width, size.y) * 0.5f
            };

            window.PushSolidBox(in rect, in style.Background);
            window.PushSolidBox(in fgRect, in style.Foreground);

            // -----------------------------------------
            // Construct the ImStrings
            // -----------------------------------------
            ImString numerator   = num.ToImString(ref window.Words);
            ImString divider     = '/'.ToImString(ref window.Words);
            ImString denominator = denom.ToImString(ref window.Words);

            var content = new ImString(numerator.Ptr, numerator.Length + denominator.Length + 1);
            ImTextStyle textStyle = style.GetTextStyle();
            window.PushTxt(content, rect, in textStyle);

            ImLayoutUtility.UpdateScope(ref lastScope, in size);
            marker.End();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void ProgressBarInternal(
            ImWindow window, float ratio, float2 size, in ImProgressBarStyle style) {

            var marker = new ProfilerMarker("Build_Ratio_Progress_Bar");
            marker.Begin();

            ref UnmanagedImWindow unmanagedWindow = ref window.UnmanagedImWindow;

            ref ImScope lastScope = ref unmanagedWindow.LastScopeRef();
            var rect = ImLayoutUtility.CreateRect(in lastScope, in size, in unmanagedWindow.ScrollOffset);

            // TODO: Add an option for left <-> right, bottom <-> top fills
            // Get the left corner
            var left = new float2(rect.Min.x, (rect.Min.y + rect.Max.y) * 0.5f);
            float width = size.x * ratio;

            var fgRect = new ImRect {
                Position = left + new float2(width * 0.5f, 0f),
                Extents = new float2(width, size.y) * 0.5f
            };

            window.PushSolidBox(in rect, in style.Background);
            window.PushSolidBox(in fgRect, in style.Foreground);

            // -----------------------------------------
            // Construct the ImString
            // -----------------------------------------
            int percent = (int)math.ceil(ratio * 100);
            ImString content = percent.ToImString(ref window.Words, 1);
            content.Ptr[content.Length - 1] = '%';

            ImTextStyle textStyle = style.GetTextStyle();
            window.PushTxt(content, rect, in textStyle);

            ImLayoutUtility.UpdateScope(ref lastScope, in size);
            marker.End();
        }

        /// <summary>
        /// Creates a ProgressBar given a width using the default styling.
        /// </summary>
        /// <param name="ratio">A value between 0 and 1</param>
        /// <param name="width">Max width of the rectangle containing the progress bar</param>
        public static void ProgressBar(float ratio, float width) {
            ProgressBar(ratio, width, ImProgressBarStyle.New());
        }

        /// <summary>
        /// Creates a ProgressBar given a width using custom styling.
        /// </summary>
        /// <param name="ratio">A value between 0 and 1</param>
        /// <param name="width">Max width of the rectangle containing the progress bar</param>
        /// <param name="style">Custom style</param>
        public static void ProgressBar(float ratio, float width, in ImProgressBarStyle style) {
            ImWindow window = ImGuiContext.GetCurrentWindow();

            // ImGuiContext.GetDefaultFont(out var glyphs, out var faceInfo);
            ref ImFontFace faceInfo = ref ImGuiRenderUtils.GetFontFace();
            var scale = style.FontSize / faceInfo.PointSize;
            var lineHeight = faceInfo.LineHeight * scale + style.Padding.y;

            ProgressBarInternal(window, ratio, new float2(width, lineHeight), in style);
        }

        /// <summary>
        /// Creates a ProgressBar given a size using the default styling.
        /// </summary>
        /// <param name="ratio">A value between 0 and 1</param>
        /// <param name="size">Size along the x and y axis</param>
        public static void ProgressBar(float ratio, float2 size) {
            var style = ImProgressBarStyle.New();
            ProgressBar(ratio, size, in style);
        }

        /// <summary>
        /// Creates a ProgressBar given a size using custom styling.
        /// </summary>
        /// <param name="ratio">A value between 0 and 1</param>
        /// <param name="size">Size along the x and y axis</param>
        /// <param name="style">A custom style</param>
        public static void ProgressBar(float ratio, float2 size, in ImProgressBarStyle style) {
            ImWindow window = ImGuiContext.GetCurrentWindow();
            ProgressBarInternal(window, ratio, size, in style);
        }

        /// <summary>
        /// Creates a ProgressBar given a size using default styling. The size is determined by 
        /// the line height and the width is determine by the parent scope.
        /// </summary>
        /// <param name="ratio">A value between 0 and 1</param>
        public static void ProgressBar(float ratio) {
            var style = ImProgressBarStyle.New();

            ImWindow window = ImGuiContext.GetCurrentWindow();
            float2 size = ImGui.CalculateRemainingLineSize(window, style.FontSize, in style.Padding);
            ProgressBarInternal(window, ratio, size, in style);
        }

        /// <summary>
        /// Creates a ProgressBar and displays the ratio as a fraction with a numerator and denominator 
        /// using a custom style.
        /// </summary>
        /// <param name="num">Numerator of the fraction</param>
        /// <param name="den">Denominator of the fraction</param>
        /// <param name="size">A custom size to define how large the progress bar is.</param>
        /// <param name="style">A custom style</param>
        public static void ProgressBar(int num, int den, float2 size, in ImProgressBarStyle style) {
            ImWindow window = ImGuiContext.GetCurrentWindow();
            ProgressBarInternal(window, num, den, size, in style);
        }

        /// <summary>
        /// Creates a ProgressBar and displays the ratio as a fraction with a numerator and denominator 
        /// using the default style.
        /// </summary>
        /// <param name="num">Numerator of the fraction</param>
        /// <param name="den">Denominator of the fraction</param>
        /// <param name="size">A custom size to define how large the progress bar is.</param>
        public static void ProgressBar(int num, int den, float2 size) {
            ImWindow window = ImGuiContext.GetCurrentWindow();
            var style = ImProgressBarStyle.New();
            ProgressBarInternal(window, num, den, size, in style);
        }

        /// <summary>
        /// Creates a ProgressBar and displays the ratio as a fraction with a numerator and denominator 
        /// using a custom style. The width and height is based off of the last known scope and 
        /// the size of the text.
        /// </summary>
        /// <param name="num">Numerator of the fraction</param>
        /// <param name="den">Denominator of the fraction</param>
        /// <param name="style">A custom style</param>
        public static void ProgressBar(int num, int den, in ImProgressBarStyle style) {
            ImWindow window = ImGuiContext.GetCurrentWindow();
            var size = ImGui.CalculateRemainingLineSize(window, style.FontSize, in style.Padding);
            ProgressBarInternal(window, num, den, size, in style);
        }

        /// <summary>
        /// Creates a ProgressBar and displays the ratio as a fraction with a numerator and denominator 
        /// using the default style. The width and height is based off of the last known scope and 
        /// the size of the text.
        /// </summary>
        /// <param name="num">Numerator of the fraction</param>
        /// <param name="den">Denominator of the fraction</param>
        public static void ProgressBar(int num, int den) {
            ProgressBar(num, den, ImProgressBarStyle.New());
        }
    }
}
