using Unity.Mathematics;

namespace InitialPrefabs.NimGui {

    /// <summary>
    /// Stores the last known size and position of the scope we intend to layout.
    /// </summary>
    public struct ImScope {
        
        /// <summary>
        /// Stores the bounding box.
        /// </summary>
        public ImRect Rect;
        
        /// <summary>
        /// Stores the last known position.
        /// </summary>
        public float2 Previous;

        /// <summary>
        /// Stores the next position for the next element. The position
        /// is stored in screen space.
        /// </summary>
        public float2 Next;

        /// <summary>
        /// Stores the added size.
        /// </summary>
        public float2 Delta;

        /// <summary>
        /// Creates a scope where the Next position is upper left.
        /// </summary>
        /// <param name="rect">The bounds of the scope</param>
        /// <returns>A scope with the metadata generated</returns>
        public static ImScope Create(in ImRect rect) {
            var pos = rect.Position + rect.Extents * new float2(-1, 1);

            // TODO: Add padding
            return new ImScope { 
                Rect     = rect,
                Next     = pos,
                Previous = pos,
            };
        }
    }
}
