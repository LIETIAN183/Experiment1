using System;
using System.Runtime.CompilerServices;
using System.Text;
using InitialPrefabs.NimGui.Collections;
using Unity.Collections;
using UnityEngine;

namespace InitialPrefabs.NimGui.Inputs {
    
    /// <summary>
    /// Generic struct which stores keyboard inputs used for a textfield.
    /// </summary>
    public struct InputText : IDisposable {

        /// <summary>
        /// Did the user press backspace? This is a read only public property
        /// and is internally set.
        /// </summary>
        public bool IsBackspaced { readonly get; internal set; }

        /// <summary>
        /// Did the user press enter? This is a read only public property
        /// and is internally set.
        /// </summary>
        public bool IsEntered { readonly get; internal set; }

        internal UnsafeArray<char> Inputs;
        internal int Length;

        /// <summary>
        /// Construct a LegacyInputText given a max capacity.
        /// </summary>
        /// <param name="maxCapacity">The size of the buffer</param>
        public InputText(int maxCapacity) {
            Inputs = new UnsafeArray<char>(maxCapacity, Allocator.Persistent);
            IsBackspaced = false;
            IsEntered = false;
            Length = 0;
        }

        /// <summary>
        /// Resets the bump allocator's index, allowing reuse of the internal buffer.
        /// </summary>
        public void Reset() {
            Length = 0;
            IsBackspaced = false;
            IsEntered = false;
        }

        /// <summary>
        /// Checks if the LegacyInputText is initialized.
        /// </summary>
        public bool IsCreated() {
            return Inputs.IsCreated();
        }

        /// <summary>
        /// Release all internal memory allocated.
        /// </summary>
        public void Dispose() {
            if (Inputs.IsCreated()) {
                Inputs.Dispose();
            }
        }

        /// <summary>
        /// Returns a readonly array pointing to the internal memory allocated.
        /// </summary>
        public UnsafeArray<char>.ReadOnly GetReadOnlyInput() {
            unsafe {
                return new UnsafeArray<char>.ReadOnly(Inputs.Ptr, Length);
            }
        }
    }

    public static class InputTextExtensions {

#if ENABLE_LEGACY_INPUT_MANAGER
        /// <summary>
        /// Collect inputs for the frame. Internally, if the buffer has already been written 
        /// to, new inputs will not be accepted.
        /// </summary>
        /// <param name="inputText">A reference to the LegacyInputText</param>
        /// <returns>A reference to same the LegacyInputText modified</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref InputText Collect(this ref InputText inputText) {
            if (inputText.Length > 0) {
                return ref inputText;
            }

            int length = 0;
            bool isBackSpaced = false;
            bool isEntered = false;

            const char backspace = '\b';
            const char enter = '\n';
            const char ret = '\r';
            foreach (var c in Input.inputString) {
                if (c == backspace) {
                    isBackSpaced = true;
                    continue; 
                }

                if (c == enter || c == ret) {
                    isEntered = true;
                    continue;
                }

                inputText.Inputs[length++] = c;
            }

            inputText.IsBackspaced = isBackSpaced;
            inputText.IsEntered = isEntered;
            inputText.Length = length;
            return ref inputText;
        }
#endif

        /// <summary>
        /// Copies all the characters in the internal buffer to a StringBuilder.
        /// <remarks>
        /// The StringBuilder must be allocated by the developer with a MaxCapacity. If the 
        /// inputText exceeds the max capacity of the StringBuilder, then the inputText is not 
        /// appended to the StringBuilder.
        /// <example>
        /// <code>
        /// StringBuilder builder = new StringBuilder(32, 64);
        /// ImGui.TextField("TextEdit", builder);
        /// </code>
        /// </example>
        /// </remarks>
        /// </summary>
        /// <param name="inputText">A reference to the LegacyInputText</param>
        /// <param name="builder">The allocated StringBuilder to add to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendTo(this ref InputText inputText, StringBuilder builder) {
            UnsafeArray<char>.ReadOnly inputs = inputText.GetReadOnlyInput();
            int size = builder.Length + inputText.Length;

            for (int i = 0; i < inputs.Length && size < builder.MaxCapacity; ++i) {
                builder.Append(inputs[i]);
            }
        }
    }
}
