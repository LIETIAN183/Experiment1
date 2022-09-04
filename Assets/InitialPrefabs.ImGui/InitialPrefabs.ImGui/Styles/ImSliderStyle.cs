using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace InitialPrefabs.NimGui {
    /// <summary>
    /// Defines how the slider button looks and its background.
    /// </summary>
    public partial struct ImSliderStyle : IStyle {

        /// <summary>
        /// Default color of the slider's background.
        /// </summary>
        public Color32 Background;

        /// <summary>
        /// Default color of the slider button.
        /// </summary>
        public Color32 ButtonDefault;

        /// <summary>
        /// 
        /// </summary>
        public Color32 ButtonHover;
        public Color32 ButtonPressed;
        public Color32 TextColor;

        public int FontSize;
        public float2 Padding;
        
        /// <summary>
        /// Constructs the default style.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImSliderStyle New() {
            return new ImSliderStyle {
                Background        = DefaultStyles.SliderBackground,
                ButtonDefault     = DefaultStyles.Default,
                ButtonHover       = DefaultStyles.Hover,
                ButtonPressed     = DefaultStyles.Pressed,
                Padding           = DefaultStyles.Padding,
                FontSize          = DefaultStyles.DefaultFontSize,
                TextColor         = DefaultStyles.Text
            };
        }
    }

    public static class ImSliderStyleExtensions {

        /// <summary>
        /// Returns a reference to a copy of the style.
        /// <remarks>
        /// This API is considered experimental.
        /// </remarks>
        /// </summary>
        /// <param name="style">The style to copy.</param>
        /// <returns>The reference to the copy of the style.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref ImSliderStyle AsRef(this ImSliderStyle style) {
            unsafe { return ref *&style; }
        }
        
        /// <summary>
        /// Gets the implicit ImButtonStyle from the ImSliderStyle.
        /// </summary>
        /// <param name="style">The reference to the ImSliderStyle.</param>
        /// <returns>An instance of the ImButtomStyle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImButtonStyle GetButtonStyle(this in ImSliderStyle style) {
            return new ImButtonStyle {
                Background = style.ButtonDefault,
                Hover      = style.ButtonHover,
                Pressed    = style.ButtonPressed,
                Padding    = style.Padding,
                Text       = style.TextColor,
                FontSize   = style.FontSize
            };
        }

        /// <summary>
        /// Gets the implicit ImTextStyle from the ImSliderStyle.
        /// </summary>
        /// <param name="style">The reference to the ImSliderStyle.</param>
        /// <returns>An instance of the ImTextStyle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImTextStyle GetTextStyle(this in ImSliderStyle style) {
            return new ImTextStyle {
                FontSize  = style.FontSize,
                Padding   = style.Padding,
                TextColor = style.TextColor
            };
        }

        /// <summary>
        /// Sets the implicit ImButtonStyle to the ImSliderStyle.
        /// </summary>
        /// <param name="style">A reference to the button style.</param>
        /// <param name="slider">A reference to ImSliderStyle.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref ImSliderStyle WithButtonStyle(this ref ImSliderStyle slider, in ImButtonStyle style) {
            slider.ButtonDefault = style.Background;
            slider.ButtonHover   = style.Hover;
            slider.ButtonPressed = style.Pressed;
            slider.TextColor     = style.Text;
            slider.FontSize      = style.FontSize;
            return ref slider;
        }
    }
}
