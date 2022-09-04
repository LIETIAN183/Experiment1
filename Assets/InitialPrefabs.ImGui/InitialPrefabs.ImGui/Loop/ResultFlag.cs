using UnityEngine;

namespace InitialPrefabs.NimGui.Loop {

    /// <summary>
    /// State flags to determine how NimGui initialized.
    /// </summary>
    public enum ResultFlag {
        /// <summary>
        /// There were no errors when initializing NimGui.
        /// </summary>
        Success = 0,
        /// <summary>
        /// InitialPrefabs/SDF shader is missing.
        /// </summary>
        MissingShader = 1 << 0,
        /// <summary>
        /// The URP_ENABLED define is added into your project, but the Graphics Settings
        /// pipeline asset is unassigned.
        /// </summary>
        MissingPipeline = 1 << 1,

        /// <summary>
        /// You are using URP, but the ImGuiRenderFeaturePass is not added to your Renderer.
        /// </summary>
        MissingRenderPass = 1 << 2,
        
        /// <summary>
        /// The default font texture was not imported when importing the NimGui package.
        /// </summary>
        MissingFontTexture = 1 << 3,

        /// <summary>
        /// The default glyph asset was not imported when importing the NimGui package.
        /// </summary>
        MissingFontAsset = 1 << 4,
    }

    internal static class ResultFlagExtensions {

        static readonly string[] ErrorMessages = new string [] {
            "[ERROR] The SDF shader is missing, please make sure all assets from NimGui are imported and that the SDF Shader is in the Always Included Shader settings.",
            "[ERROR] You have enabled URP in NimGui, but have not assigned the Scriptable Render Pipeline Settings in your Project.",
            "[ERROR] Your Pipeline Asset is missing the ImGuiRenderFeaturePass.",
            "[ERROR] Please make sure that you imported the UbuntuMono-Regular_Atlas.png, this is the default fallback texture.",
            "[ERROR] Please make sure that you imported the UbuntuMono-Regular_Glyphs.asset, this is the default fallback font data."
        };
           
        /// <summary>
        /// After initializaing NimGui, we can check the state of the Result Flag and log any issues.
        /// </summary>
        /// <param name="flag">The ResultFlag state.</param>
        public static void LogResults(this ResultFlag flag) {
#if UNITY_EDITOR
            bool pauseEditor = false;
#endif
            for (int i = 0; i < 5; ++i) {
                var current = (ResultFlag)(1 << i);

                if ((current & flag) > 0) {
#if UNITY_EDITOR
                    pauseEditor = true;
#endif
                    Debug.LogError(ErrorMessages[i]);
                }
            }

#if UNITY_EDITOR
            if (pauseEditor) {
                Debug.LogError("Please exit play mode and fix all possible errors.");
                Debug.Break();
            }
#endif
        }
    }
}
