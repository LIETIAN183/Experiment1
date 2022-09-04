using System.Runtime.CompilerServices;
using InitialPrefabs.NimGui.Render;
using InitialPrefabs.NimGui.Text;
using Unity.Mathematics;
using Unity.Profiling;

namespace InitialPrefabs.NimGui {

    public static partial class ImGui {
    
        /// <summary>
        /// Calcaultes the size of the line based on the scope's next widget position.
        /// </summary>
        /// <param name="window">The window to calculate the remaining size.</param>
        /// <param name="fontSize">The size of the font.</param>
        /// <param name="padding">The amount of spacing between the current and next widget.</param>
        /// <returns>The size of the rect.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 CalculateRemainingLineSize(
            ImWindow window, 
            int fontSize, 
            in float2 padding) {

            ref var fontFace = ref ImGuiRenderUtils.GetFontFace();

            ImScope scope = window.UnmanagedImWindow.LastScope();
            float lineHeight = fontFace.CalculateLineHeight(fontSize, padding.y);

            float deltaWidth = 0;
            int multiplier = 2;

            if (scope.Delta.y <= math.EPSILON && scope.Delta.x > math.EPSILON) {
                // Multiply the padding by 3 so we can account for ImGui.SameLine();
                multiplier = 3;
                deltaWidth = scope.Previous.x - scope.Rect.Min.x + scope.Delta.x;
            }

            return new float2(
                scope.Rect.Size.x - deltaWidth - padding.x * multiplier,
                lineHeight);
        }
        
        /// <summary>
        /// Ensures that the next widget is drawn on the same line instead of the next line.
        /// <example>
        /// <code>
        /// ------------   ------------
        /// | Widget 1 |   | Widget 2 |
        /// ------------   ------------
        /// The second widget will be drawn on the same line after Widget 1.
        /// </code>
        /// </example>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void SameLine() {
            var marker = new ProfilerMarker("SameLine");
            marker.Begin();

            ImWindow window = ImGuiContext.GetCurrentWindow();
            ref ImScope last = ref window.UnmanagedImWindow.LastScopeRef();

            // TODO: Figure out how to pass in the padding.
            var xPadding = new float2(DefaultStyles.Padding.x, 0);
            var nextPosition =  last.Previous + last.Delta * new float2(1, 0) + xPadding;

            if (nextPosition.x + last.Delta.x < last.Rect.Max.x - xPadding.x * 2) {
                last.Next = nextPosition;
            }

            last.Delta = new float2(last.Delta.x, 0);

            marker.End();
        }

        /// <summary>
        /// Instead of drawing the widget on the next line, the next line is skipped and the 
        /// next widget is drawn on that succeeding line.
        ///
        /// <example>
        /// <code>
        /// ------------ 
        /// | Widget 1 |
        /// ------------ 
        ///             ImGui.SkipLine() will create an empty line here
        /// ------------ 
        /// | Widget 2 |
        /// ------------ 
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="style">A custom style for the skipped line.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SkipLine(in ImSkipLineStyle style) {
            ImWindow window = ImGuiContext.GetCurrentWindow();
            ref ImScope scope = ref window.UnmanagedImWindow.LastScopeRef();

            float2 size = ImGui.CalculateRemainingLineSize(
                window, 
                style.FontSize, 
                in style.Padding);

            ImLayoutUtility.UpdateScope(ref scope, in size);
        }


        /// <summary>
        /// Instead of drawing the widget on the next line, the next line is skipped and the 
        /// next widget is drawn on that succeeding line.
        ///
        /// <example>
        /// <code>
        /// ------------ 
        /// | Widget 1 |
        /// ------------ 
        ///             ImGui.SkipLine() will create an empty line here
        /// ------------ 
        /// | Widget 2 |
        /// ------------ 
        /// </code>
        /// </example>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SkipLine() {
            ImSkipLineStyle style = ImSkipLineStyle.New();
            SkipLine(in style);
        }
    }
}
