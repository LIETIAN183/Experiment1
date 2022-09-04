using System.Runtime.CompilerServices;
using UnityEngine;

namespace InitialPrefabs.NimGui {

    /// <summary>
    /// Defines the style of the Line.
    /// </summary>
    public partial struct ImLineStyle : IStyle {

        /// <summary>
        /// The foreground color.
        /// </summary>
        public Color32 Color;
        
        /// <summary>
        /// The amount of spacing along the x axis. Unlike most styles, this 
        /// does not care about the the Y axis.
        /// </summary>
        public float Padding;

        /// <summary>
        /// Constructs the default style of the Line using the DefaultStyles config.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImLineStyle New() {
            return new ImLineStyle {
                Color   = DefaultStyles.Text,
                Padding = DefaultStyles.Padding.x * 2
            };
        }
    }
}
