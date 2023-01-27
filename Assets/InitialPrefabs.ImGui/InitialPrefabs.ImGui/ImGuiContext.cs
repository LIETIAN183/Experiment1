using InitialPrefabs.NimGui.Collections;
using InitialPrefabs.NimGui.Loop;
using InitialPrefabs.NimGui.Render;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

#if URP_ENABLED
using UnityEngine.Rendering.Universal;
#endif

namespace InitialPrefabs.NimGui
{

    public static unsafe partial class ImGuiContext
    {

        // ------------------------------------------------------------------------
        // Store multiple windows, by default 1 is generated automatically
        // ------------------------------------------------------------------------
        internal static readonly List<ImWindow> Windows = new List<ImWindow>();

        // ------------------------------------------------------------------------
        // Static content must be initialized manually and released manually
        // ------------------------------------------------------------------------
#if URP_ENABLED
        internal static ImGuiRenderFeature ImGuiRenderFeature;
#else
        internal static CommandBuffer BuiltInCommandBuffer = new CommandBuffer() { name = "ImGui.Builtin" };
        internal static MaterialPropertyBlock BuiltinPropertyBlock = new MaterialPropertyBlock();
#endif

        internal static ResultFlag CheckDependency()
        {
            ResultFlag flag = ResultFlag.Success;
#if URP_ENABLED
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp == null)
            {
                flag |= ResultFlag.MissingPipeline;
            }

            if (!PipelineUtils.TryGetRendererAssets<ImGuiRenderFeature, ImGuiRenderPass>(
                out ImGuiRenderFeature))
            {
                flag |= ResultFlag.MissingRenderPass;
            }
#else
            BuiltinPropertyBlock.SetFloat("_LinearCorrection", QualitySettings.activeColorSpace == ColorSpace.Linear ? 1 : 0);
#endif
            return flag;
        }

        internal static void Initialize()
        {
            Windows.Clear();
            // ------------------------------------------------------------------------
            // Initialize the first window to be the screen's width/height
            // ------------------------------------------------------------------------
#if UNITY_EDITOR
            Debug.Log($"<color=yellow>Initializing Primary_Window.</color>");
#endif
            var size = new int2(Screen.width, Screen.height);
            var position = (float2)size / 2f;
            Windows.Add(new ImWindow(50, size, position, "Primary_Window"));

#if URP_ENABLED
            // ------------------------------------------------------------------------
            // Grab the URP Asset
            // ------------------------------------------------------------------------
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp == null)
            {
            }

            if (!PipelineUtils.TryGetRendererAssets<ImGuiRenderFeature, ImGuiRenderPass>(
                out ImGuiRenderFeature))
            {
            }
#endif
            // TODO: Write an HDRP define logic
        }

        internal static void Release()
        {
#if UNITY_EDITOR
            Debug.Log($"<color=yellow>Releasing all windows.</color>");
#endif
            // ------------------------------------------------------------------------
            // Dispose all draw data from the windows
            // ------------------------------------------------------------------------
            foreach (var window in Windows)
            {
                window.Dispose();
            }
            Windows.Clear();
        }

        /// <summary>
        /// If for some reason we are tracking multiple windows, this allows you to reference and
        /// iterate through windows.
        /// </summary>
        public static ReadOnlyCollection<ImWindow> All()
        {
            return new ReadOnlyCollection<ImWindow>(Windows);
        }

        /// <summary>
        /// Returns the first Window stored.
        /// </summary>
        public static ImWindow GetCurrentWindow()
        {
            return Windows[0];
        }
    }
}
