using UnityEngine;
#if !URP_ENABLED
using UnityEngine.Rendering;
#endif

namespace InitialPrefabs.NimGui.Loop {

    public static class DefaultImGuiInitialization {

#if !IMGUI_MANUAL_INIT
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Setup() {
            // -------------------------------------------------------------------------------
            // Construct a GameObject with Hide Flags, so people can't mess with it
            // -------------------------------------------------------------------------------
            var go = new GameObject("ImGui Runner Proxy") {
                hideFlags = HideFlags.HideInHierarchy
            };

            // -------------------------------------------------------------------------------
            // Add the component which will run ImGui
            // -------------------------------------------------------------------------------
            go.AddComponent<DefaultImGuiInitializationProxy>();

            // -------------------------------------------------------------------------------
            // Ensure the gameObject is not destroyed, so we always have an instance running.
            // Cause we can't contain constructors/destructors into the main game loop. :(
            // -------------------------------------------------------------------------------
            Object.DontDestroyOnLoad(go);
        }
#endif

#if !URP_ENABLED
        /// <summary>
        /// If using the builtin render pipeline, this will attach a command buffer
        /// to your target camera. This will additionally attach a component to the
        /// Camera's gameObject called <see cref="CameraScheduleUtil">, which
        /// is respsonbile for invoking a coroutine to clear the command buffer at the
        /// very end of the frame.
        /// <remarks>
        /// If you are using the Universal Render Pipeline you will typically not
        /// need to setup a camera directly!
        /// </remarks>
        /// </summary>
        /// <param name="camera">The camera to handle rendering the UI.</param>
        /// <param name="evt">The event at which the UI will draw.</param>
        public static void SetupCamera(Camera camera, CameraEvent evt) {
            camera.AddCommandBuffer(evt, ImGuiContext.BuiltInCommandBuffer);
        }

        /// <summary>
        /// Cleans up the target camera of the command buffer responsible
        /// for drawing UI.
        /// </summary>
        /// <param name="camera">The camera that handles rendering the UI.</param>
        /// <param name="evt">The event at which UI is currently drawn at.</param>
        public static void TearDownCamera(Camera camera, CameraEvent evt) {
            if (camera != null) {
                camera.RemoveCommandBuffer(evt, ImGuiContext.BuiltInCommandBuffer);
            }
        }
#endif
    }
}
