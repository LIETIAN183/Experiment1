using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace InitialPrefabs.NimGui {
    /// <summary>
    /// Defines the style of the skipped line.
    /// </summary>
    public partial struct ImSkipLineStyle : IStyle {

        /// <summary>
        /// The amount of spacing between the current the widget and 
        /// next widget along both the X and Y axis.
        /// </summary>
        public float2 Padding;

        /// <summary>
        /// The font sized used to define the amount of spacing to skip.
        /// </summary>
        public int FontSize;

        /// <summary>
        /// Constructs the default style of the skipped line using the DefaultStyles config.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImSkipLineStyle New() {
            return new ImSkipLineStyle {
                Padding = DefaultStyles.Padding,
                FontSize = DefaultStyles.DefaultFontSize
            };
        }
    }
}
