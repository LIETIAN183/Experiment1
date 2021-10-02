using Unity.Entities;
using UnityEngine;


public class InputManger : MonoBehaviour
{
    public static InputManger Instance { get; private set; }

    public bool simlationStatus = false;

    void Awake()
    {
        // 单例模式判断
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 切换场景时不销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.P) && !simlationStatus)
        {
            World.DefaultGameObjectInjectionWorld.GetExistingSystem<EnvInitialSystem>().Active(0);
            simlationStatus = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Time.timeScale = Time.timeScale == 0 ? 1 : 0;
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            World.DefaultGameObjectInjectionWorld.GetExistingSystem<ReloadSystem>().Enabled = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
