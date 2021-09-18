using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine.SceneManagement;
using BansheeGz.BGDatabase;

// TODO: Display Acc
public class ECSUIController : MonoBehaviour
{
    public static ECSUIController Instance { get; private set; }
    public HorizontalSelector EqSelector;
    // StatusBtn 实现 Pause/Continue 功能
    public ButtonManager startBtn, pauseBtn, reloadBtn, exitBtn, analysisBtn, exportBtn;
    // 判断 PauseBtn 应该显示 Pause 还是 Continue
    bool pauseBtnFlag = false;
    public ProgressBar progress;

    public NotificationManager notification;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
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

    public void Setup()
    {
        pauseBtn.GetComponent<CanvasGroup>().interactable = false;

        // TODO： 修复 Reload 卡顿
        reloadBtn.GetComponent<CanvasGroup>().interactable = false;

        // 关联 HorizontalSelector 数据
        EqSelector.itemList = GetNameList();//获取可选的地震，转换 IEnumerable<string> 为 List<Dropdown.OptionData>
        // 同步 Progress 最大值
        EqSelector.selectorEvent.AddListener((int index) => { progress.maxValue = SetupBlobSystem.gmBlobRefs[index].Value.gmArray.Length; });
        EqSelector.SetupSelector();
        EqSelector.ForwardClick();
        EqSelector.UpdateUI();

        // StartBtn 地震开始按钮
        startBtn.clickEvent.AddListener(() =>
            {
                // 获得选择的地震 Index. 开始仿真
                World.DefaultGameObjectInjectionWorld.GetExistingSystem<EnvInitialSystem>().Active(EqSelector.index);

                // 更新 Button 状态
                startBtn.GetComponent<CanvasGroup>().interactable = false;
                EqSelector.GetComponent<CanvasGroup>().interactable = false;
                pauseBtn.GetComponent<CanvasGroup>().interactable = true;

                ShowNotification("Start Simulation");
            });// 关联地震开始按钮

        // PauseBtn 暂停/继续按钮
        pauseBtn.clickEvent.AddListener(ChangeStatus);

        // ReloadBtn 重新载入场景按钮
        // FIXME
        // 场景相应的 Lighting Setting 设置 Auto Generate, 否则 Reload 后光照存在问题
        reloadBtn.clickEvent.AddListener(() =>
        {
            // 关闭 System
            World.DefaultGameObjectInjectionWorld.GetExistingSystem<GroundMotionSystem>().Enabled = false;
            // 重置场景
            var entityManager = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;
            entityManager.DestroyEntity(entityManager.UniversalQuery);
            // 不能使用 SceneManager.GetActiveScene().name 此方法返回 SubScene 的名字，而不是主场景的名字
            SceneManager.LoadScene("EqSimulation", LoadSceneMode.Single);
        });// 关联重置场景按钮

        // Exit Button
        exitBtn.clickEvent.AddListener(System.Diagnostics.Process.GetCurrentProcess().Kill);

        // Analysis Button


        // Export Button
        exportBtn.clickEvent.AddListener(BGExcelImportGo.Instance.Export);
    }

    // 修改 Pause Button
    // PauseBtnFlag false 时显示 Pause, true 时显示 Continue
    // TODO: 暂停导致 PauseButton 按钮动画播放不完全
    // TODO: TImeScale 方法在 ECS 场景中是否适用存疑
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
        // 更新 ButtonManger, 否则修改 Text 不生效，因为该 ButtonManger 存在两个 Text
        pauseBtn.UpdateUI();
        pauseBtnFlag = !pauseBtnFlag;
        // pauseBtn.buttonText = "Continue";
        // Debug.Log(pauseBtn.buttonText);
    }

    // 从 BlobAsset 中获得所有地震的名字列表
    List<HorizontalSelector.Item> GetNameList()
    {
        List<HorizontalSelector.Item> nameList = new List<HorizontalSelector.Item>();
        foreach (var item in SetupBlobSystem.gmBlobRefs)
        {
            string name = item.Value.gmName.ToString();
            nameList.Add(new HorizontalSelector.Item(name));
        }
        return nameList;
    }

    /// <summary>
    /// Reset is called when the user hits the Reset button in the Inspector's
    /// context menu or when adding the component the first time.
    /// 重置函数
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
                case "ExitBtn":
                    exitBtn = button;
                    break;
                default:
                    break;
            }
        }
        progress = GetComponentInChildren<ProgressBar>();
        progress.currentValue = 0;
        progress.maxValue = 0;
        pauseBtn.GetComponent<CanvasGroup>().interactable = false;
        notification = GetComponentInChildren<NotificationManager>();
    }

    public void ShowNotification(string message)
    {
        notification.title = message;
        notification.UpdateUI();
        notification.OpenNotification();
    }
}