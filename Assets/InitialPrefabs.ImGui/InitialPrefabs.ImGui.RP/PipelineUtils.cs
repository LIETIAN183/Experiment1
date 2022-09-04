using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal {

#if URP_ENABLED
    /// <summary>
    /// Allows a ScriptableRendererFeature to grab a RenderPass.
    /// </summary>
    public interface IRenderFeature<T> where T : ScriptableRenderPass {
        T GetPass();
    }

    public static class PipelineUtils {

        /// <summary>
        /// Attempts to grab a render feature from the current assigned render pipeline asset.
        /// </summary>
        /// <param name="renderFeature">The render feature found in the Pipeline Asset.r</param>
        /// <returns>True, if found</returns>
        public static bool TryGetRendererAssets<T, U>(out T renderFeature) 
            where T : ScriptableRendererFeature, IRenderFeature<U> 
            where U : ScriptableRenderPass {
            UniversalRenderPipelineAsset urp = (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;

            if (urp == null) {
                renderFeature = null;
                return false;
            }

            foreach (var renderer in urp.m_RendererDataList) {
                List<ScriptableRendererFeature> features = renderer.m_RendererFeatures;

                foreach (ScriptableRendererFeature feature in features) {
                    T fValue = feature as T;
                    if (fValue != null) {
                        renderFeature = fValue;
                        return true;
                    }
                }
            }
            renderFeature = null;
            return false;
        }
    }
#endif
}
