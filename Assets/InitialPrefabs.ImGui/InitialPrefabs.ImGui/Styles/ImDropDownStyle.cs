using System.Runtime.CompilerServices;
using InitialPrefabs.NimGui.Text;
using Unity.Mathematics;
using UnityEngine;

namespace InitialPrefabs.NimGui {

    /// <summary>
    /// Defines the style of the DropDown.
    /// </summary>
    public struct ImDropDownStyle : IStyle {
        
        /// <summary>
        /// The default color.
        /// </summary>
        public Color32 Background;

        /// <summary>
        /// The color when the mouse is over the dropdown.
        /// </summary>
        public Color32 Hover;
        
        /// <summary>
        /// The color when the mouse clicks the dropdown.
        /// </summary>
        public Color32 Pressed;

        /// <summary>
        /// The color of text.
        /// </summary>
        public Color32 Text;

        /// <summary>
        /// Column-wise alignment.
        /// </summary>
        public HorizontalAlignment Column;

        /// <summary>
        /// Row-wise alignment.
        /// </summary>
        public VerticalAlignment Row;

        /// <summary>
        /// Size of the text.
        /// </summary>
        public int FontSize;

        /// <summary>
        /// Spacing between the dropdown and the next widget.
        /// </summary>
        public float2 Padding;

        /// <summary>
        /// Constructs the default DropdownStyle.
        /// </summary>
        /// <returns>The dropdown style using values from DefaultStyles.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImDropDownStyle New() {
            return new ImDropDownStyle {
                Background = DefaultStyles.Default,
                Hover      = DefaultStyles.Hover,
                Pressed    = DefaultStyles.Pressed,
                Text       = DefaultStyles.Text,
                FontSize   = DefaultStyles.DefaultFontSize,
                Padding    = DefaultStyles.Padding,
                Column     = HorizontalAlignment.Left,
                Row        = VerticalAlignment.Center
            };
        }
    }

    public static class ImDropDownStyleExtensions {
        
        /// <summary>
        /// Gets the associated ImButtonStyle.
        /// </summary>
        /// <param name="style">The dropdown style to reference.</param>
        /// <returns>Returns the style of the ImButtonStyle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImButtonStyle GetButtonStyle(this in ImDropDownStyle style) {
            return new ImButtonStyle {
                Background = style.Background,
                Hover = style.Hover,
                Pressed = style.Pressed,
                Text = style.Text,
                FontSize = style.FontSize,
                Padding = style.Padding
            };
        }

        /// <summary>
        /// Gets the associated ImTextStyle.
        /// </summary>
        /// <param name="style">The dropdown style to reference.</param>
        /// <returns>Returns the style of the ImTextStyle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImTextStyle GetTextStyle(this in ImDropDownStyle style) {
            return new ImTextStyle {
                FontSize = style.FontSize,
                TextColor = style.Text,
                Padding = style.Padding,
                Column = style.Column,
                Row = style.Row
            };
        }
    }
}
