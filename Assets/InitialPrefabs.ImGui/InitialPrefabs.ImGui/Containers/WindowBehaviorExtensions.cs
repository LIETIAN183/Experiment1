using System.Runtime.CompilerServices;
using InitialPrefabs.NimGui.Text;

namespace InitialPrefabs.NimGui {

    public static class WindowBehaviorExtensions {

        /// <summary>
        /// If the pane was previously closed, shows the window.
        /// </summary>
        /// <param name="window">The window containing the pane.</param>
        /// <param name="title">The title of the pane</param>
        public static void OpenPane(this ImWindow window, string title) {
            unsafe { 
                uint id = TextUtils.GetStringHash(title);
                if (window.UnmanagedImWindow.ImClosed->ContainsKey(id)) {
                    (*window.UnmanagedImWindow.ImClosed)[id] = false;
                }
            }
        }

        /// <summary>
        /// If the pane is available, closes the window.
        /// </summary>
        /// <param name="window">The window containing the pane.</param>
        /// <param name="title">The title of the pane</param>
        public static void ClosePane(this ImWindow window, string title) {
            unsafe {
                uint id = TextUtils.GetStringHash(title);
                if (window.UnmanagedImWindow.ImClosed->ContainsKey(id)) {
                    (*window.UnmanagedImWindow.ImClosed)[id] = true;
                }
            }
        }

        /// <summary>
        /// Is the window closed?
        /// </summary>
        /// <param name="title">The title of the pane to look for.</param>
        /// <param name="window">The window containing the pane.</param>
        public static bool IsClosed(this ImWindow window, string title) {
            unsafe {
                uint id = TextUtils.GetStringHash(title);
                bool exists = window.UnmanagedImWindow.ImClosed->TryGetValue(id, out bool closed);
                return exists && !closed;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsCollapsed(this ref UnmanagedImWindow window, uint id) {
            unsafe {
                window.ImCollapsibles->TryGetValue(id, out bool isCollapsed);
                return isCollapsed;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsClosed(this ref UnmanagedImWindow window, uint id) {
            unsafe {
                window.ImClosed->TryGetValue(id, out bool isClosed);
                return isClosed;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ToggleClosed(this ref UnmanagedImWindow window, uint id) {
            unsafe {
                if (window.ImClosed->TryGetValue(id, out bool isClosed)) {
                    (*window.ImClosed)[id] = !isClosed;
                } else {
                    window.ImClosed->Add(id, true);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ToggleCollapsible(this ref UnmanagedImWindow window, uint id) {
            unsafe {
                if (window.ImCollapsibles->TryGetValue(id, out bool isCollapsed)) {
                    (*window.ImCollapsibles)[id] = !isCollapsed;
                } else {
                    window.ImCollapsibles->Add(id, true);
                }
            }
        }
    }
}
