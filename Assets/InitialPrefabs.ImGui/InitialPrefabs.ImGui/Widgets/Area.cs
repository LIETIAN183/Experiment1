using InitialPrefabs.NimGui.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace InitialPrefabs.NimGui {

    public static partial class ImGui {

        internal static unsafe void UnsafeBeginScope(ImRect rect, ref UnmanagedImWindow window) {
            UnsafeList<ImScope>* scopes = window.ImScopeVector;
            var scope = ImScope.Create(rect);
            scopes->Add(in scope);
        }

        internal static unsafe void UnsafeEndScope(ref UnmanagedImWindow window, bool autoLayout = false) {
            UnsafeList<ImScope>* scopes = window.ImScopeVector;

            // We must keep the root scope
            if (scopes->Length > 1) {
                scopes->Length--;

                if (autoLayout) {
                    var prev = (scopes->LastPtr() + 1)->Rect.Size;
                    ImLayoutUtility.UpdateScope(ref window.LastScopeRef(), in prev);
                }
            }
        }

        /// <summary>
        /// Begins a new scope and ensures so that all widgets drawn are now 
        /// relative to the scope.
        /// </summary>
        /// <param name="rect">The size and position of the new scope.</param>
        public static unsafe void BeginScope(ImRect rect) {
            var window = ImGuiContext.GetCurrentWindow().UnmanagedImWindow;
            UnsafeBeginScope(rect, ref window);
        }

        /// <summary>
        /// Ends the previous scope and updates the layout engine.
        /// </summary>
        /// <param name="rect">The size and position of the new scope.</param>
        public static unsafe void EndScope(bool autoLayout = false) {
            var window = ImGuiContext.GetCurrentWindow().UnmanagedImWindow;
            UnsafeEndScope(ref window, autoLayout);
        }
    }
}
