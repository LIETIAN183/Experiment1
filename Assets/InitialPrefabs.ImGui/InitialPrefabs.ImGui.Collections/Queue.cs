using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace InitialPrefabs.NimGui.Collections {

    /// <summary>
    /// A readonly queue that allows peek and dequeue.
    /// </summary>
    public ref struct StackAllocManagedQueue<T> {

        public readonly int Count => Collection.Count;

        internal IList<T> Collection;
        internal int Index;

        public StackAllocManagedQueue(IList<T> source) {
            Collection = source;
            Index = 0;
        }

        public T Peek() {
            AccessUtility.CheckIndexOutOfRange(Index, Count);
            return Collection[Index];
        }

        public T Dequeue() {
            T value = Peek();
            Index++;
            return value;
        }

        public bool IsEmpty() {
            return Index >= Count;
        }
    }

    /// <summary>
    /// A ReadOnly Queue that allows you to peek and dequeue. This only 
    /// stores blittable data.
    /// </summary>
    public unsafe struct ReadOnlyQueue<T> where T : unmanaged {

        [NativeDisableUnsafePtrRestriction]
        internal T* Ptr;
        internal int Index;

        public readonly int Count;

        public ReadOnlyQueue(T* ptr, int count) {
            Ptr = ptr;
            Index = 0;
            Count = count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Peek() {
            AccessUtility.CheckIndexOutOfRange(Index, Count);
            return Ptr[Index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Dequeue() {
            return ref UnsafeUtility.ArrayElementAsRef<T>(Ptr, Index++);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty() {
            return Index >= Count;
        }
    }
}
