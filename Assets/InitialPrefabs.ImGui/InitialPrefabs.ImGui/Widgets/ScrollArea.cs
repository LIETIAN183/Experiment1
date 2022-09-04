using InitialPrefabs.NimGui.Collections;
using InitialPrefabs.NimGui.Inputs;
using InitialPrefabs.NimGui.Render;
using InitialPrefabs.NimGui.Text;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Profiling;

namespace InitialPrefabs.NimGui {

    public ref struct ImScrollArea {

        bool autoLayout;

        /// <summary>
        /// Convenience struct to call ImGui.BeginScrollArea. Using the dispose pattern also calls 
        /// ImGui.EndScrollArea.
        ///
        /// <code>
        /// using (var scrollArea = new ImScrollArea("Title", viewportHeight, maxHeight)) {
        ///     // Implement logic you would like to draw within the scroll area.
        /// }
        /// </code>
        /// </summary>
        /// <param name="name">The name of the scroll area.</param>
        /// <param name="viewportHeight">How much space should be added to he scroll area?</param>
        /// <param name="maxHeight">What is the maximum height of the scroll area?</param>
        /// <param name="autoLayout">Should the scroll area be laid out automatically? Generally, yes.</param>
        public ImScrollArea(string name, float viewportHeight, float maxHeight, bool autoLayout = true) {
            this.autoLayout = autoLayout;
            ImScrollAreaStyle style = ImScrollAreaStyle.New();
            ImWindow window = ImGuiContext.GetCurrentWindow();
            ImGui.BeginScrollArea(window, name, viewportHeight, maxHeight, in style);
        }

        public ImScrollArea(string name, bool autoLayout = true) {
            this.autoLayout = autoLayout;

            var style = ImScrollAreaStyle.New();
            var window = ImGuiContext.GetCurrentWindow();
            var lastScope = window.UnmanagedImWindow.LastScope();
            var height = lastScope.Next.y - lastScope.Rect.Min.y - style.Padding.y;
            ImGui.BeginScrollArea(window, name, height, height * 3, in style);
        }

        public void Dispose() {
            ImGui.EndScrollArea(autoLayout);
        }
    }

    public static partial class ImGui {
        
