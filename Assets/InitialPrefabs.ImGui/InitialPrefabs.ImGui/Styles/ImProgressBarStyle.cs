using System.Runtime.CompilerServices;
using InitialPrefabs.NimGui.Text;
using Unity.Mathematics;
using UnityEngine;

namespace InitialPrefabs.NimGui {
    /// <summary>
    /// Defines the style of the progress bar.
    /// </summary>
    public partial struct ImProgressBarStyle : IStyle {
        public Color32 Background;
        public Color32 Foreground;

        public Color32 TextColor;
        public int FontSize;
        public HorizontalAlignment Column;
        public VerticalAlignment Row;

        internal float2 Padding;

        /// <summary>
        /// Constructs the default style of the ProgressBar using the DefaultStyles config.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImProgressBarStyle New() {
            return new ImProgressBarStyle {
                Background = DefaultStyles.Default,
                Foreground = DefaultStyles.Hover,
                TextColor  = DefaultStyles.Text,
                FontSize   = DefaultStyles.DefaultFontSize,
                Padding    = DefaultStyles.Padding
            };
        }
    }

    public static class ImProgressBarStyleExtensions {

        /// <summary>
        /// Gets the implicit text style from the ImProgressBarStyle.
        /// </summary>
        /// <param name="style">The progress bar's style.</param>
        /// <returns>ImTextStyle</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImTextStyle GetTextStyle(this in ImProgressBarStyle style) {
            return new ImTextStyle {
                Column    = style.Column,
                Row       = style.Row,
                FontSize  = style.FontSize,
                Padding   = style.Padding,
                TextColor = style.TextColor
            };
        }
    }

}
