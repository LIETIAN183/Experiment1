using System.Runtime.CompilerServices;
using InitialPrefabs.NimGui.Collections;
using InitialPrefabs.NimGui.Inputs;
using InitialPrefabs.NimGui.Render;
using InitialPrefabs.NimGui.Text;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace InitialPrefabs.NimGui {

    /// <summary>
    /// Describes the state of the Pane.
    /// </summary>
    public enum ImPaneFlags {
        Closed    = 1 << 1,
        Collapsed = 1 << 2,
        Pinned    = 1 << 3
    }

    /// <summary>
    /// The ImPane is a stack allocated convenience struct to create 
    /// draggable and collapsible panes.
    /// </summary>
    public ref struct ImPane {
        
        /// <summary>
        /// Internally checks if the pane should show. This property is set 
        /// using the out parameter of ImGui.BeginPane(...) method.
        /// </summary>
        public readonly bool IsVisible;

        bool isBackBuffered;
        bool autoLayout;

        /// <summary>
        /// Disposable stack-only struct to conveniently call BeginPane and EndPane.
        /// <example>
        /// How to use the dispose pattern
        /// <code>
        /// using (var pane = new ImPane(label, position, size, windowStyle, buttonStyle)) {
        ///     // Write your pane logic here
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="title">The title of the pane</param>
        /// <param name="position">The initial position of the pane</param>
        /// <param name="size">The size of the pane</param>
        /// <param name="paneStyle">The style of the pane</param>
        /// <param name="buttonStyle">The style of the button</param>
        /// <param name="flags">Any flags which define the initial behavior of the Pane</param>
        /// <param name="autoLayout">Optionally, you can force the pane to automically layout the window</param>
        public ImPane(
            string title,
            float2 position, 
            float2 size, 
            in ImPaneStyle paneStyle, 
            in ImButtonStyle buttonStyle,
            ImPaneFlags flags = default,
            bool autoLayout = false) {

            bool isOpen = ImGui.BeginPane(
                title, position, size, 
                in buttonStyle, 
                in paneStyle, 
                out var collapsed, 
                out isBackBuffered,
                flags);

            IsVisible = !collapsed && isOpen;
            this.autoLayout = autoLayout;
        }

        /// <summary>
        /// Disposable stack-only struct to conveniently call BeginPane and EndPane. This uses the default
        /// color schemes.
        /// <example>
        /// How to use the dispose pattern
        /// <code>
        /// using (var pane = new ImPane(label, position, size)) {
        ///     // Write your pane logic here
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="title">The title of the pane</param>
        /// <param name="position">The initial position of the pane</param>
        /// <param name="size">The size of the pane</param>
        /// <param name="flags">Any flags which define the initial behavior of the Pane</param>
        /// <param name="autoLayout">Optionally, you can force the pane to automically layout the window</param>
        public ImPane(
            string title,
            float2 position, 
            float2 size,
            ImPaneFlags flags = default,
            bool autoLayout = false) {

            var buttonStyle = ImButtonStyle.New();
            var paneStyle = ImPaneStyle.New();

            bool isOpen = ImGui.BeginPane(
                title, position, size, 
                in buttonStyle, 
                in paneStyle, 
                out bool collapsed, 
                out isBackBuffered,
                flags);

            IsVisible = !collapsed && isOpen;
            this.autoLayout = autoLayout;
        }

        /// <summary>
        /// Disposable stack-only struct to conveniently call BeginPane and EndPane. You can supply a unique id
        /// instead of the BeginPane method hashing the label.
        ///
        /// <example>
        /// How to use the dispose pattern
        /// <code>
        /// using (var pane = new ImPane(label, position, size)) {
        ///     // Write your pane logic here
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="controlID">A unique ID for the pane</param>
        /// <param name="title">The title of the pane</param>
        /// <param name="position">The initial position of the pane</param>
        /// <param name="size">The size of the pane</param>
        /// <param name="paneStyle">The style of the pane</param>
        /// <param name="buttonStyle">The style of the button</param>
        /// <param name="autoLayout">Optionally, you can force the pane to automically layout the window</param>
        public ImPane(
            uint controlID, 
            string title, 
            float2 position,
            float2 size, 
            ImPaneFlags flags,
            in ImPaneStyle paneStyle, 
            in ImButtonStyle buttonStyle,
            bool autoLayout = false) {

            ImWindow window = ImGuiContext.GetCurrentWindow();
            ImString content = window.Words.Request(title);

            bool isOpen = ImGui.BeginPane(
                window, 
                controlID, 
                in content, 
                in position, 
                in size, 
                in buttonStyle, 
                in paneStyle, 
                out bool collapsed, 
                out isBackBuffered);

            IsVisible = !collapsed && isOpen;
            this.autoLayout = autoLayout;
        }

        public void Dispose() {
            ImWindow current = ImGuiContext.GetCurrentWindow();
            ImGui.EndPane(current, isBackBuffered, autoLayout);
        }
    }

    public static partial class ImGui {
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool Contains(uint interactedItem, in ImRect rect, in int2 position) {
            return rect.Contains(position) && interactedItem == 0;
        }

        internal unsafe static bool BeginPane(
            ImWindow window,
            uint id,
            in ImString content,
            in float2 position,
            in float2 size,
            in ImButtonStyle buttonStyle,
            in ImPaneStyle paneStyle,
            out bool collapse,
            out bool isRenderedLast,
            ImPaneFlags flags = default) {

            // ------------------------------------------------------------
            // Create the close button and collapse button IDs.
            // ------------------------------------------------------------
            int metaSize = content.Length + 2;
            ImString closeLabel = window.Words.Request(metaSize);
            closeLabel.AppendClose(in content);

            ImString collapsedLabel = window.Words.Request(metaSize);
            collapsedLabel.AppendCollapse(in content);

            uint closeID = TextUtils.GetStringHash(closeLabel);
            uint collapseID = TextUtils.GetStringHash(collapsedLabel);

            ref UnmanagedImWindow unmanagedWindow = ref window.UnmanagedImWindow;
            // ------------------------------------------------------------
            // Flags logic
            // ------------------------------------------------------------
            if (!unmanagedWindow.ImClosed->ContainsKey(id) && (flags & ImPaneFlags.Closed) > 0) {
                unmanagedWindow.ImClosed->Add(id, true);
                collapse = false;
                isRenderedLast = false;
                return false;
            } else if (unmanagedWindow.IsClosed(id)) {
                collapse = false;
                isRenderedLast = false;
                return false;
            }

            if ((flags & ImPaneFlags.Collapsed) > 0 && 
                !unmanagedWindow.ImCollapsibles->ContainsKey(id)) {
                unmanagedWindow.ImCollapsibles->Add(id, true);
                collapse = true;
            }

            bool isTracked = unmanagedWindow.TrackedItem == id;
            isRenderedLast = isTracked || 
                (unmanagedWindow.LastTrackedItem == id && unmanagedWindow.TrackedItem == 0);

            Mouse mouseState = InputHelper.GetMouseState();
            ImScope lastScope  = unmanagedWindow.LastScope();

            UnsafeArray<ImGlyph> glyphs = ImGuiRenderUtils.GetGlyphs();
            ref ImFontFace fontFace = ref ImGuiRenderUtils.GetFontFace();

            float2 textAreaSize = TextUtils.CalculateSize(
                in content, 
                in fontFace, 
                glyphs, 
                lastScope.Rect.Size.x, 
                paneStyle.DefaultFontSize);

            UnsafeParallelHashMap<uint, ImPaneOffset>* paneOffsets = unmanagedWindow.ImPaneOffsets;

            // If the previous position exists, then we must use the previously cached position.
            var windowRect = new ImRect(
                paneOffsets->TryGetValue(id, out var previous) ? previous.Position : position, 
                size / 2f);

            ImGui.BeginScope(windowRect);
            // ------------------------------------------------------------
            // Create the title bar
            // ------------------------------------------------------------
            float scale = paneStyle.TitleFontSize / fontFace.PointSize;

            // Readjust the text area size
            textAreaSize.x = math.max(size.x, textAreaSize.x + scale * fontFace.LineHeight * 2);

            var tl                = new float2(windowRect.Min.x, windowRect.Max.y);
            var titleBarExtents   = new float2(textAreaSize.x, scale * fontFace.LineHeight) / 2f;
            float2 center         = tl + titleBarExtents;
            var titleBarRect      = new ImRect(center, titleBarExtents);
            var otherButtonOffset = new float2(titleBarRect.Extents.y);

            // ------------------------------------------------------------
            // Create the collapsible button
            // ------------------------------------------------------------
            var collapsibleRect = new ImRect {
                Position = tl + otherButtonOffset,
                Extents  = new float2(titleBarRect.Extents.y)
            };

            // ------------------------------------------------------------
            // Create the close button
            // ------------------------------------------------------------
            var closeRect = new ImRect {
                Position = windowRect.Max + otherButtonOffset * new float2(-1, 1),
                Extents = new float2(titleBarRect.Extents.y)
            };

            // ------------------------------------------------------------
            // Modify the title bar rect
            // ------------------------------------------------------------
            var titleBarOffset = new float2(collapsibleRect.Extents.x, 0);
            ImRect titleBarButtonRect = titleBarRect;
            titleBarButtonRect.Extents -= titleBarOffset * 2;

            // ------------------------------------------------------------
            // Create the full pane rect
            // ------------------------------------------------------------
            var verticalExtent = new float2(0, titleBarRect.Extents.y);
            var scopeRect = new ImRect(windowRect.Position + verticalExtent, 
                windowRect.Extents + verticalExtent);

            Color32 collapseColor = paneStyle.CollapseDefaultFg;
            Color32 collapseBgColor = paneStyle.DefaultButtonBackground;

            // ------------------------------------------------------------
            // Collapse button logic
            // ------------------------------------------------------------
            bool inCollapsed = Contains(
                unmanagedWindow.TrackedItem, 
                in collapsibleRect, 
                in mouseState.Position);

            Color32 closeColor = paneStyle.CloseDefaultFg;
            Color32 closeBgColor = paneStyle.DefaultButtonBackground;
            // ------------------------------------------------------------
            // Close button logic
            // ------------------------------------------------------------
            bool inClose = Contains(
                unmanagedWindow.TrackedItem,
                in closeRect,
                in mouseState.Position);

            collapse = unmanagedWindow.IsCollapsed(id);

            // ------------------------------------------------------------
            // Draggable Window Logic
            // ------------------------------------------------------------
            bool isOverTargetRect = !inCollapsed && !inClose;

            // Determine the target rect and whether the mouse is over the window.
            if (!collapse) {
                isOverTargetRect &= scopeRect.Contains(mouseState.Position);
            } else {
                isOverTargetRect &= titleBarRect.Contains(mouseState.Position);
            }

            if (isOverTargetRect && unmanagedWindow.TrackedItem == 0) {
                unmanagedWindow.HotItem = id;
                if (unmanagedWindow.ActiveItem == id) {
                    if (mouseState.IsAny(Mouse.State.Down | Mouse.State.Held)) {
                        // First click so we must track it.
                        unmanagedWindow.TrackedItem = id;

                        paneOffsets->Update(id, new ImPaneOffset {
                            Offset = windowRect.Position - mouseState.Position,
                            Position = windowRect.Position
                        });
                    }
                }
            }

            // ------------------------------------------------------------
            // Collapsed button logic
            // ------------------------------------------------------------
            if (inCollapsed) {
                unmanagedWindow.HotItem = collapseID;
                if (unmanagedWindow.ActiveItem == collapseID) {
                    if (mouseState.Is(Mouse.State.Released)) {
                        unmanagedWindow.ToggleCollapsible(id);
                        unmanagedWindow.ResetActiveItem();
                    } else if (mouseState.IsAny(Mouse.State.Held | Mouse.State.Down)) {
                        collapseColor = paneStyle.CollapseDefaultFg;
                        collapseBgColor = paneStyle.DefaultButtonPress;
                    } else {
                        collapseColor = paneStyle.CollapseHoverFg;
                        collapseBgColor = paneStyle.DefaultButtonHover;
                    }
                }
            }

            // ------------------------------------------------------------
            // Close button logic
            // ------------------------------------------------------------
            if (inClose) {
                unmanagedWindow.HotItem = closeID;
                if (unmanagedWindow.ActiveItem == closeID) {
                    if (mouseState.Is(Mouse.State.Released)) {
                        unmanagedWindow.ToggleClosed(id);
                        unmanagedWindow.ResetActiveItem();
                    } else if (mouseState.IsAny(Mouse.State.Held | Mouse.State.Down)) {
                        closeColor = buttonStyle.Pressed;
                        closeBgColor = paneStyle.ClosePressedFg;
                    } else {
                        closeColor = buttonStyle.Hover;
                        closeBgColor = paneStyle.CloseHoverFg;
                    }
                }
            }

            if (unmanagedWindow.TrackedItem == id) {
                // Reset the tracked item
                switch (mouseState.Click) {
                    case Mouse.State.Released:
                        unmanagedWindow.LastTrackedItem = unmanagedWindow.TrackedItem;
                        unmanagedWindow.TrackedItem = 0;
                        break;
                    case Mouse.State.Held:
                        // If the pane is not pinned.
                        if ((flags & ImPaneFlags.Pinned) == 0) {
                            previous.Position = mouseState.Position + previous.Offset;
                            paneOffsets->Update(id, previous);
                        }
                        break;
                }
            }

            ref ImDrawBuilder cmds = ref window.UnmanagedImWindow.DrawBuffer;
            if (isRenderedLast) {
                cmds.Next();

                if (isOverTargetRect) {
                    unmanagedWindow.TryUpdateBufferedIds(id);
                } else if (inClose) {
                    unmanagedWindow.TryUpdateBufferedIds(closeID);
                } else if (inCollapsed) {
                    unmanagedWindow.TryUpdateBufferedIds(collapseID);
                }
            }

            // ------------------------------------------------------------
            // Drawing logic
            // ------------------------------------------------------------
            if (!collapse) {
                // Push the window
                window.PushSolidBox(in windowRect, in paneStyle.Pane);
            }

            cmds.Peek().PushScissor(in scopeRect);

            // Draw the title bar background
            window.PushSolidBox(in titleBarRect, in buttonStyle.Background);

            // Add the text content and draw the window
            ImTextStyle textStyle = paneStyle.GetTextStyle();
            window.PushTxt(content, titleBarRect, in textStyle);

            // Draw the collapsible button logic
            window.PushSolidBox(in collapsibleRect, in collapseBgColor);
            collapsibleRect.Extents *= 0.75f;
            window.PushHamburgerMenu(in collapsibleRect, in collapseColor);

            // Draw the close button logic
            window.PushSolidBox(in closeRect, in closeBgColor);
            closeRect.Extents *= 0.75f;
            window.PushX(in closeRect, in closeColor);
            return true;
        }

        /// <summary>
        /// Begins a draggable pane that defines a scope that can be drawn into.
        /// </summary>
        /// <param name="title">Label for the window</param>
        /// <param name="position">The position of where to initially draw</param>
        /// <param name="size">How big the window is</param>
        /// <param name="buttonStyle">Buttons colors</param>
        /// <param name="paneStyle">Window colors</param>
        /// <param name="collapse">Is the window collapsed?</param>
        /// <param name="isBackbuffered">Is the window queued to a different buffer?</param>
        public static bool BeginPane(
            string title, 
            in float2 position, 
            in float2 size, 
            in ImButtonStyle buttonStyle,
            in ImPaneStyle paneStyle,
            out bool collapse,
            out bool isBackbuffered,
            ImPaneFlags flags = default) {

            ImWindow window = ImGuiContext.GetCurrentWindow();

            // ------------------------------------------------------------
            // Create a readonly string for the title
            // ------------------------------------------------------------
            ImString content = window.Words.Request(title);
            uint id = TextUtils.GetStringHash(in content);

            return BeginPane(
                window, 
                id, 
                in content, 
                in position, 
                in size, 
                in buttonStyle, 
                in paneStyle, 
                out collapse, 
                out isBackbuffered,
                flags);
        }

        /// <summary>
        /// Ends the Pane so the next widget can be correctly drawn.
        /// </summary>
        /// <param name="pop">Popping ensures that the next widget drawn is not backbuffered</param>
        /// <param name="autoLayout">If you need to update the layout, for free floating panes, you don't need to</param>
        public static void EndPane(ImWindow window, bool pop, bool autoLayout = false) {
            ImGui.EndScope(autoLayout);
            ref ImDrawBuilder cmds = ref window.UnmanagedImWindow.DrawBuffer;
            cmds.Peek().PopScissor();
            if (pop) {
                cmds.Previous();
            }
        }
    }
}
