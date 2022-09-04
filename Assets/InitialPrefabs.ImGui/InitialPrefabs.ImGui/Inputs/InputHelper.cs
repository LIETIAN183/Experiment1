using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
using InputMouse = UnityEngine.InputSystem.Mouse;
#elif ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine;
#endif

namespace InitialPrefabs.NimGui.Inputs {

    /// <summary>
    /// The LegacyInputHelper interfaces with Unity's old input system and 
    /// tracks mouse clicks and scroll wheel.
    /// </summary>
    public static class InputHelper {

        static Mouse Mouse;
        static InputText InputTextHelper;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        static bool isKeyboardRegistered = false;
#endif

        internal static void Initialize() {
            if (InputTextHelper.IsCreated()) {
                InputTextHelper.Dispose();
            }
            InputTextHelper = new InputText(10);
            Mouse = new Mouse { Click = Mouse.State.None };
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            for (int i = 0; i < loop.subSystemList.Length; ++i) {
                if (loop.subSystemList[i].type == typeof(PreUpdate)) {
                    loop.subSystemList[i].updateDelegate -= PreUpdate;
                    loop.subSystemList[i].updateDelegate += PreUpdate;
                }
            }

            PlayerLoop.SetPlayerLoop(loop);

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
             // Reset this for when Domain Reload is disabled
            isKeyboardRegistered = false;
            if (Keyboard.current != null)
            {
                Keyboard.current.onTextInput -= TextInput;
                Keyboard.current.onTextInput += TextInput;
                isKeyboardRegistered = true;
            }
#endif
        }

        internal static void Release() {
            if (InputTextHelper.IsCreated()) {
                InputTextHelper.Dispose();
            }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            Keyboard.current.onTextInput -= TextInput;
            isKeyboardRegistered = false;
#endif
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            for (int i = 0; i < loop.subSystemList.Length; ++i) {
                if (loop.subSystemList[i].type == typeof(PreUpdate)) {
                    loop.subSystemList[i].updateDelegate -= PreUpdate;
                }
            }

            PlayerLoop.SetPlayerLoop(loop);
        }

        static void PreUpdate() {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (!isKeyboardRegistered && Keyboard.current != null)
            {
                Keyboard.current.onTextInput -= TextInput;
                Keyboard.current.onTextInput += TextInput;
                isKeyboardRegistered = true;
            }

            var currentMouse = InputMouse.current;
            if (currentMouse == null) {
                return;
            }

            switch (Mouse.Click) {
                case Mouse.State.None:
                    if (currentMouse.leftButton.wasPressedThisFrame) {
                        Mouse.Click = Mouse.State.Down;
                    }
                    break;
                case Mouse.State.Down:
                    if (currentMouse.leftButton.isPressed) {
                        Mouse.Click = Mouse.State.Held;
                    }
                    break;
                case Mouse.State.Held:
                    if (currentMouse.leftButton.wasReleasedThisFrame) {
                        Mouse.Click = Mouse.State.Released;
                    }
                    break;
                case Mouse.State.Released:
                    Mouse.Click = Mouse.State.None;
                    break;
            }
            var position = currentMouse.position.ReadValue();
            Mouse.Position = new int2((int)position.x, (int)position.y);
            Mouse.ScrollDelta = currentMouse.scroll.ReadValue() * 1f / 50f;
            Mouse.IsScrolling = math.lengthsq(Mouse.ScrollDelta) > 0;
#elif ENABLE_LEGACY_INPUT_MANAGER
            var x = (int)Input.mousePosition.x;
            var y = (int)Input.mousePosition.y;

            Mouse.Position = new int2(x, y);

            // Ideally we need a state machine for the mouse clicks.
            switch (Mouse.Click) {
                case Mouse.State.None:
                    // From the none state we can only enter the Down state
                    if (Input.GetKeyDown(KeyCode.Mouse0)) {
                        Mouse.Click = Mouse.State.Down;
                    }
                    break;
                case Mouse.State.Down:
                    // From the down state we can only enter hold or released
                    if (Input.GetKey(KeyCode.Mouse0)) {
                        Mouse.Click = Mouse.State.Held;
                    }

                    if (Input.GetKeyUp(KeyCode.Mouse0)) {
                        Mouse.Click = Mouse.State.Released;
                    }
                    break;
                case Mouse.State.Held:
                    // We can only enter the release state
                    if (!Input.GetKey(KeyCode.Mouse0)) {
                        Mouse.Click = Mouse.State.Released;
                    }
                    break;
                case Mouse.State.Released:
                    // When we release, we must set this back to the None state.
                    Mouse.Click = Mouse.State.None;
                    break;
            }

            // Update the scroll wheel
            Mouse.ScrollDelta = Input.mouseScrollDelta;
            Mouse.IsScrolling = math.lengthsq(Mouse.ScrollDelta) > 0;
#endif
            InputTextHelper.Reset();
        }

        /// <summary>
        /// Returns a copy of the mouse struct. Any manipulations on the copy will 
        /// not be reflected.
        /// </summary>
        /// <returns>Copy of the mouse struct.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Mouse GetMouseState() => Mouse;

        /// <summary>
        /// Allows access to the InputTextHelper.
        /// </summary>
        /// <returns>A reference to the static InputTextHelper.</returns>
        public static ref InputText GetInputTextHelper() => ref InputTextHelper;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        static void TextInput(char c) {
            switch (c) {
                case '\b':
                    InputTextHelper.IsBackspaced = true;
                    break;
                case '\r':
                case '\n':
                    InputTextHelper.IsEntered = true;
                    break;
                default:
                    InputTextHelper.Inputs[InputTextHelper.Length++] = c;
                    break;
            }
        }
#endif
    }
}
