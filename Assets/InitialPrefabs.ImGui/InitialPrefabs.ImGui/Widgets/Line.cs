using InitialPrefabs.NimGui.Render;
using Unity.Mathematics;

namespace InitialPrefabs.NimGui
{

    public static partial class ImGui
    {

        internal static unsafe void LineInternal(float height, ImLineStyle style)
        {
            var window = ImGuiContext.GetCurrentWindow();
            ref var unmanagedWindow = ref window.UnmanagedImWindow;
            ref ImScope lastScope = ref unmanagedWindow.LastScopeRef();

            var width = lastScope.Rect.Size.x - style.Padding;
            var size = new float2(width, height);
            size.x /= 10;// x 轴占据窗口的 1/10

            var rect = ImLayoutUtility.CreateRect(in lastScope, in size, in unmanagedWindow.ScrollOffset);
            window.PushSolidBox(in rect, in style.Color);
            ImLayoutUtility.UpdateScope(ref lastScope, in size);
        }

        /// <summary>
        /// Draws a line using the default style.
        /// </summary>
        public static void Line()
        {
            LineInternal(1, ImLineStyle.New());
        }

        /// <summary>
        /// Draws a line given a custom style.
        /// </summary>
        /// <param name="style">The style of the line.</param>
        public static void Line(ImLineStyle style)
        {
            LineInternal(1, style);
        }
    }
}
