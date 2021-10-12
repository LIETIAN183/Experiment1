using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Michsky.UI.ModernUIPack;


public class InputManger : MonoBehaviour
{
    public GameObject UIInterface;

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        // 按 H 键隐藏UI界面
        if (Input.GetKeyUp(KeyCode.H))
        {
            UIInterface.SetActive(!UIInterface.activeInHierarchy);
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
