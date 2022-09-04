using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace InitialPrefabs.NimGui {

    /// <summary>
    /// Defines the style of the viewable content, the scrollbar, and the scrollbar button.
    /// </summary>
    public partial struct ImScrollAreaStyle : IStyle {

        /// <summary>
        /// Size of the scroll button.
        /// </summary>
        public float ScrollButtonWidth;

        /// <summary>
        /// Amount of spacing between the current and next widget.
        /// </summary>
        public float2 Padding;

        /// <summary>
        /// Default scroll button color.
        /// </summary>
        public Color32 ButtonDefault;

        /// <summary>
        /// Scroll button color when the mouse is over the scroll button.
        /// </summary>
        public Color32 ButtonHover;

        /// <summary>
        /// Scroll button color when the mouse presses the button.
        /// </summary>
        public Color32 ButtonPressed;

        /// <summary>
        /// Background color of the scrollbar.
        /// </summary>
        public Color32 ScrollBarBackground;

        /// <summary>
        /// Panel color of the scroll area.
        /// </summary>
        public Color32 ScrollBarPanel;

        /// <summary>
        /// Time between frames.
        /// </summary>
        public float DeltaTime;

        /// <summary>
        /// Speed of scrolling with a mousewheel.
        /// </summary>
        public float ScrollSpeed;
        
        /// <summary>
        /// Constructs a new instance of the ScrollAreaStyle with default settings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImScrollAreaStyle New() {
            return new ImScrollAreaStyle {
                ScrollButtonWidth   = DefaultStyles.ScrollBarWidth,
                Padding             = DefaultStyles.Padding,
                ButtonDefault       = DefaultStyles.Default,
                ButtonHover         = DefaultStyles.Hover,
                ButtonPressed       = DefaultStyles.Pressed,
                ScrollBarBackground = DefaultStyles.ScrollBarBackground,
                ScrollBarPanel      = DefaultStyles.ScrollBarPanel,
                ScrollSpeed         = Screen.height * 1.5f,
                DeltaTime           = Time.deltaTime,
            };
        }
    }

    public static class ImScrollAreaStyleExtensions {

        /// <summary>
        /// Sets the implicit button style in the slider.
        /// </summary>
        /// <param name="scroll">The reference to the ImScrollAreaStyle.</param>
        /// <param name="style">The reference to the ImSliderStlye.</param>
        /// <returns>The same reference to the ImScrollAreaStyle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImButtonStyle GetButtonStyle(this in ImScrollAreaStyle style) {
            return new ImButtonStyle {
                Background  = style.ButtonDefault,
                Hover    = style.ButtonHover,
                Pressed  = style.ButtonPressed,
            };
        }

        /// <summary>
        /// Sets the implicit button style in the slider.
        /// </summary>
        /// <param name="scroll">The reference to the ImScrollAreaStyle.</param>
        /// <param name="style">The reference to the ImSliderStlye.</param>
        /// <returns>The same reference to the ImScrollAreaStyle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref ImScrollAreaStyle WithButtonStyle(
            this ref ImScrollAreaStyle scroll, in ImButtonStyle style) {
            scroll.ButtonDefault = style.Background;
            scroll.ButtonHover   = style.Hover;
            scroll.ButtonPressed = style.Pressed;
            return ref scroll;
        }
    }

}
