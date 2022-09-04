using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

[assembly: InternalsVisibleTo("InitialPrefabs.ImGui.Tests")]
namespace InitialPrefabs.NimGui.Collections {

    public interface IElement<T> where T : struct { }

    public unsafe struct UnsafeArray<T> : IDisposable, IElement<T> where T : unmanaged {

        public ref struct ReadOnly {

            public readonly int Length;

            [NativeDisableUnsafePtrRestriction]
            T* Ptr;

            public ReadOnly(T* ptr, int capacity) {
                Ptr = ptr;
                Length = capacity;
            }

            public ref T this[int index] {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get {
                    return ref UnsafeUtility.ArrayElementAsRef<T>(Ptr, index);
                }
            }
        }

        public readonly int Length;

        [NativeDisableUnsafePtrRestriction]
        internal T* Ptr;

        Allocator allocator;

        public UnsafeArray(int capacity, Allocator alloc) {
            allocator = alloc;
            Length = capacity;

            Ptr = (T*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<T>() * capacity, UnsafeUtility.AlignOf<T>(), alloc);
        }

        public void Dispose() {
            if (Ptr != null) {
                UnsafeUtility.Free(Ptr, allocator);
                Ptr = null;
            }
        }

        public bool IsCreated() {
            return Ptr != null;
        }

        public T this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                // AccessUtility.CheckIndexOutOfRange(index, Length);
                return *(Ptr + index);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                // AccessUtility.CheckIndexOutOfRange(index, Length);
                *(Ptr + index) = value;
            }
        }
    }
}
