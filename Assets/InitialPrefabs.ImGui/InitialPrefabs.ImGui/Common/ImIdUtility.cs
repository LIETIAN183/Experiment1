using System.Runtime.CompilerServices;

namespace InitialPrefabs.NimGui.Common {

    /// <summary>
    /// The ImId class is a utility to get an unsigned integer as a control ID. 
    /// </summary>
    public static class ImIdUtility {
        static uint ID = 1;

        /// <summary>
        /// Returns the current integer and increments so the next time RequestId() is called,
        /// the next positive integer is returned.
        /// </summary>
        /// <returns>The current unsigned integer cached</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RequestId() {
            var id = ID;
            ID += 1;
            return id;
        }

        /// <summary>
        /// Internal function used to reset the cached ID back to 1 so the next frame 
        /// can reuse the same ID.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Reset() {
            ID = 1;
        }
    }
}
