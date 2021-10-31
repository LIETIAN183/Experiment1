using UnityEngine;

public class ScreenShot : MonoBehaviour
{
    public Camera _camera;
    private float timer;

    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0) timer -= Time.deltaTime;
        else _camera.depth = -1;

        if (Input.GetKeyUp(KeyCode.L))
        {
            // 切换到截图摄像机
            _camera.depth = 10;
            ScreenCapture.CaptureScreenshot(Application.streamingAssetsPath + "\\Test.png");
            // 延迟反切换，切换太快则截图还是主 Camera 渲染的内容，上一句函数可能有延迟
            timer = 1;
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
