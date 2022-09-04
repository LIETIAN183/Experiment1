using InitialPrefabs.NimGui.Collections;
using InitialPrefabs.NimGui.Common;
using InitialPrefabs.NimGui.Inputs;
using InitialPrefabs.NimGui.Render;
using InitialPrefabs.NimGui.Text;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace InitialPrefabs.NimGui {

    public static partial class ImGui {

        [BurstCompile(FloatPrecision.Low, FloatMode.Fast)]
        internal struct CalculateMaxSizeJob : IJob {

            [WriteOnly]
            public NativeReference<float2> Size;

            [ReadOnly]
            public NativeArray<ImString> Strings;

            public float Fontsize;

            public ImFontFace FaceInfo;

            public UnsafeArray<ImGlyph> Glyphs;

            public float2 Padding;

            public void Execute() {
                var comparer = new GlyphComparer();
                var maxWidth = 0f;

                float scale = Fontsize / FaceInfo.PointSize;

                for (int i = 0; i < Strings.Length; ++i) {
                    ImString text = Strings[i];
                    var currentWidth = 0f;
                    for (int j = 0; j < text.Length; ++j) {
                        char c = text[j];
                        int idx = Glyphs.BinarySearch(c, comparer);
                        ImGlyph glyph = Glyphs[idx];
                        currentWidth += (glyph.Advance - glyph.Bearings.x) * scale;
                    }

                    maxWidth = math.max(currentWidth, maxWidth);
                }

                float height = FaceInfo.LineHeight * scale;
                Size.Value = new float2(maxWidth + 2 * height, height) + Padding;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static NativeArray<ImRect> CreateOptionsRect(int count, in ImRect rect) {
            var options = new NativeArray<ImRect>(count, Allocator.Temp);
            var offset = new float2(0, -rect.Size.y);

            for (int i = 0; i < count; ++i) {
                options[i] = new ImRect {
                    Extents = rect.Extents,
                    Position = rect.Position + ((i + 1) * offset)
                };
            }

            return options;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ImRect Consolidate(in NativeArray<ImRect> rects) {
            var position = new float2();
            float height = 0f;
            float width = rects[0].Extents.x;

            for (int i = 0; i < rects.Length; ++i) {
                ImRect current = rects[i];
                position += current.Position;
                height += current.Extents.y;
            }

            int size = math.select(rects.Length, 1, rects.Length == 0);
            return new ImRect {
                Position = position / size,
                Extents  = new float2(width, height)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe static NativeArray<ImString> Convert(ref ImWords words, string[] options) {
            var optionsImString = new NativeArray<ImString>(options.Length, Allocator.TempJob);
            for (int i = 0; i < options.Length; ++i) {
                fixed (char* ptr = options[i]) {
                    ref ImString element = ref optionsImString.ElementAt(i);
                    element = words.Request(options[i]);
                }
            }
            return optionsImString;
        }

        internal static unsafe ushort DropdownInternal(
            ImWindow window, 
            uint id, 
            ushort initialIndex, 
            string[] options, 
            in ImDropDownStyle style) {

            var marker = new ProfilerMarker("Build_Dropdown");
            marker.Begin();
            ref ImScope lastKnownScope = ref window.UnmanagedImWindow.LastScopeRef();
            // -----------------------------------------
            // Calculate the max size for each box
            // -----------------------------------------
            UnsafeArray<ImGlyph> glyphs = ImGuiRenderUtils.GetGlyphs();
            ref ImFontFace fontFace = ref ImGuiRenderUtils.GetFontFace();
            NativeArray<ImString> convertedOptions = Convert(ref window.Words, options);
            var size = new NativeReference<float2>(Allocator.TempJob);

            new CalculateMaxSizeJob {
                FaceInfo = fontFace,
                Fontsize = style.FontSize,
                Glyphs   = glyphs,
                Strings  = convertedOptions,
                Size     = size,
                Padding  = style.Padding
            }.Run();

            ImButtonStyle buttonStyle = style.GetButtonStyle();
            Mouse mouseState = InputHelper.GetMouseState();

            bool state = window.UnmanagedImWindow.ImToggled->ContainsKey(id);

            if (ImGui.OnButtonRelease(
                id, 
                size.Value, 
                in buttonStyle, 
                in mouseState, 
                ref window.UnmanagedImWindow, 
                out Color32 finalColor, 
                out ImRect rect)) {

                if (!state) {
                    window.UnmanagedImWindow.ImToggled->TryAdd(id, state = true);
                } else {
                    window.UnmanagedImWindow.ImToggled->Remove(id);
                    state = false;
                }
            }
            
            // If the state is true, then we have to draw the rest of the options and figure out if the 
            // mouse is hovered over the option
            // We still have to draw the first option anyways because its the selected option by default
            window.PushSolidBox(in rect, in finalColor);

            if (!window.UnmanagedImWindow.ImOptions->TryGetValue(id, out ushort selectedIndex)) {
                window.UnmanagedImWindow.ImOptions->Add(id, initialIndex);
                selectedIndex = initialIndex;
            }
            selectedIndex = (ushort)math.clamp(selectedIndex, 0, options.Length - 1);

            var padding = style.Padding.x * 0.5f;
            ImTextStyle textStyle = style.GetTextStyle();
            ImRect currentRect = rect;
            currentRect.Position.x += padding * 0.5f;
            currentRect.Extents.x -= padding;
            window.PushTxt(convertedOptions[selectedIndex], currentRect, in textStyle);

            // Draw the dropdown button
            float halfHeight = size.Value.y * 0.5f;
            float2 dropDownPosition = rect.Max - new float2(halfHeight);
            var dropDownRect = new ImRect(dropDownPosition, halfHeight * 0.5f);

            window.PushHamburgerMenu(in dropDownRect, in style.Text);

            if (state) {
                // ----------------------------------------------
                // Create the remaining options
                // ----------------------------------------------
                // We need to draw the remaining options
                // TODO: Figure out how to make sure the item is drawn on top and elements behind the dropdown 
                // don't interfere.
                var dropdownOptionsRects = CreateOptionsRect(options.Length, in rect);
                // Consolidate the rectangles so we can detect whether or not we've clicked outside of it
                var consolidatedRect = Consolidate(in dropdownOptionsRects);

                // Increment to the next command because we need to draw on top of everything.
                window.UnmanagedImWindow.DrawBuffer.Next();

                // Draw this as one giant rectangle to reduce the draw calls
                window.PushSolidBox(in consolidatedRect, in buttonStyle.Background);

                for (ushort i = 0; i < dropdownOptionsRects.Length; ++i) {
                    ImString optionLabel = convertedOptions[i];
                    uint optionId = TextUtils.GetStringHash(in optionLabel);
                    ImRect localRect = dropdownOptionsRects[i];

                    if (localRect.Contains(mouseState.Position)) {
                        window.UnmanagedImWindow.HotItem = optionId;
                        window.UnmanagedImWindow.TryUpdateBufferedIds(optionId);

                        if (window.UnmanagedImWindow.ActiveItem == optionId) {
                            Color32 ctxColor = mouseState.Is(Mouse.State.None) ? style.Hover : style.Pressed;
                            window.PushSolidBox(in localRect, in ctxColor);
                            // We know we clicked a valid option so remove the toggle state & select the option
                            if (mouseState.Is(Mouse.State.Released)) {
                                window.UnmanagedImWindow.ImToggled->Remove(id);
                                (*window.UnmanagedImWindow.ImOptions)[id] = i;
                            }
                        }
                    }

                    // Draw the text
                    ImRect adjustedRect = localRect;
                    adjustedRect.Extents.x -= padding;
                    adjustedRect.Position.x += padding / 2f;
                    window.PushTxt(optionLabel, adjustedRect, in textStyle);

                    // Draw a dot so the user knows which option is selected
                    if (i == selectedIndex) {
                        float2 topRight = localRect.Max;
                        var dotRect  = new ImRect(topRight - localRect.Extents.y, 2);
                        window.PushSolidBox(in dotRect, in style.Text);
                    }
                }

                // Decrement back because we are finished drawing everything at the end.
                window.UnmanagedImWindow.DrawBuffer.Previous();

                // Perform the logic to remove the toggled options if 
                // we didn't click the consolidated && toggle rect
                if (mouseState.IsAny(Mouse.State.Released | Mouse.State.Down) && 
                    !consolidatedRect.Contains(mouseState.Position) && 
                    !rect.Contains(mouseState.Position)) {
                    window.UnmanagedImWindow.ImToggled->Remove(id);
                }

                // NOTE: Reminder to clear here even though this is a no-op, 
                // if the allocation changes, then I won't forget.
                dropdownOptionsRects.Dispose();
            }

            // Dispose all temporary allocated 
            size.Dispose();
            convertedOptions.Dispose();
            marker.End();
            return selectedIndex;
        }

        /// <summary>
        /// Shows a menu dropdown where you can select a single option. Elements behind the dropdown menu 
        /// will not be selected.
        /// </summary>
        /// <param name="options">A set number of options</param>
        /// <param name="style">A style for the dropdown menu</param>
        /// <returns>The index of the element selected</returns>
        public static ushort Dropdown(string[] options, in ImDropDownStyle style) {
            ImWindow window = ImGuiContext.GetCurrentWindow();
            uint id = ImIdUtility.RequestId();
            return DropdownInternal(window, id, 0, options, in style);
        }

        /// <summary>
        /// Shows a menu dropdown where you can select a single option. Elements behind the dropdown menu 
        /// will not be selected.
        /// </summary>
        /// <param name="options">A set number of options</param>
        /// <param name="style">A style for the dropdown menu</param>
        /// <returns>The index of the element selected</returns>
        public static ushort Dropdown(string[] options, ushort initialIndex, in ImDropDownStyle style) {
            ImWindow window = ImGuiContext.GetCurrentWindow();
            uint id = ImIdUtility.RequestId();
            return DropdownInternal(window, id, initialIndex, options, in style);
        }

        /// <summary>
        /// Shows a menu dropdown where you can select a single option. Elements behind the dropdown menu 
        /// will not be selected.
        /// </summary>
        /// <param name="options">A set number of options</param>
        /// <returns>The index of the element selected</returns>
        public static ushort Dropdown(string[] options) {
            ImDropDownStyle style = ImDropDownStyle.New();
            return Dropdown(options, in style);
        }

        /// <summary>
        /// Shows a menu dropdown where you can select a single option. Elements behind the dropdown menu 
        /// will not be selected.
        /// </summary>
        /// <param name="options">A set number of options</param>
        /// <returns>The index of the element selected</returns>
        public static ushort Dropdown(string[] options, ushort initialIndex) {
            ImDropDownStyle style = ImDropDownStyle.New();
            ImWindow window = ImGuiContext.GetCurrentWindow();
            uint id = ImIdUtility.RequestId();
            return DropdownInternal(window, id, initialIndex, options, in style);
        }

        /// <summary>
        /// Shows a menu dropdown where you can select a single option. Elements behind the dropdown menu 
        /// will not be selected.
        /// </summary>
        /// <param name="label">A label to provide more context</param>
        /// <param name="options">A set number of options</param>
        /// <returns>The index of the element selected</returns>
        public static ushort Dropdown(string label, string[] options) {
            var style = ImDropDownStyle.New();
            ImWindow window = ImGuiContext.GetCurrentWindow();

            ImTextStyle textStyle = style.GetTextStyle();
            textStyle.WithColumn(HorizontalAlignment.Left);
            ImGui.LabelInternal_Left(window, label, in textStyle);
            ImGui.SameLine();

            uint id = TextUtils.GetStringHash(label);
            return DropdownInternal(window, id, 0, options, in style);
        }

        /// <summary>
        /// Shows a menu dropdown where you can select a single option. Elements behind the dropdown menu 
        /// will not be selected.
        /// </summary>
        /// <param name="label">A label to provide more context</param>
        /// <param name="initialIndex">The initial index of the Dropdown</param>
        /// <param name="options">A set number of options</param>
        /// <returns>The index of the element selected</returns>
        public static ushort Dropdown(string label, ushort initialIndex, string[] options) {
            var style = ImDropDownStyle.New();
            ImWindow window = ImGuiContext.GetCurrentWindow();

            ImTextStyle textStyle = style.GetTextStyle();
            textStyle.WithColumn(HorizontalAlignment.Left);
            ImGui.LabelInternal_Left(window, label, in textStyle);
            ImGui.SameLine();

            uint id = TextUtils.GetStringHash(label);
            return DropdownInternal(window, id, initialIndex, options, in style);
        }

        /// <summary>
        /// Shows a menu dropdown where you can select a single option. Elements behind the dropdown menu 
        /// will not be selected.
        /// </summary>
        /// <param name="label">A label to provide more context</param>
        /// <param name="options">A set number of options</param>
        /// <param name="style">A style for the dropdown menu</param>
        /// <returns>The index of the element selected</returns>
        public static ushort Dropdown(string label, string[] options, in ImDropDownStyle style) {
            ImWindow window = ImGuiContext.GetCurrentWindow();

            ImTextStyle textStyle = style.GetTextStyle();
            textStyle.WithColumn(HorizontalAlignment.Left);
            ImGui.LabelInternal_Left(window, label, in textStyle);
            ImGui.SameLine();

            uint id = TextUtils.GetStringHash(label);
            return DropdownInternal(window, id, 0, options, in style);
        }

        /// <summary>
        /// Shows a menu dropdown where you can select a single option. Elements behind the dropdown menu 
        /// will not be selected.
        /// </summary>
        /// <param name="label">A label to provide more context</param>
        /// <param name="initialIndex">The initial index of the Dropdown</param>
        /// <param name="options">A set number of options</param>
        /// <param name="style">A style for the dropdown menu</param>
        /// <returns>The index of the element selected</returns>
        public static ushort Dropdown(string label, ushort initialIndex, string[] options, in ImDropDownStyle style) {
            ImWindow window = ImGuiContext.GetCurrentWindow();

            ImTextStyle textStyle = style.GetTextStyle();
            textStyle.WithColumn(HorizontalAlignment.Left);
            ImGui.LabelInternal_Left(window, label, in textStyle);
            ImGui.SameLine();

            uint id = TextUtils.GetStringHash(label);
            return DropdownInternal(window, id, initialIndex, options, in style);
        }
    }
}
