using InitialPrefabs.NimGui.Inputs;
using InitialPrefabs.NimGui.Render;
using InitialPrefabs.NimGui.Text;
using UnityEngine;

namespace InitialPrefabs.NimGui.Loop
{

    [AddComponentMenu("")]
    internal class DefaultImGuiInitializationProxy : MonoBehaviour
    {

        ResultFlag flag;

        void OnEnable()
        {
            // -----------------------------------------------------------------
            // Load the texture && glyphs
            // -----------------------------------------------------------------
            Texture2D tex = Resources.Load<Texture2D>("FontData/UbuntuMono/UbuntuMono-Regular_Atlas");
            SerializedFontData fontDataAsset = Resources.Load<SerializedFontData>("FontData/UbuntuMono/UbuntuMono-Regular_Glyphs");
            // Texture2D tex = Resources.Load<Texture2D>("Font/TimesNewRoman_Atlas");
            // SerializedFontData fontDataAsset = Resources.Load<SerializedFontData>("Font/TimesNewRoman_Glyphs");

            if (tex == null)
            {
                flag |= ResultFlag.MissingFontTexture;
            }

            if (fontDataAsset == null)
            {
                flag |= ResultFlag.MissingFontAsset;
            }

            flag |= ImGuiRenderUtils.CheckDependency();
            flag |= ImGuiContext.CheckDependency();

            if (flag != ResultFlag.Success)
            {
                flag.LogResults();
                return;
            }

            // -----------------------------------------------------------------
            // Initialize the context
            // -----------------------------------------------------------------
            ImGuiRenderUtils.Initialize(tex, fontDataAsset);
            ImGuiContext.Initialize();
            ImGuiRunner.Initialize();
            InputHelper.Initialize();
            Resources.UnloadAsset(fontDataAsset);
        }

        void OnDisable()
        {
            if (flag != ResultFlag.Success)
            {
                return;
            }

            // -----------------------------------------------------------------
            // Complete all running jobs
            // -----------------------------------------------------------------
            ImGuiRunner.Dependency.Complete();

            // -----------------------------------------------------------------
            // Release all native memory allocated
            // -----------------------------------------------------------------
            ImGuiRunner.Release();
            ImGuiContext.Release();
            ImGuiRenderUtils.Release();
            InputHelper.Release();
        }
    }
}
