using System.Runtime.CompilerServices;
using InitialPrefabs.NimGui.Text;
using Unity.Mathematics;
using UnityEngine;

namespace InitialPrefabs.NimGui {

    /// <summary>
    /// Stores the style of the TextField.
    /// </summary>
    public partial struct ImTextFieldStyle : IStyle {

        /// <summary>
        /// Default size of the text.
        /// </summary>
        public int FontSize;

        /// <summary>
        /// Default color of the text.
        /// </summary>
        public Color32 Text;

        /// <summary>
        /// Background color for the textfield.
        /// </summary>
        public Color32 Background;

        /// <summary>
        /// Color of the textfield when the mouse is over it.
        /// </summary>
        public Color32 Hover;

        /// <summary>
        /// Column wise alignment.
        /// </summary>
        public HorizontalAlignment Column;

        /// <summary>
        /// Row wise alignment.
        /// </summary>
        public VerticalAlignment Row;

        /// <summary>
        /// Spacing between the this and the next widget.
        /// </summary>
        public float2 Padding;

        /// <summary>
        /// Constructs a new ImTextFieldStyle using values from the DefaultStyles.
        /// <see cref="InitialPrefabs.NimGui.DefaultStyles"/>
        /// </summary>
        /// <returns>An instance of the ImTextFieldStyle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImTextFieldStyle New() {
            return new ImTextFieldStyle {
                FontSize   = DefaultStyles.DefaultFontSize,
                Background = DefaultStyles.Default,
                Padding    = DefaultStyles.Padding,
                Text       = DefaultStyles.Text,
                Hover      = DefaultStyles.TextFieldHover,
                Column     = HorizontalAlignment.Left,
                Row        = VerticalAlignment.Center,
            };
        }
    }

    public static class ImTextFieldStyleExtensions {

        /// <summary>
        /// Gets the implicit ImTextFieldStyle from the TextField.
        /// </summary>
        /// <param name="style">The ImTextFieldStyle reference.</param>
        /// <returns>An instance of ImTextFieldStyle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImTextStyle GetTextStyle(this in ImTextFieldStyle style) {
            return new ImTextStyle {
                Column    = style.Column,
                Row       = style.Row,
                FontSize  = style.FontSize,
                Padding   = style.Padding,
                TextColor = style.Text
            };
        }
        
        /// <summary>
        /// Gets the implicit ImButtonStyle from the TextField.
        /// </summary>
        /// <param name="style">The ImTextFieldStyle reference.</param>
        /// <returns>An instance of ImButtonStyle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImButtonStyle GetButtonStyle(this in ImTextFieldStyle style) {
            return new ImButtonStyle {
                Background = style.Background,
                Text       = style.Text,
                Padding    = style.Padding,
                FontSize   = style.FontSize,
                Hover      = style.Hover,
                Pressed    = Color.clear,
                Column     = style.Column,
                Row        = style.Row,
            };
        }
    }

}
