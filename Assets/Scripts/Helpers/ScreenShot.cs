using UnityEngine;
using Unity.Entities;

public class ScreenShot : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.L))
        {
            var debugtype = World.DefaultGameObjectInjectionWorld.GetExistingSystem<FlowFieldDebugSystem>()._curDisplayType;

            ScreenCapture.CaptureScreenshot(Application.streamingAssetsPath + "/" + debugtype.ToString() + "_" + Time.time + ".png");
        }
    }

    // 此方法无法读取 UI，放弃
    // RenderPipelineManager.endFrameRendering += (context, camera) =>
    // {
    //     OnPostRender();
    // };
    // void OnPostRender()
    // {
    //     _camera.targetTexture = RenderTexture.GetTemporary(1920, 1080, 24);
    //     RenderTexture renderTexture = _camera.targetTexture;
    //     RenderTexture.active = renderTexture;
    //     Texture2D renderResult = new Texture2D(1920, 1080, TextureFormat.ARGB4444, false);
    //     // yield return new WaitForEndOfFrame();
    //     renderResult.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
    //     renderResult.Apply();
    //     _camera.targetTexture = null;
    //     RenderTexture.ReleaseTemporary(renderTexture);

    //     var name = Application.streamingAssetsPath + "/CameraScreenShot.png";
    //     File.WriteAllBytes(name, renderResult.EncodeToPNG());
    // }
}
