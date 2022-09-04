using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace InitialPrefabs.NimGui.Text {

    /// <summary>
    /// Unsafe representation of a string. 
    /// <remarks>
    /// The struct does not implement an IDisposable interface because 
    /// the purpose of the struct is to "borrow" a reference to a 
    /// string's pointer. This is typically used in conjunection with 
    /// ImWords or a fixed string.
    /// </remarks>
    /// <seealso cref="InitialPrefabs.NimGui.Text.ImWords"/>
    /// </summary>
    public unsafe struct ImString {

        [NativeDisableUnsafePtrRestriction]
        internal char* Ptr;

        public readonly ushort Length;

        public ref char this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return ref UnsafeUtility.ArrayElementAsRef<char>(Ptr, index); }
        }

        /// <summary>
        /// Pins a string using a fixed statement and stores the pointer to the head 
        /// and stores the string's length.
        /// </summary>
        public ImString(string contents) {
            fixed (char* ptr = contents) {
                Ptr = ptr;
            }
            Length = (ushort)contents.Length;
        }

        /// <summary>
        /// Allows passing a pointer and length. This is generally used in conjunction with 
        /// the TextBuffer.
        /// </summary>
        public ImString(char* ptr, int length) {
            Ptr = ptr;
            Length = (ushort)length;
        }

        public override string ToString() {
            char* c = stackalloc char[Length];
            UnsafeUtility.MemCpy(c, Ptr, sizeof(char) * Length);
            return new string(c, 0, Length);
        }
    }

    public static class ImStringExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void AppendClose(this ref ImString dst, in ImString src) {
            UnsafeUtility.MemCpy(dst.Ptr, src.Ptr, sizeof(char) * src.Length);

            fixed (char* closePtr = "_X") {
                UnsafeUtility.MemCpy(dst.Ptr + src.Length, closePtr, sizeof(char) * 2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void AppendCollapse(this ref ImString dst, in ImString src) {
            UnsafeUtility.MemCpy(dst.Ptr, src.Ptr, sizeof(char) * src.Length);

            fixed (char* collapsedPtr = "_☰ ") {
                UnsafeUtility.MemCpy(dst.Ptr + src.Length, collapsedPtr, sizeof(char) * 2);
            }
        }
    }
}
