using System.Runtime.CompilerServices;
using InitialPrefabs.NimGui.Text;
using Unity.Mathematics;
using UnityEngine;

namespace InitialPrefabs.NimGui {

    /// <summary>
    /// Stores the font style.
    /// </summary>
    public partial struct ImTextStyle : IStyle {

        /// <summary>
        /// Default font size.
        /// </summary>
        public int FontSize;

        /// <summary>
        /// Column wise alignment.
        /// </summary>
        public HorizontalAlignment Column;

        /// <summary>
        /// Row wise alignment.
        /// </summary>
        public VerticalAlignment Row;

        /// <summary>
        /// Default text color.
        /// </summary>
        public Color32 TextColor;

        /// <summary>
        /// The amount of space between the current and next widget.
        /// </summary>
        public float2 Padding;

        /// <summary>
        /// Constructs a new instance of the ImTextStyle using default values from
        /// DefaultStyles.
        /// <see cref="InitialPrefabs.NimGui.DefaultStyles"/>
        /// </summary>
        /// <returns>An instance of ImTextStyle</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImTextStyle New() {
            return new ImTextStyle {
                FontSize  = DefaultStyles.DefaultFontSize,
                Column    = HorizontalAlignment.Center,
                Row       = VerticalAlignment.Center,
                TextColor = DefaultStyles.Text,
                Padding   = new float2(DefaultStyles.Padding.x, 0)
            };
        }
    }

    public static class ImTextStyleExtensions {
        
        /// <summary>
        /// Fluent API to set the color of the TextStyle.
        /// </summary>
        /// <param name="style">ImTextStyle to reference.</param>
        /// <param name="color">Color of the text.</param>
        /// <returns>An instance of the ImTextStyle.</returns>
        public static ref ImTextStyle WithColor(this ref ImTextStyle style, Color32 color) {
            style.TextColor = color;
            return ref style;
        }
    }
}
