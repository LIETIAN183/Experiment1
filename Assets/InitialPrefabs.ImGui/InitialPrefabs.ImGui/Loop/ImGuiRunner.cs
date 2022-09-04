using InitialPrefabs.NimGui.Common;
using InitialPrefabs.NimGui.Render;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;

#if URP_ENABLED
using UnityEngine.Rendering.Universal;
#endif

namespace InitialPrefabs.NimGui.Loop {

    internal static class ImGuiRunner {

        internal static JobHandle Dependency;

        internal static void Initialize() {
            var loop = PlayerLoop.GetCurrentPlayerLoop();

            for (int i = 0; i < loop.subSystemList.Length; ++i) {
                if (loop.subSystemList[i].type == typeof(PreUpdate)) {
                    loop.subSystemList[i].updateDelegate -= ScheduleDraw;
                    loop.subSystemList[i].updateDelegate += ScheduleDraw;
                }

                if (loop.subSystemList[i].type == typeof(PostLateUpdate)) {
                    loop.subSystemList[i].updateDelegate -= Build;
                    loop.subSystemList[i].updateDelegate += Build;
                }
            }

            PlayerLoop.SetPlayerLoop(loop);
        }

        internal static void Release() {
            Dependency.Complete();
            Dependency = default;

            var loop = PlayerLoop.GetCurrentPlayerLoop();
            for (int i = 0; i < loop.subSystemList.Length; ++i) {
                if (loop.subSystemList[i].type == typeof(PreUpdate)) {
                    loop.subSystemList[i].updateDelegate -= ScheduleDraw;
                }

                if (loop.subSystemList[i].type == typeof(PostLateUpdate)) {
                    loop.subSystemList[i].updateDelegate -= Build;
                }
            }
            PlayerLoop.SetPlayerLoop(loop);
        }

        internal static void ScheduleDraw() {
            // -----------------------------------------------------------
            // Complete previous jobs
            // -----------------------------------------------------------
            Dependency.Complete();

#if UNITY_EDITOR && URP_ENABLED
            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp == null) {
                return;
            }

            if (!PipelineUtils.TryGetRendererAssets<ImGuiRenderFeature, ImGuiRenderPass>(out var rendererFeature)) {
                return;
            }
#endif

            // -----------------------------------------------------------
            // Reset some contexts
            // -----------------------------------------------------------
            ImIdUtility.Reset();

#if URP_ENABLED
            var pass = ImGuiContext.ImGuiRenderFeature.GetPass();
            var commandBuffer = pass.DrawCommand;
            var propertyBlock = pass.PropertyBlock;
#else
            var commandBuffer = ImGuiContext.BuiltInCommandBuffer;
            var propertyBlock = ImGuiContext.BuiltinPropertyBlock;
#endif

            commandBuffer.Clear();

            // -----------------------------------------------------------
            // Set the view projection matrix
            // -----------------------------------------------------------
            commandBuffer.SetViewProjectionMatrices(
                Matrix4x4.identity,
                Matrix4x4.Ortho(0, Screen.width, 0, Screen.height, -1, 1));

            var all = ImGuiContext.All();
            for (int i = 0; i < all.Count; ++i) {
                var window = all[i];

                window.GenerateMeshInternal();
                window.Draw(commandBuffer, propertyBlock);

                // Clear our remaining temp vertex/index/submesh data & context data
                window.ResetContext();
            }
        }

        internal static void Build() {
            var all = ImGuiContext.All();

            for (int i = 0; i < all.Count; ++i) {
                var window = all[i];

                window.ResetTempMeshData();
                window.Prepare();
#if UNITY_WEBGL
                window.RunBuild();
#else
                Dependency = JobHandle.CombineDependencies(
                    window.ScheduleBuild(Dependency),
                    Dependency);
#endif
            }
        }
    }
}
