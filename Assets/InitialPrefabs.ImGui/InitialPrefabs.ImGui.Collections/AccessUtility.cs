using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace InitialPrefabs.NimGui.Collections {

    internal static class AccessUtility {

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        internal static void CheckIndexOutOfRange(int index, int capacity) {
            if (index < 0) {
                throw new ArgumentOutOfRangeException($"Index {index} must be a value between [0..{capacity - 1}]");
            }

            if (index >= capacity) {
                throw new ArgumentOutOfRangeException($"Index {index} must not exceed the Capacity {capacity}");
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        internal static void CheckAvailableSize(int requestedSize, int capacity) {
            if (requestedSize > capacity) {
                throw new InvalidOperationException($"Requested size exceeds capacity. " +
                    $"Requesting: {requestedSize}, but only has {capacity} available.");
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        internal static void CheckExists<T>(int id, UnsafeParallelHashMap<int, T> container) where T : struct {
            if (!container.ContainsKey(id)) {
                throw new InvalidOperationException($"ID {id} does not exist.");
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        internal static void CheckExists<T>(int id, NativeParallelHashMap<int, T> container) where T : struct {
            if (!container.ContainsKey(id)) {
                throw new InvalidOperationException($"ID {id} does not exist.");
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        internal static void IsNotNull<T>(T value) {
            if (value == null) {
                throw new InvalidOperationException($"Value for type: {typeof(T)} cannot be null!");
            }
        }
    }
}
