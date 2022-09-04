using InitialPrefabs.NimGui.Collections;
using System;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;

namespace InitialPrefabs.NimGui.Text {

    /// <summary>
    /// Create a persistent buffer which stores all characters requested. This 
    /// is a bump allocator and the data must be reset each frame.
    /// </summary>
    public unsafe struct ImWords : IDisposable {

        /// <summary>
        /// What is the maximum number of characters that the 
        /// bump allocator can store?
        /// </summary>
        public readonly int Capacity;

        [NativeDisableUnsafePtrRestriction]
        internal char* Ptr;
        internal int Index;

        readonly Allocator allocator;

        /// <summary>
        /// Create a persistent buffer which stores all characters requested.
        /// </summary>
        /// <param name="maxChars">The maximum # of characters we can store.</param>
        public ImWords(int maxChars) {
            Ptr = (char*)UnsafeUtility.Malloc(
                sizeof(char) * maxChars, UnsafeUtility.AlignOf<char>(), Allocator.Persistent);

            Capacity  = maxChars;
            allocator = Allocator.Persistent;
            Index     = 0;
        }

        /// <summary>
        /// Copies the contents of the string into the internal buffer and returns a 
        /// ReadOnlyString that points to the memory's contents.
        /// </summary>
        /// <param name="text">The string to copy over</param>
        /// <returns>A readonly string from the Words buffer.</returns>
        public ImString Request(string text) {
            var marker = new ProfilerMarker("Copy_Text_To_Buffer");
            marker.Begin();
            // TODO: Check thread index - since this is not a multithreaded friendly request.
            AccessUtility.CheckAvailableSize(text.Length, Capacity - Index);

            char* head = Ptr + Index;

            fixed (char* sPtr = text) {
                UnsafeUtility.MemCpy(head, sPtr, sizeof(char) * text.Length);
            }

            // Increment the index so we don't overwrite the contents.
            Index += text.Length;

            marker.End();
            return new ImString(head, text.Length);
        }

        /// <summary>
        /// Copies the contents of the string into the internal buffer and returns a 
        /// ReadOnlyString that points to the memory's contents.
        /// </summary>
        /// <param name="builder">The StringBuilder to read from.</param>
        /// <returns>A readonly string from the Words buffer.</returns>
        public ImString Request(StringBuilder builder) {
            int length = builder.Length;
            AccessUtility.CheckAvailableSize(length, Capacity - Index);
            char* head = Ptr + Index;

            for (int i = 0; i < length; ++i) {
                head[i] = builder[i];
            }

            Index += length;
            return new ImString(head, length);
        }

        /// <summary>
        /// Copies the contents of the string into the internal buffer and returns a 
        /// ReadOnlyString that points to the memory's contents.
        /// </summary>
        /// <param name="builder">The StringBuilder to read from.</param>
        /// <param name="startIndex">The first index of the character of the slice.</param>
        /// <returns>A readonly string from the Words buffer.</returns>
        public ImString Request(StringBuilder builder, ushort startIndex) {
            int length = builder.Length - startIndex;
            AccessUtility.CheckAvailableSize(length, Capacity - Index);
            char* head = Ptr + Index;

            for (int i = 0; i < length; ++i) {
                head[i] = builder[i + startIndex];
            }

            Index += length;
            return new ImString(head, length);
        }

        /// <summary>
        /// Returns an "empty" string with the requested size.
        /// </summary>
        /// <param name="size">The # of characters to request</param>
        /// <returns>A readonly "empty" string from the Words buffer.</returns>
        public ImString Request(int size) {
            AccessUtility.CheckAvailableSize(size, Capacity - Index);
            char* head = Ptr + Index;
            // Increment the index so we don't overwrite the contents.
            Index += size;

            return new ImString(head, size);
        }

        /// <summary>
        /// Returns a string with a single character.
        /// </summary>
        /// <param name="c">The character, to request into the TextBuffer</param>
        /// <returns>A string with the character.</returns>
        public ImString Request(char c) {
            AccessUtility.CheckAvailableSize(1, Capacity - Index);
            char* head = Ptr + Index;
            // Increment the index so we don't overwrite the contents.
            Index++;

            *head = c;
            return new ImString(head, 1);
        }

        /// <summary>
        /// Resets the internal pointer to the buffer. This allows the buffer to be reused multiple 
        /// times without reallocating new memory.
        /// </summary>
        public void Reset() {
            Index = 0;
        }
        
        /// <summary>
        /// Frees the allocated fixed buffer.
        /// </summary>
        public void Dispose() {
            if (Ptr != null) {
                UnsafeUtility.Free(Ptr, allocator);
                Ptr = null;
            }
        }
    }
}
