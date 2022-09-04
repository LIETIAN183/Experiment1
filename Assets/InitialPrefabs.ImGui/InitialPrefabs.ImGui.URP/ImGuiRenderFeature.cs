using UnityEngine;
using UnityEngine.Rendering;
#if URP_ENABLED
using UnityEngine.Rendering.Universal;
#endif

namespace InitialPrefabs.NimGui.Render {
#if URP_ENABLED
    public class ImGuiRenderPass : ScriptableRenderPass {

        public CommandBuffer DrawCommand { get; private set; }
        public MaterialPropertyBlock PropertyBlock { get; private set; }

        public ImGuiRenderPass() {
            DrawCommand     = CommandBufferPool.Get("ImGui Render Pass");
            PropertyBlock   = new MaterialPropertyBlock();
            renderPassEvent = RenderPassEvent.AfterRendering;

            PropertyBlock.SetFloat("_LinearCorrection", QualitySettings.activeColorSpace == ColorSpace.Linear ? 1 : 0);
        }

        public ImGuiRenderPass(RenderPassEvent renderEvent) : this() {
            renderPassEvent = renderEvent;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
#if UNITY_EDITOR
            if (renderingData.cameraData.isSceneViewCamera || renderingData.cameraData.isPreviewCamera) {
                return;
            }
#endif

            if (renderingData.cameraData.isDefaultViewport &&
                renderingData.cameraData.camera.isActiveAndEnabled) {

                // The render pass will only execute the command buffer
                // See the ImGuiRender class which will clear the command buffer
                context.ExecuteCommandBuffer(DrawCommand);
            }
        }
    }

    public class ImGuiRenderFeature : ScriptableRendererFeature, IRenderFeature<ImGuiRenderPass> {

        public RenderPassEvent Event;

        public ImGuiRenderPass scriptablePass;

        /// <inheritdoc/>
        public override void Create() {
            scriptablePass = new ImGuiRenderPass(Event);
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            renderer.EnqueuePass(scriptablePass);
        }

        public ImGuiRenderPass GetPass() {
            return scriptablePass;
        }
    }
#endif
}
