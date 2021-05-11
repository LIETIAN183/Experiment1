// using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Michsky.UI.ModernUIPack;

namespace Project.Deprecated
{
    // TODO: 添加 CineMachine
    // TODO: UI 可移动或隐藏
    public class ModernUIController : MonoBehaviour
    {
        public HorizontalSelector EqSelector;
        // StatusBtn 实现 Pause/Continue 功能
        public ButtonManager startBtn, pauseBtn, reloadBtn;
        // 判断 PauseBtn 应该显示 Pause 还是 Continue 
        bool pauseBtnFlag = false;
        public ProgressBar progress;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            pauseBtn.GetComponent<CanvasGroup>().interactable = false;
        }
        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            // 关联 HorizontalSelector
            EqSelector.selectorEvent.AddListener(index => EqManger.Instance.folder = EqSelector.itemList[index].itemTitle);// 选择后，赋值 folder 具体选择的地震
            EqSelector.itemList = EqManger.Instance.EarthquakeFolders().Select(f => new HorizontalSelector.Item(f)).ToList();//获取可选的地震，转换 IEnumerable<string> 为 List<Dropdown.OptionData>，显示在下拉框中
            EqSelector.SetupSelector();

            // StartBtn 地震开始
            startBtn.clickEvent.AddListener(() =>
                {
                    EqManger.Instance.StartEq();
                    progress.maxValue = EqManger.Instance.GetTime();
                    startBtn.GetComponent<CanvasGroup>().interactable = false;
                    pauseBtn.GetComponent<CanvasGroup>().interactable = true;
                });// 关联地震开始按钮

            // PauseBtn 暂停/继续
            pauseBtn.clickEvent.AddListener(ChangeStatus);

            // ReloadBtn 重新载入场景
            // 重置 Timescale 否则 Pause 状态下重置仿真无法重新开始
            reloadBtn.clickEvent.AddListener(() => { Time.timeScale = 1; EqManger.Instance.Reload(); });// 关联重置场景按钮

            // Progress 显示时间进度
            Counter.Instance.onValueChanged.AddListener(i => progress.currentValue = i);// 更新 Slider 滑动条进度

            // 关联地震结束
            // TODO: UI 提示地震结束
            EqManger.Instance.endEarthquake.AddListener(Reset);
        }

        // 修改 Pause Button
        // PauseBtnFlag false 时显示 Pause, true 时显示 Continue
        // 暂停导致 PauseButton 按钮动画播放不完全
        void ChangeStatus()
        {
            switch (pauseBtnFlag)
            {
                case false: // 从显示 Pause 转到显示 Continue
                    pauseBtn.buttonText = "Continue";
                    Time.timeScale = 0;// 暂停状态
                    break;
                case true: // 从显示 Continue 转到显示 Pause
                    pauseBtn.buttonText = "Pause";
                    Time.timeScale = 1;// 继续状态
                    break;
            }
            pauseBtnFlag = !pauseBtnFlag;
        }

        /// <summary>
        /// Reset is called when the user hits the Reset button in the Inspector's
        /// context menu or when adding the component the first time.
        /// </summary>
        void Reset()
        {
            EqSelector = GetComponentInChildren<HorizontalSelector>();
            // Button[] buttons = GetComponentsInChildren<Button>();
            foreach (var button in GetComponentsInChildren<ButtonManager>())
            {
                switch (button.name)
                {
                    case "StartBtn":
                        startBtn = button;
                        break;
                    case "PauseBtn":
                        pauseBtn = button;
                        break;
                    case "ReloadBtn":
                        reloadBtn = button;
                        break;
                    default:
                        break;
                }
            }
            progress = GetComponentInChildren<ProgressBar>();
            progress.currentValue = 0;
            progress.maxValue = 0;
        }
    }
}