        /// <summary>
        /// Starts a scroll area given the viewport height and the max height. The scroll area is hardscissored to 
        /// the region drawn.
        /// </summary>
        /// <param name="title">The unique identifier of the scroll area.</param>
        /// <param name="viewportHeight">The height in which we can view. Typically smaller than the maxHeight.</param>
        /// <param name="maxHeight">The maximum height that the area can store.</param>
        /// <param name="scrollAreaStyle">The style of the scroll area.</param>
        public static unsafe void BeginScrollArea(
            ImWindow window,
            string title, 
            float viewportHeight, 
            float maxHeight, 
            in ImScrollAreaStyle scrollAreaStyle) {

            var marker = new ProfilerMarker("Scroll_Area_Logic");
            marker.Begin();

            ref UnmanagedImWindow unmanagedWindow = ref window.UnmanagedImWindow;
            Mouse mouse = InputHelper.GetMouseState();

            // ---------------------------------------------------
            // Create the title & its ID.
            // ---------------------------------------------------
            ImString content = window.Words.Request(title);
            uint id = TextUtils.GetStringHash(in content);

            ImScope lastKnownScope = unmanagedWindow.LastScope();
            float2 prevSize = lastKnownScope.Rect.Size;

            // ---------------------------------------------------
            // Figure out the full scroll area
            // ---------------------------------------------------
            var adjustedSize = new float2(
                prevSize.x - scrollAreaStyle.Padding.x * 2,
                viewportHeight - scrollAreaStyle.Padding.y) * 0.5f;

            var positionOffset = new float2(lastKnownScope.Rect.Extents.x, -viewportHeight * 0.5f);
            var scrollAreaRect = new ImRect(lastKnownScope.Next + positionOffset, adjustedSize);

            // ---------------------------------------------------
            // Figure out the main rect area
            // ---------------------------------------------------
            var buttonWidth = scrollAreaStyle.ScrollButtonWidth * 0.5f;
            float shift  = -buttonWidth - scrollAreaStyle.Padding.x;

            ImRect mainAreaRect = scrollAreaRect;
            mainAreaRect.Position.x = mainAreaRect.Position.x - buttonWidth;
            mainAreaRect.Extents.x  = mainAreaRect.Extents.x - buttonWidth;

            // The main area rect is the scope that we must begin and end.
            ImGui.BeginScope(mainAreaRect);
            window.PushSolidBox(in mainAreaRect, in scrollAreaStyle.ScrollBarPanel);

            // ---------------------------------------------------
            // Figure out the scrollbar's background rect 
            // ---------------------------------------------------
            var scrollBarBgRect = new ImRect(
                scrollAreaRect.Max - new float2(scrollAreaStyle.ScrollButtonWidth * 0.5f, scrollAreaRect.Extents.y),
                new float2(scrollAreaStyle.ScrollButtonWidth * 0.5f, scrollAreaRect.Extents.y));

            window.PushSolidBox(in scrollBarBgRect, in scrollAreaStyle.ScrollBarBackground);

            // ---------------------------------------------------
            // Scrollbar Button Logic
            // ---------------------------------------------------
            // TODO Figure out the x.
            float scrollButtonHeight = viewportHeight * (viewportHeight / maxHeight);
            var offset = new float2(0, scrollBarBgRect.Extents.y - scrollButtonHeight * 0.5f);

            float2 top = scrollBarBgRect.Position + offset;
            float2 bottom = scrollBarBgRect.Position - offset;

            UnsafeParallelHashMap<uint, float2>* scrollOffsets = unmanagedWindow.ImScrollOffsets;

            // TODO: Make this expansive so that usability isn't kind of shit.
            // Assign the position to the top as the first step.
            var scrollButtonRect = new ImRect(top, 
                new float2(scrollAreaStyle.ScrollButtonWidth, scrollButtonHeight) * 0.5f);

            if (scrollOffsets->TryGetValue(id, out float2 prevDistance)) {
                scrollButtonRect.Position = math.clamp(scrollButtonRect.Position + prevDistance, bottom, top);
            }

            // For now only handle vertical scrolling
            // Calculate the distance between the mouse and the position.
            ImButtonStyle buttonStyle = scrollAreaStyle.GetButtonStyle();

            Mouse.State clicked = ImGui.OnScrollButtonHold(
                id, 
                in scrollButtonRect, 
                in buttonStyle,
                in mouse, 
                ref unmanagedWindow, 
                out var finalColor);

            // Get the clickOffset
            UnsafeParallelHashMap<uint, ImPaneOffset>* paneOffsets = unmanagedWindow.ImPaneOffsets;

            // Calculate t, the % that the user is scrolled through
            float t = math.distancesq(scrollButtonRect.Position, top) / math.distancesq(top, bottom);

            if ((clicked & Mouse.State.Down) > 0) {
                // Capture the distance betwen the mouse and the button
                // then we can offset the position of the distanceVector.
                paneOffsets->Update(id, new ImPaneOffset {
                    Position = top,
                    Offset = mouse.Position - top
                });
            } else if ((clicked & Mouse.State.Held) > 0 && paneOffsets->TryGetValue(id, out ImPaneOffset clickOffset)) {
                float2 distanceVector = mouse.Position - top;
                // TODO: Need to handle the x position
                scrollOffsets->Update(id, new float2(0, distanceVector.y));
            } else if (scrollAreaRect.Contains(mouse.Position)) {
                bool isBackBuffered  = unmanagedWindow.DrawBuffer.Index() > 0;
                if (isBackBuffered || unmanagedWindow.HotItem == 0) {
                    // Update the hot scrolling item
                    unmanagedWindow.HotItem = id;
                    unmanagedWindow.TryUpdateBufferedIds(id);
                }

                // When the active scroll item is the hot item and we are scrolling, then we 
                // move the items
                if ((unmanagedWindow.ActiveItem == id /*|| isBackBuffered*/) && mouse.IsScrolling) {
                    scrollOffsets->TryGetValue(id, out var distanceVector);
                    distanceVector = math.clamp(
                        distanceVector + mouse.ScrollDelta * new float2(scrollAreaStyle.ScrollSpeed) * scrollAreaStyle.DeltaTime,
                        -scrollButtonHeight * ((int)(maxHeight / viewportHeight) - 1), 
                        0);

                    scrollOffsets->Update(id, distanceVector);
                }
            }

            // ---------------------------------------------------
            // Scrollbar Button Rendering
            // ---------------------------------------------------
            window.PushSolidBox(in scrollButtonRect, in finalColor);

            // Update the stored scroll offset
            var distanceOffset = (maxHeight - viewportHeight) * t;
            unmanagedWindow.ScrollOffset = new float2(0, distanceOffset);

            // ---------------------------------------------------
            // Enable hardware scissoring
            // ---------------------------------------------------
            Commands cmds = unmanagedWindow.DrawBuffer.Peek();
            cmds.PushScissor(in mainAreaRect);

            marker.End();
        }

        /// <summary>
        /// Ends the scroll area and disables hardware scissoring.
        /// </summary>
        /// <param name="updateLayout">Should the layout be updated? Typically, yes.</param>
        public static void EndScrollArea(bool updateLayout = true) {
            var marker = new ProfilerMarker("Adjust_Viewport_Positions");
            marker.Begin();

            // ---------------------------------------------------
            // End the scope
            // ---------------------------------------------------
            ImGui.EndScope(updateLayout);

            // ---------------------------------------------------
            // Disable hardware scissoring
            // ---------------------------------------------------
            ImWindow window = ImGuiContext.GetCurrentWindow();
            ref UnmanagedImWindow unmanagedWindow = ref window.UnmanagedImWindow;
            Commands cmds = unmanagedWindow.DrawBuffer.Peek();
            cmds.PopScissor();

            unmanagedWindow.ScrollOffset = new float2();

            marker.End();
        }
    }
}
