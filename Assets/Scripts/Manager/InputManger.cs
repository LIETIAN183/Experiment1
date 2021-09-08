using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Michsky.UI.ModernUIPack;


// FIXME: Build 后 Right Bar 无反应，可能和该脚本有关
// 无法正常读取地震数据 
public class InputManger : MonoBehaviour
{
    public GameObject UIInterface;

    private ButtonManager pauseBtn;

    private bool pauseInteractivable;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        foreach (var button in UIInterface.GetComponentsInChildren<ButtonManager>())
        {
            if (button.name.Equals("PauseBtn"))
            {
                pauseBtn = button;
            }
        }
    }

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

        // 当 PauseBtn 激活时，按 Space 暂停仿真
        pauseInteractivable = pauseBtn.GetComponent<CanvasGroup>().interactable;

        if (pauseInteractivable && Input.GetKeyUp(KeyCode.Space))
        {   // TODO: 判断 PauseBtn 状态
            pauseBtn.clickEvent.Invoke();
        }
    }
}
