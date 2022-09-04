using System.Text;
using InitialPrefabs.NimGui.Collections;
using InitialPrefabs.NimGui.Common;
using InitialPrefabs.NimGui.Inputs;
using InitialPrefabs.NimGui.Render;
using InitialPrefabs.NimGui.Text;
using Unity.Mathematics;

namespace InitialPrefabs.NimGui {
    
    /// <summary>
    /// Convenience struct to conveniently create a collapsible scope.
    /// </summary>
    public ref struct ImCollapsibleArea {
        
        /// <summary>
        /// Use this property to determine whether or not you should 
        /// draw widgets.
        /// </summary>
        public readonly bool IsVisible;
        
        /// <summary>
        /// Stack only struct to call BeginCollapsible in a scope similar to 
        /// <seealso cref="ImPane"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// This should be used with the Dispose pattern.
        /// </remarks>
        /// 
        /// <example>
        /// How to use the Dispose pattern.
        /// <code>
        /// using (ImCollapsibleArea area = new ImCollapsible("Collapsible")) {
        ///     if (area.IsVisible) {
        ///         // Draw more logic here.
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <param name="label">The label for the clickable button.</param>
        public ImCollapsibleArea(string label) {
            var style = ImButtonStyle.New();
            ImWindow window = ImGuiContext.GetCurrentWindow();
            ImString content = window.Words.Request(label);

            ImGui.BeginCollapsible(window, in content, in style, false, out bool isCollapsed);
            IsVisible = !isCollapsed;
        }

        /// <summary>
        /// Stack only struct to call BeginCollapsible in a scope similar to 
        /// <seealso cref="ImPane"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// This should be used with the Dispose pattern.
        /// </remarks>
        /// 
        /// <example>
        /// How to use the Dispose pattern.
        /// <code>
        /// using (ImCollapsibleArea area = new ImCollapsible("Collapsible")) {
        ///     if (area.IsVisible) {
        ///         // Draw more logic here.
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <param name="label">The label for the clickable button.</param>
        /// <param name="style">A custom button style for the collapsible area.</param>
        public ImCollapsibleArea(string label, in ImButtonStyle style) {
            ImWindow window = ImGuiContext.GetCurrentWindow();
            ImString content = window.Words.Request(label);

            ImGui.BeginCollapsible(window, in content, in style, false, out bool isCollapsed);
            IsVisible = !isCollapsed;
        }

        /// <summary>
        /// Stack only struct to call BeginCollapsible in a scope similar to 
        /// <seealso cref="ImPane"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// This should be used with the Dispose pattern.
        /// </remarks>
        /// 
        /// <example>
        /// How to use the Dispose pattern.
        /// <code>
        /// using (ImCollapsibleArea area = new ImCollapsible("Collapsible")) {
        ///     if (area.IsVisible) {
        ///         // Draw more logic here.
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <param name="builder">The builder containing the label for the clickable button.</param>
        public ImCollapsibleArea(StringBuilder builder) {
            var style = ImButtonStyle.New();
            ImWindow window = ImGuiContext.GetCurrentWindow();
            ImString content = window.Words.Request(builder);

            ImGui.BeginCollapsible(window, in content, in style, false, out bool isCollapsed);
            IsVisible = !isCollapsed;
        }

        /// <summary>
        /// Stack only struct to call BeginCollapsible in a scope similar to 
        /// <seealso cref="ImPane"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// This should be used with the Dispose pattern.
        /// </remarks>
        /// 
        /// <example>
        /// How to use the Dispose pattern.
        /// <code>
        /// using (ImCollapsibleArea area = new ImCollapsible("Collapsible")) {
        ///     if (area.IsVisible) {
        ///         // Draw more logic here.
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <param name="builder">The builder containing the label for the clickable button.</param>
        /// <param name="style">A custom button style.</param>
        public ImCollapsibleArea(StringBuilder builder, in ImButtonStyle style) {
            ImWindow window = ImGuiContext.GetCurrentWindow();
            ImString content = window.Words.Request(builder);

            ImGui.BeginCollapsible(window, in content, in style, false, out bool isCollapsed);
            IsVisible = !isCollapsed;
        }

        /// <summary>
        /// Stack only struct to call BeginCollapsible in a scope similar to 
        /// <seealso cref="ImPane"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// This should be used with the Dispose pattern.
        /// </remarks>
        /// 
        /// <example>
        /// How to use the Dispose pattern.
        /// <code>
        /// using (ImCollapsibleArea area = new ImCollapsible("Collapsible", true)) {
        ///     if (area.IsVisible) {
        ///         // Draw more logic here.
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <param name="label">The StringBuilder containing the label for the clickable button.</param>
        /// <param name="isInitiallyCollapsed">Do you want the area collapsed.</param>
        public ImCollapsibleArea(string label, bool isInitiallyCollapsed) {
            var style = ImButtonStyle.New();
            ImWindow window = ImGuiContext.GetCurrentWindow();

            ImString content = window.Words.Request(label);
            ImGui.BeginCollapsible(
                window, 
                in content, 
                in style, 
                isInitiallyCollapsed, 
                out bool isCollapsed);

            IsVisible = !isCollapsed;
        }

        /// <summary>
        /// Stack only struct to call BeginCollapsible in a scope similar to 
        /// <seealso cref="ImPane"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// This should be used with the Dispose pattern.
        /// </remarks>
        /// 
        /// <example>
        /// How to use the Dispose pattern.
        /// <code>
        /// using (ImCollapsibleArea area = new ImCollapsible("Collapsible", true)) {
        ///     if (area.IsVisible) {
        ///         // Draw more logic here.
        ///     }
        /// }
        /// </code>
        /// </example>
        /// 
        /// <param name="label">The StringBuilder containing the label for the clickable button.</param>
        /// <param name="isInitiallyCollapsed">Do you want the area collapsed.</param>
        /// <param name="style">A custom button style.</param>
        public ImCollapsibleArea(string label, bool isInitiallyCollapsed, in ImButtonStyle style) {
            ImWindow window = ImGuiContext.GetCurrentWindow();

            ImString content = window.Words.Request(label);
            ImGui.BeginCollapsible(
                window, 
                in content, 
                in style, 
                isInitiallyCollapsed, 
                out bool isCollapsed);

            IsVisible = !isCollapsed;
        }

        /// <summary>
        /// Stack only struct to call BeginCollapsible in a scope similar to 
        /// <seealso cref="ImPane"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// This should be used with the Dispose pattern.
        /// </remarks>
        /// 
        /// <example>
        /// How to use the Dispose pattern.
        /// <code>
        /// using (ImCollapsibleArea area = new ImCollapsible("Collapsible", true)) {
        ///     if (area.IsVisible) {
        ///         // Draw more logic here.
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <param name="builder">The builder containing the label for the clickable button.</param>
        /// <param name="isInitiallyCollapsed">Do you want the area collapsed.</param>
        public ImCollapsibleArea(StringBuilder builder, bool isInitiallyCollapsed) {
            var style = ImButtonStyle.New();
            ImWindow window = ImGuiContext.GetCurrentWindow();

            ImString content = window.Words.Request(builder);
            ImGui.BeginCollapsible(
                window, 
                in content, 
                in style, 
                isInitiallyCollapsed, 
                out bool isCollapsed);

            IsVisible = !isCollapsed;
        }

        /// <summary>
        /// Stack only struct to call BeginCollapsible in a scope similar to 
        /// <seealso cref="ImPane"/>.
        /// </summary>
        /// 
        /// <remarks>
        /// This should be used with the Dispose pattern.
        /// </remarks>
        /// 
        /// <example>
        /// How to use the Dispose pattern.
        /// <code>
        /// using (ImCollapsibleArea area = new ImCollapsible("Collapsible", true)) {
        ///     if (area.IsVisible) {
        ///         // Draw more logic here.
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <param name="builder">The builder containing the label for the clickable button.</param>
        /// <param name="isInitiallyCollapsed">Do you want the area collapsed.</param>
        /// <param name="style">A custom button style.</param>
        public ImCollapsibleArea(StringBuilder builder, bool isInitiallyCollapsed, in ImButtonStyle style) {
            ImWindow window = ImGuiContext.GetCurrentWindow();

            ImString content = window.Words.Request(builder);
            ImGui.BeginCollapsible(
                window, 
                in content, 
                in style, 
                isInitiallyCollapsed, 
                out bool isCollapsed);

            IsVisible = !isCollapsed;
        }

        public void Dispose() { }
    }

