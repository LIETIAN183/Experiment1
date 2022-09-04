using InitialPrefabs.NimGui.Text;
using System;

namespace InitialPrefabs.NimGui {
    
    /// <summary>
    /// The global state to clean up.
    /// </summary>
    public enum PruneFlag : short {
        All       = ~0,
        Collapsed = 1 << 0,
        Dropdown  = 1 << 1,
        Pane      = 1 << 2,
        Scroll    = 1 << 3,
        Toggle    = 1 << 4,
    }

    public static partial class ImGui {

        static readonly PruneFlag[] Flags = (PruneFlag[])System.Enum.GetValues(typeof(PruneFlag));

        /// <summary>
        /// Removes any id that has been internally cached. This includes Collapsed 
        /// elements, Dropdowns, Pane offsets, Scroll offsets, and Toggled states.
        /// <example>
        /// <code>
        /// string title = "Title";
        ///
        /// using (ImPane = new ImPane(title, ...)) {
        ///     ...
        /// }
        ///
        /// ImGui.Prune(title, PruneFlag.Prune);
        /// </code>  
        /// </example>    
        /// </summary>
        /// <param name="ids">An array of ids.</param>
        /// <param name="flags">The internal state to clean up, by default we clean everything.</param>
        public static void Prune(uint[] ids, PruneFlag flags = PruneFlag.All) {
            for (int i = 0; i < ids.Length; ++i) {
                Prune(ids[i], flags);
            }
        }
        
        /// <summary>
        /// Removes any id that has been internally cached. This includes Collapsed 
        /// elements, Dropdowns, Pane offsets, Scroll offsets, and Toggled states.
        /// <example>
        /// <code>
        /// string title = "Title";
        ///
        /// using (ImPane = new ImPane(title, ...)) {
        ///     ...
        /// }
        ///
        /// ImGui.Prune(title, PruneFlag.Prune);
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="titles">An array of unique labels.</param>
        /// <param name="flags">The internal state to clean up, by default we clean everything.</param>
        public static void Prune(string[] titles, PruneFlag flags = PruneFlag.All) {
            unsafe {
                uint* ids = stackalloc uint[titles.Length];
                for (int i = 0; i < titles.Length; ++i) {
                    ids[i] = TextUtils.GetStringHash(titles[i]);
                }

                for (int i = 0; i < titles.Length; ++i) {
                    Prune(ids[i], flags);
                }
            }
        }

        /// <summary>
        /// Removes any id that has been internally cached. This includes Collapsed 
        /// elements, Dropdowns, Pane offsets, Scroll offsets, and Toggled states.
        /// <example>
        /// <code>
        /// string title = "Title";
        ///
        /// using (ImPane = new ImPane(title, ...)) {
        ///     ...
        /// }
        ///
        /// ImGui.Prune(title, PruneFlag.Prune);
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="title">A unique title.</param>
        /// <param name="flags">The internal state to clean up, by default we clean everything.</param>
        public static void Prune(string title, PruneFlag flags = PruneFlag.All) {
            uint id = TextUtils.GetStringHash(title);
            Prune(id, flags);
        }

        /// <summary>
        /// Removes any id that has been internally cached. This includes Collapsed 
        /// elements, Dropdowns, Pane offsets, Scroll offsets, and Toggled states.
        /// <example>
        /// <code>
        /// int controlID = 1;
        ///
        /// using (ImPane pane = new ImPane(controlID, ...)) {
        ///     ...
        /// }
        ///
        /// ImGui.Prune(controlID, PruneFlag.Pane);
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="id">The unique ID to remove.</param>
        /// <param name="flags">The internal state to clean up, by default we clean everything.</param>
        public static void Prune(uint id, PruneFlag flags = PruneFlag.All) {
            unsafe {
                for (int i = 0; i < Flags.Length; ++i) {
                    var flag = Flags[i] & flags;

                    switch (flag) {
                        case PruneFlag.Collapsed:
                            foreach (var window in ImGuiContext.Windows) {
                                window.UnmanagedImWindow.ImCollapsibles->Remove(id);
                            }
                            break;
                        case PruneFlag.Dropdown:
                            foreach (var window in ImGuiContext.Windows) {
                                window.UnmanagedImWindow.ImOptions->Remove(id);
                            }
                            break;
                        case PruneFlag.Toggle:
                            foreach (var window in ImGuiContext.Windows) {
                                window.UnmanagedImWindow.ImToggled->Remove(id);
                            }
                            break;
                        case PruneFlag.Scroll: 
                            foreach (var window in ImGuiContext.Windows) {
                                window.UnmanagedImWindow.ImScrollOffsets->Remove(id);
                            }
                            break;
                        case PruneFlag.Pane:
                            foreach (var window in ImGuiContext.Windows) {
                                window.UnmanagedImWindow.ImPaneOffsets->Remove(id);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
