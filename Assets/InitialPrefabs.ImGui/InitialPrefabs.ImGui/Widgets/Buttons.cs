using System.Runtime.CompilerServices;
using InitialPrefabs.NimGui.Collections;
using InitialPrefabs.NimGui.Inputs;
using InitialPrefabs.NimGui.Render;
using InitialPrefabs.NimGui.Text;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace InitialPrefabs.NimGui {

    public static partial class ImGui {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Mouse.State OnScrollButtonHold(
            uint id,
            in ImRect rect,
            in ImButtonStyle style,
            in Mouse mouse,
            ref UnmanagedImWindow window,
            out Color32 finalColor) {

            finalColor = style.Background;
            var clicked = Mouse.State.None;

            if (rect.Contains(mouse.Position)) {
                window.HotItem = id;

                window.TryUpdateBufferedIds(id);

                //if ((window.ActiveItem == id || window.DrawBuffer.Index() > 0) && window.InteractedItem == 0) {
                if ((window.ActiveItem == id) && window.TrackedItem == 0) {
                    // We know we are first clicking the button
                    if (mouse.Is(Mouse.State.Down)) {
                        clicked |= Mouse.State.Down;
                        finalColor = style.Pressed;

                        // We know we must track this
                        window.TrackedItem = id;
                    }
                } else if (window.TrackedItem == id) {
                    if (mouse.Is(Mouse.State.Held)) {
                        clicked |= Mouse.State.Held;
                        finalColor = style.Pressed;
                    } else if (mouse.Is(Mouse.State.None)) {
                        finalColor = style.Hover;
                    }
                };
            }

            // We know we are tracking this button but our mouse isn't over the button
            if (window.TrackedItem == id && !rect.Contains(mouse.Position)) {
                if (mouse.Is(Mouse.State.Held)) {
                    clicked |= Mouse.State.Held;
                    finalColor = style.Pressed;
                } else if (mouse.Is(Mouse.State.Released)) {
                    clicked |= Mouse.State.Released;
                    finalColor = style.Background;

                    window.LastTrackedItem = window.TrackedItem;
                    window.TrackedItem = 0;
                } else {
                    finalColor = style.Background;
                    window.TrackedItem = 0;
                }
            }

            return clicked;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe static bool OnButtonRelease_TextField(
            uint id,
            in ImRect rect,
            in ImButtonStyle buttonStyle,
            in Mouse mouseState,
            ref UnmanagedImWindow window,
            out Color32 finalColor) {

            ref ImScope lastKnownScope = ref window.LastScopeRef();

            bool clicked = false;
            finalColor = buttonStyle.Background;

            if (rect.Contains(mouseState.Position) && 
                lastKnownScope.Rect.Contains(mouseState.Position) && 
                window.TrackedItem == 0) {

                window.HotItem = id;
                window.TryUpdateBufferedIds(id);

                if (window.ActiveItem == id) {
                    finalColor = buttonStyle.Hover;
                    if (mouseState.Is(Mouse.State.Released)) {
                        clicked = true;
                        window.ResetActiveItem();
                    }
                }
            }
            return clicked;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe static bool OnButtonRelease(
            uint id, 
            in float2 size, 
            in ImButtonStyle style, 
            in Mouse mouseState,
            ref UnmanagedImWindow window,
            out Color32 finalColor,
            out ImRect rect) { 
            
            // TODO: Create const readonly pointers to string data.
            var marker = new ProfilerMarker("Release_Button_Logic");
            marker.Begin();
            
            ref ImScope lastKnownScope = ref window.LastScopeRef();

            rect = ImLayoutUtility.CreateRect(in lastKnownScope, in size, in window.ScrollOffset);
            bool clicked = false;

            finalColor = style.Background;
            if (rect.Contains(mouseState.Position) && 
                lastKnownScope.Rect.Contains(mouseState.Position) && 
                window.TrackedItem == 0) {

                // Update the hot item, so we know which is the last known ID
                // The latest drawn element will be the final hot item.
                window.HotItem = id;

                window.TryUpdateBufferedIds(id);
                
                // This introduces a frame delay, so we must check for the most active item, 
                // which is set from the previous frame.
                if (window.ActiveItem == id) {
                    if (mouseState.Is(Mouse.State.Released)) {
                        clicked = true;
                        finalColor = style.Pressed;
                        window.ResetActiveItem();
                    } else if (mouseState.IsAny(Mouse.State.Held | Mouse.State.Down)) {
                        finalColor = style.Pressed;
                    } else {
                        finalColor = style.Hover;
                    }
                }
            }

            float2 maxSize = rect.Size;
            ImLayoutUtility.UpdateScope(ref lastKnownScope, in maxSize);
            marker.End();
            return clicked;
        }

        internal static bool Button(
            ImWindow window, 
            in uint id, 
            in ImString content, 
            in ImButtonStyle buttonStyle) {

            var marker = new ProfilerMarker("Build_Button");
            marker.Begin();

            ref UnmanagedImWindow unmanagedWindow = ref window.UnmanagedImWindow;
            Mouse mouseState = InputHelper.GetMouseState();

            var lastScope = unmanagedWindow.LastScope();

            ref ImFontFace fontFace = ref ImGuiRenderUtils.GetFontFace();
            UnsafeArray<ImGlyph> glyphs = ImGuiRenderUtils.GetGlyphs();

            float lastYSize = lastScope.Rect.Size.y;

            float2 size = math.ceil(TextUtils.CalculateSize(
                in content, 
                in fontFace, 
                in glyphs, 
                in lastYSize, 
                in buttonStyle.FontSize) + buttonStyle.Padding);

            bool final = ImGui.OnButtonRelease(
                id, 
                in size, 
                in buttonStyle, 
                in mouseState, 
                ref unmanagedWindow, 
                out var color, 
                out var rect);

            window.PushSolidBox(in rect, in color);
            ImTextStyle textStyle = buttonStyle.GetTextStyle();
            window.PushTxt(content, rect, in textStyle);

            marker.End();
            return final;
        }

        /// <summary>
        /// The default Button using a preconfigured Button Style. This registers a click 
        /// when the Mouse is released.
        /// </summary>
        /// <param name="label">The text to display in the button.</param>
        public static bool Button(string label) {
            var style = ImButtonStyle.New();
            return Button(label, in style);
        }

        /// <summary>
        /// Constructs a text button given a label and a button style.
        /// </summary>
        /// <param name="label">The text to display in the button.</param>
        /// <param name="style">The style of button.</param>
        public static bool Button(string label, in ImButtonStyle style) {
            var window = ImGuiContext.GetCurrentWindow();
            // ------------------------------------------------------------
            // Create a readonly string
            // ------------------------------------------------------------
            ImString content = window.Words.Request(label);
            uint id = TextUtils.GetStringHash(in content);
            return Button(window, id, content, style);
        }

        /// <summary>
        /// Constructs a Button given a control ID and label with the default ButtonStyle.
        /// </summary>
        /// <param name="controlID">A supplied unique ID.</param>
        /// <param name="label">The text to display </param>
        public static bool Button(uint controlID, string label) {
            var window = ImGuiContext.GetCurrentWindow();
            // ------------------------------------------------------------
            // Create a readonly string
            // ------------------------------------------------------------
            ImString content = window.Words.Request(label);
            var style = ImButtonStyle.New();
            return Button(window, controlID, content, in style);
        }

        /// <summary>
        /// Constructs a Button given a control ID and label 
        /// </summary>
        /// <param name="controlID">A supplied unique ID.</param>
        /// <param name="label">The text to display </param>
        /// <param name="style">A custom button style</param>
        public static bool Button(uint controlID, string label, ImButtonStyle style) {
            var window = ImGuiContext.GetCurrentWindow();
            // ------------------------------------------------------------
            // Create a readonly string
            // ------------------------------------------------------------
            ImString content = window.Words.Request(label);
            return Button(window, controlID, content, style);
        }
    }
}
