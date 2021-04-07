// using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
// 枚举
enum STATUS
{
    INACTIVE, ACTIVE, PAUSE
};
// TODO: 添加 CineMachine
public class UIControl : MonoBehaviour
{
    public Dropdown selectEq;
    public Button startBtn;
    // 实现 Pause/Continue 功能
    public Button StatusBtn;

    // 判断当前状态
    [ShowInInspector]
    STATUS currentStatus = STATUS.INACTIVE;
    public Button reloadBtn;
    public Slider progress;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        // 关联 DropDown
        selectEq.onValueChanged.AddListener(index => EqManger.Instance.folders = selectEq.options[index].text);// 下拉框选择后，赋值 folders 具体选择的地震
        selectEq.options = EqManger.Instance.EarthquakeFolders().Select(f => new Dropdown.OptionData(f)).ToList();//获取可选的地震，转换 IEnumerable<string> 为 List<Dropdown.OptionData>，显示在下拉框中

        // 关联 Start
        startBtn.onClick.AddListener(EqManger.Instance.StartEq);// 关联地震开始按钮
        EqManger.Instance.startEarthquake.AddListener(() => { progress.maxValue = EqManger.Instance.GetTime(); currentStatus = STATUS.ACTIVE; });// 设置 Slider 最大值,修改当前 Status 按钮状态

        // 关联 Status 按钮点击事件
        StatusBtn.onClick.AddListener(ChangeStatus);

        // 关联 ReLoad
        reloadBtn.onClick.AddListener(EqManger.Instance.ReLoad);// 关联重置场景按钮

        // 关联 Progress
        Counter.Instance.onValueChanged.AddListener(i => progress.value = i);// 更新 Slider 滑动条进度

        // 关联地震结束
        // TODO: UI 提示地震结束
        EqManger.Instance.endEarthquake.AddListener(Reset);

        // 设置 DropDown 初始值
        selectEq.value = -1;// 实际 value 值为 0，但是设置为 0 时， onValueChanged 不触发， EqManger folders 参数与此则会不对应
    }

    // 修改 Status Button
    void ChangeStatus()
    {
        switch (currentStatus)
        {
            case STATUS.INACTIVE:
                // TODO: Add Notification to Start First
                break;
            case STATUS.ACTIVE:
                currentStatus = STATUS.PAUSE;
                StatusBtn.GetComponentInChildren<Text>().text = "CONTINUE";
                Time.timeScale = 0;// 暂停
                startBtn.interactable = false;// 修改 Start 按钮状态
                Debug.Log("PAUSE");
                break;
            case STATUS.PAUSE:
                currentStatus = STATUS.ACTIVE;
                StatusBtn.GetComponentInChildren<Text>().text = "PAUSE";
                Time.timeScale = 1;// 继续
                startBtn.interactable = true;// 修改 Start 按钮状态
                break;
            default:
                break;
        }
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
                case "StatusBtn":
                    StatusBtn = button;
                    break;
                case "ReLoadBtn":
                    reloadBtn = button;
                    break;
                default:
                    break;
            }
        }
        progress = GetComponentInChildren<Slider>();
        progress.value = 0;
        progress.maxValue = 0;
        progress.interactable = false;
    }
}