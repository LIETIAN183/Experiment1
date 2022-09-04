using System.Runtime.CompilerServices;
using InitialPrefabs.NimGui.Text;
using Unity.Mathematics;
using UnityEngine;

namespace InitialPrefabs.NimGui {

    /// <summary>
    /// Stores the color states of the button.
    /// </summary>
    public partial struct ImButtonStyle : IStyle {

        /// <summary>
        /// The default color.
        /// </summary>
        public Color32 Background;

        /// <summary>
        /// The color when the mouse is hovered over the button.
        /// </summary>
        public Color32 Hover;

        /// <summary>
        /// The color when the mouse is clicked on the button.
        /// </summary>
        public Color32 Pressed;

        /// <summary>
        /// The color of the text.
        /// </summary>
        public Color32 Text;

        /// <summary>
        /// Size of the text.
        /// </summary>
        public int FontSize;

        /// <summary>
        /// The spacing between the button and the next widget.
        /// </summary>
        public float2 Padding;

        /// <summary>
        /// Column-wise alignment.
        /// </summary>
        public HorizontalAlignment Column;

        /// <summary>
        /// Row-wise alignment.
        /// </summary>
        public VerticalAlignment Row;

        /// <summary>
        /// Constructs a new instance of the ButtonStyle with default settings.
        /// </summary>   
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImButtonStyle New() {
            return new ImButtonStyle {
                Background = DefaultStyles.Default,
                Hover      = DefaultStyles.Hover,
                Pressed    = DefaultStyles.Pressed,
                Text       = DefaultStyles.Text,
                FontSize   = DefaultStyles.DefaultFontSize,
                Padding    = DefaultStyles.Padding,
                Column     = HorizontalAlignment.Center,
                Row        = VerticalAlignment.Center
            };
        }
    }

    public static class ImButtonStyleExtensions {

        /// <summary>
        /// Gets the implicit ImTextStyle from the ImButtonStyle.
        /// </summary>
        /// <param name="buttonStyle">The button style to reference.</param>
        /// <returns>An ImButtonStyle</returns>       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImTextStyle GetTextStyle(this in ImButtonStyle buttonStyle) {
            return new ImTextStyle {
                FontSize  = buttonStyle.FontSize,
                TextColor = buttonStyle.Text,
                Padding   = buttonStyle.Padding,
                Column    = buttonStyle.Column,
                Row       = buttonStyle.Row,
            };
        }
    }
}
