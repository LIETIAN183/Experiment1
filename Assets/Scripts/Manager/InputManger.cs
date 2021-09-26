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
    public ButtonManager pauseBtn;

    private bool pauseInteractivable;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
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
        if (pauseBtn.GetComponent<CanvasGroup>().interactable && Input.GetKeyUp(KeyCode.Space))
        {
            pauseBtn.clickEvent.Invoke();
        }
    }
}
