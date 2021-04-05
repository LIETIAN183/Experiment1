using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO: 多分辨率适配
public class UIControl : MonoBehaviour
{
    public Dropdown selectEq;
    public Button startBtn;
    public Button endBtn;
    public Button reloadBtn;
    public Slider progress;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        // 关联 UI 和 EqManger via 事件
        selectEq.onValueChanged.AddListener(index => EqManger.Instance.folders = selectEq.options[index].text);// 下拉框选择后，赋值 earthquakes
        selectEq.options = EqManger.Instance.EarthquakeFolders().Select(f => new Dropdown.OptionData(f)).ToList();//获取可选的地震，转换 IEnumerable<string> 为 List<Dropdown.OptionData>，显示在下拉框中
        startBtn.onClick.AddListener(EqManger.Instance.StartEq);// 关联地震开始按钮
        startBtn.onClick.AddListener(() => progress.maxValue = EqManger.Instance.GetTime());// 设置 Slider 最大值
        endBtn.onClick.AddListener(EqManger.Instance.EndEq);// 关联地震结束按钮
        reloadBtn.onClick.AddListener(EqManger.Instance.ReLoad);// 关联重置场景按钮
        Counter.Instance.onValueChanged.AddListener(UpdateProgress);// 更新 Slider 滑动条进度

        selectEq.value = -1;// 实际 value 值为 0，但是设置为 0 时， onValueChanged 不触发， EqManger folders 参数与此则会不对应
    }
    public void UpdateProgress(int i)
    {
        progress.value = i;
    }

    /// <summary>
    /// Reset is called when the user hits the Reset button in the Inspector's
    /// context menu or when adding the component the first time.
    /// </summary>
    void Reset()
    {
        selectEq = GetComponentInChildren<Dropdown>();
        // Button[] buttons = GetComponentsInChildren<Button>();
        foreach (var button in GetComponentsInChildren<Button>())
        {
            switch (button.name)
            {
                case "StartBtn":
                    startBtn = button;
                    break;
                case "EndBtn":
                    endBtn = button;
                    break;
                case "ReLoadBtn":
                    reloadBtn = button;
                    break;
                default:
                    break;
            }
        }
        progress = GetComponentInChildren<Slider>();
    }
}