    public static partial class ImGui {
        
        /// <summary>
        /// Creates a collapsible area.
        /// </summary>
        /// <param name="window">The window to create the collapsible area in.</param>
        /// <param name="label">The content to show in the clickable button.</param>
        /// <param name="style">The style of the collapsible area.</param>
        /// <param name="isInitiallyCollapsed">Should the area be collapsed on start?</param>
        /// <param name="isCollapsed">Is the area currently collapsed?</param>
        public unsafe static void BeginCollapsible(
            ImWindow window,
            in ImString label, 
            in ImButtonStyle style, 
            bool isInitiallyCollapsed,
            out bool isCollapsed) {

            UnsafeArray<ImGlyph> glyphs = ImGuiRenderUtils.GetGlyphs();
            ref ImFontFace fontFace = ref ImGuiRenderUtils.GetFontFace();

            float2 size = ImGui.CalculateRemainingLineSize(window, style.FontSize, in style.Padding);
            Mouse mouseState = InputHelper.GetMouseState();

            uint id = ImIdUtility.RequestId();

            ref UnmanagedImWindow unmanagedWindow = ref window.UnmanagedImWindow;
            if (!unmanagedWindow.ImCollapsibles->TryGetValue(id, out isCollapsed)) {
                isCollapsed = isInitiallyCollapsed;
                unmanagedWindow.ImCollapsibles->Add(id, isCollapsed);
            }

            if (ImGui.OnButtonRelease(
                id, 
                in size, 
                in style, 
                in mouseState, 
                ref unmanagedWindow, 
                out var finalColor, 
                out ImRect rect)) {

                (*unmanagedWindow.ImCollapsibles)[id] = !isCollapsed;
            }

            window.PushSolidBox(in rect, in finalColor);
            ImTextStyle textStyle = style.GetTextStyle();
            window.PushTxt(label, rect, in textStyle);

            // Determine the collapsible icon
            float2 topLeft = rect.Min;
            float extent = size.y * 0.5f;
            var collapseRect = new ImRect {
                Position = topLeft + extent,
                Extents = extent * 0.5f
            };

            window.PushHamburgerMenu(in collapseRect, in style.Text);
        }
    }
}
