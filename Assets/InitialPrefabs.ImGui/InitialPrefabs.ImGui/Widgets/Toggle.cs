using InitialPrefabs.NimGui.Collections;
using InitialPrefabs.NimGui.Common;
using InitialPrefabs.NimGui.Inputs;
using InitialPrefabs.NimGui.Render;
using InitialPrefabs.NimGui.Text;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace InitialPrefabs.NimGui {

    public static partial class ImGui {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe bool Toggle(
            ImWindow window, 
            uint id, 
            bool initial, 
            in ImButtonStyle style) {

            ref ImScope lastScope = ref window.UnmanagedImWindow.LastScopeRef();
            ref ImFontFace fontFace = ref ImGuiRenderUtils.GetFontFace();

            Mouse mouse = InputHelper.GetMouseState();
            float2 lineHeight = fontFace.LineHeight * (style.FontSize / fontFace.PointSize) + style.Padding.y;

            bool contains = window.UnmanagedImWindow.ImToggled->TryGetValue(id, out bool state);
            if (!contains && initial) {
                window.UnmanagedImWindow.ImToggled->Add(id, state = true);
            }

            if (ImGui.OnButtonRelease(
                id, 
                in lineHeight, 
                in style, 
                in mouse, 
                ref window.UnmanagedImWindow, 
                out Color32 finalColor, 
                out ImRect toggleRect)) {

                if (contains) {
                    state = !(*window.UnmanagedImWindow.ImToggled)[id];
                    (*window.UnmanagedImWindow.ImToggled)[id] = state;
                } else {
                    window.UnmanagedImWindow.ImToggled->Add(id, true);
                    state = true;
                }
            }

            window.PushSolidBox(in toggleRect, in finalColor);
            // Draw the checkmark that will fill the button
            if (state) {
                toggleRect.Extents *= 0.75f;
                window.PushCheckmark(in toggleRect, in style.Text);
            }
            return state;
        }

        /// <summary>
        /// Returns the current state of the Toggle box. If the box is checked, then 
        /// the state is true, otherwise returns false.
        ///
        /// <remarks>The control ID is useful for pruning cached global states. You only need to 
        /// prune when you are no longer using the Toggle functionality.
        /// </remarks>
        ///
        /// <code>
        /// class UIBehaviour {
        ///     uint toggleID;
        ///     // Gets called once per frame.
        ///     void Update() {
        ///         if (ImGui.Toggle(out toggleID)) {
        ///             ...
        ///         }
        ///     }
        ///
        ///     ~UIBehaviour() {
        ///         // Internally this will remove the cached Toggle state from 
        ///         // it's internal hashmap, when this Object is destroyed.
        ///         ImGui.Prune(toggleID, PruneFlag.Toggle);
        ///     }
        /// }
        /// </code>
        /// </summary>
        /// <param name="controlID">The value of the ID that NimGui generated for this Toggle</param>
        /// <param name="initial">The initial state of the Toggle.</param>
        /// <returns>True, if checked, false if not.</returns>
        public static bool Toggle(out uint controlID, bool initial = false) {
            ImWindow window = ImGuiContext.GetCurrentWindow();
            var style = ImButtonStyle.New();

            controlID = ImIdUtility.RequestId();
            return Toggle(window, controlID, initial, in style);
        }

        /// <summary>
        /// Returns the current state of the Toggle box. If the box is checked, then 
        /// the state is true, otherwise returns false.
        /// 
        /// <remarks>
        /// The control ID is useful for pruning cached global states. You only need to 
        /// prune when you are no longer using the Toggle functionality.
        /// </remarks>
        ///
        /// <code>
        /// class UIBehaviour {
        ///     uint toggleID;
        ///     // Gets called once per frame.
        ///     void Update() {
        ///         if (ImGui.Toggle(out toggleID)) {
        ///             ...
        ///         }
        ///     }
        ///
        ///     ~UIBehaviour() {
        ///         // Internally this will remove the cached Toggle state from 
        ///         // it's internal hashmap, when this Object is destroyed.
        ///         ImGui.PruneToggle(toggleID, PruneFlags.Toggle);
        ///     }
        /// }
        /// </code>
        /// </summary>
        /// <param name="controlID">The value of the ID that NimGui generated for this Toggle</param>
        /// <param name="style">A custom style for the toggle box.</param>
        /// <param name="initial">The initial state of the Toggle.</param>
        /// <returns>True, if checked, false if not.</returns>
        public static bool Toggle(out uint controlID, in ImButtonStyle style, bool initial = false) {
            ImWindow window = ImGuiContext.GetCurrentWindow();
            controlID = ImIdUtility.RequestId();
            return Toggle(window, controlID, initial, in style);
        }

        /// <summary>
        /// Returns the current state of the Toggle box. If the box is checked, then the state is true, 
        /// otherwise returns false.
        /// 
        /// <remarks>
        /// Unlike <seealso cref="Toggle(out uint, bool)"/>, the control ID is determined by using 
        /// <seealso cref="TextUtils.GetStringHash(string)"/>.
        /// </remarks>
        /// 
        /// <code>
        /// class UIBehaviour {
        ///     // Gets called once per frame.
        ///     void Update() {
        ///         ImGui.Toggle("Label");
        ///     }
        ///     
        ///     ~UIBehaviour() {
        ///         ImGui.PruneToggle(TextUtils.GetStringHash("Label"), PruneFlags.Toggle);
        ///     }
        /// }
        /// </code>
        /// </summary>
        /// <param name="label">A label to provide context for the toggle box</param>
        /// <param name="initial">The initial value of the Toggle.</param>
        /// <returns>True, if checked, false if not.</returns>
        public static bool Toggle(string label, bool initial = false) {
            var style = ImButtonStyle.New();
            return Toggle(label, in style, initial);
        }

        /// <summary>
        /// Returns the current state of the Toggle box. If the box is checked, then the state is true, 
        /// otherwise returns false.
        /// 
        /// <remarks>
        /// Unlike <seealso cref="Toggle(out uint, bool)"/>, the control ID is determined by using 
        /// <seealso cref="TextUtils.GetStringHash(string)"/>.
        /// </remarks>
        /// 
        /// <code>
        /// class UIBehaviour {
        ///     // Gets called once per frame.
        ///     void Update() {
        ///         ImGui.Toggle("Label");
        ///     }
        ///     
        ///     ~UIBehaviour() {
        ///         ImGui.PruneToggle(TextUtils.GetStringHash("Label"), PruneFlags.Toggle);
        ///     }
        /// }
        /// </code>
        /// </summary>
        /// <param name="label">A label to provide context for the toggle box</param>
        /// <param name="style">A custom style for the button.</param>
        /// <param name="initial">The initial value of the Toggle.</param>
        /// <returns>True, if checked, false if not.</returns>
        public static bool Toggle(string label, in ImButtonStyle style, bool initial = false) {
            ImWindow window = ImGuiContext.GetCurrentWindow();
            // ------------------------------------------
            // Create the label
            // -----------------------------------------
            ImString content = window.Words.Request(label);
            ref ImScope lastScope = ref window.UnmanagedImWindow.LastScopeRef();

            ref ImFontFace fontFace = ref ImGuiRenderUtils.GetFontFace();
            UnsafeArray<ImGlyph> glyphs = ImGuiRenderUtils.GetGlyphs();
            var lastSize = lastScope.Rect.Size.x;

            var size = TextUtils.CalculateSize(
                in content, 
                in fontFace, 
                in glyphs, 
                in lastSize,
                in style.FontSize) + style.Padding;

            ImTextStyle textStyle = style.GetTextStyle();
                textStyle.WithColumn(HorizontalAlignment.Left);
            var rect = ImLayoutUtility.CreateRect(
                in lastScope, 
                in size, 
                in window.UnmanagedImWindow.ScrollOffset);

            size.x -= style.Padding.x;

            window.PushTxt(content, rect, in textStyle);
            ImLayoutUtility.UpdateScope(ref lastScope, in size);

            ImGui.SameLine();
            return Toggle(window, TextUtils.GetStringHash(in content), initial, in style);
        }
    }
}
