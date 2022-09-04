using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace InitialPrefabs.NimGui.Collections {

    public static unsafe class CollectionExtensions {

        public static StackAllocManagedQueue<T> AsQueue<T>(this IList<T> collection) {
            return new StackAllocManagedQueue<T>(collection);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeArray<T>.ReadOnly AsReadOnly<T>(this in UnsafeArray<T> collection) where T : unmanaged {
            return new UnsafeArray<T>.ReadOnly(collection.Ptr, collection.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Last<T>(this in UnsafeList<T> collection) where T : unmanaged {
            var lastIdx = collection.Length - 1;
            return collection.ElementAt(lastIdx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe static T* LastPtr<T>(this ref UnsafeList<T> collection) where T : unmanaged {
            var lastIdx = collection.Length - 1;
            return collection.Ptr + lastIdx;
        }

        public static int BinarySearch<T, U>(this in UnsafeArray<T> collection, T value, U comp) 
            where T : unmanaged where U : IComparer<T> {
            return NativeSortExtension.BinarySearch(collection.Ptr, collection.Length, value, comp);
        }

        public static ReadOnlyQueue<T> AsReadOnlyQueue<T>(this in UnsafeList<T> collection) where T : unmanaged {
            return new ReadOnlyQueue<T>(collection.Ptr, collection.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T ElementAt<T>(this ref NativeArray<T> array, int i) where T : unmanaged {
            unsafe {
                return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), i);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Update<T, U>(this ref UnsafeParallelHashMap<T, U> map, T key, U item) 
            where T : unmanaged, IEquatable<T>
            where U : unmanaged {

            if (!map.TryAdd(key, item)) {
                map[key] = item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T ElementAt<T>(this ref UnsafeArray<T> array, int index) where T : unmanaged {
            return ref UnsafeUtility.ArrayElementAsRef<T>(array.Ptr, index);
        }
    }
}
