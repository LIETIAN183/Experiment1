using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace InitialPrefabs.NimGui.Inputs {

    /// <summary>
    /// Stores information on what the mouse inputs are.
    /// </summary>
    public struct Mouse {

        /// <summary>
        /// Describes the current state of the mouse.
        /// </summary>
        public enum State {
            None     = 0,
            Down     = 1 << 0,
            Held     = 1 << 1,
            Released = 1 << 2
        }

        /// <summary>
        /// What position is the mouse on?
        /// </summary>
        public int2 Position;

        /// <summary>
        /// What is the current primary mouse button state?
        /// </summary>
        public State Click;

        /// <summary>
        /// What was the change in the scroll wheel?
        /// </summary>
        public float2 ScrollDelta;

        /// <summary>
        /// Is the scroll wheel actively used?
        /// </summary>
        public bool IsScrolling;

        /// <summary>
        /// Provide a human readable format of the text.
        /// </summary>
        /// <returns>An easily text readable statement of the mouse state.</returns>
        public override string ToString() { 
            return $"Position: {Position}, MouseState: {Click}, Scroll Delta: {ScrollDelta}";
        }
    }

    public static class MouseExtensions {
        
        /// <summary>
        /// Extension function to check if the mouse is currently a state.
        /// </summary>
        /// <param name="mouse">Reference to the mouse struct</param>
        /// <param name="state">The current state we want to compare to</param>
        /// <returns>True, if the mouse state is the state we are looking for</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Is(this in Mouse mouse, Mouse.State state) {
            return mouse.Click == state;
        }

        /// <summary>
        /// Extension function to check if the mouse is any of the following states.
        /// <seealso cref="InitialPrefabs.NimGui.Inputs.Mouse.State"/>
        /// </summary>
        /// <param name="mouse">Reference to the mouse struct</param>
        /// <param name="state">The current state we want to compare to</param>
        /// <returns>True, if the mouse state is at least one of the state we are looking for</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAny(this in Mouse mouse, Mouse.State state) {
            return (mouse.Click & state) > 0;
        }
    }
}
