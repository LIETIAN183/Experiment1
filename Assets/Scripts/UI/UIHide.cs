using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: 重构
public class UIHide : MonoBehaviour
{
    public GameObject UIInterface;
    public ECSUIController controller;

    // Update is called once per frame
    void Update()
    {
        // GeyKeyUp 是主动查询是否有按键按下后弹起
        if (Input.GetKeyUp(KeyCode.H))
        {
            UIInterface.SetActive(!UIInterface.activeInHierarchy);
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {   // TODO: 判断 PauseBtn 状态
            controller.pauseBtn.clickEvent.Invoke();
        }
    }
}
