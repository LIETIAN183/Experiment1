using System.Runtime.CompilerServices;
using InitialPrefabs.NimGui.Render;
using Unity.Mathematics;
using UnityEngine;

namespace InitialPrefabs.NimGui {

    public static partial class ImGui {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe static void BoxInternal(
            ImWindow window, in float2 size, in Color32 color, in bool updateScope) {

            ref var unmanagedWindow = ref window.UnmanagedImWindow;
            ref ImScope scope = ref unmanagedWindow.LastScopeRef();
            var rect = ImLayoutUtility.CreateRect(
                in scope, 
                in size, 
                in window.UnmanagedImWindow.ScrollOffset);

            if (updateScope) {
                ImLayoutUtility.UpdateScope(ref scope, in size);
            }

            window.PushSolidBox(in rect, in color);
        }

        /// <summary>
        /// Draws a box.
        /// </summary>
        /// <param name="size">How big is the box?</param>
        /// <param name="color">What color is the box?</param>
        /// <param name="updateScope">If the scope is updated, the next widget will be drawn below the box.</param>
        public static void Box(float2 size, Color32 color, bool updateScope = false) {
            BoxInternal(ImGuiContext.GetCurrentWindow(), in size, in color, in updateScope);
        }
    }
}
