using System;
using System.Diagnostics;

namespace InitialPrefabs.NimGui.Collections {

    internal static class BoundsUtility {

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        public static void CheckMinMax<T>(T min, T max) where T : unmanaged, IComparable<T> {
            if (min.CompareTo(max) > 0) {
                throw new InvalidOperationException($"The min value: {min} is greater than: {max}.");
            }
        }
    }
}
