using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine.SceneManagement;
using BansheeGz.BGDatabase;

public class ECSUIController : MonoBehaviour
{
    public static ECSUIController Instance { get; private set; }
    public HorizontalSelector EqSelector;
    // StatusBtn 实现 Pause/Continue 功能
    public ButtonManager startBtn, exitBtn, analysisBtn, exportBtn;
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
        // 关联 HorizontalSelector 数据
        EqSelector.itemList = GetNameList();//获取可选的地震，转换 IEnumerable<string> 为 List<Dropdown.OptionData>
        // 同步 Progress 最大值
        EqSelector.selectorEvent.AddListener((int index) => { progress.maxTime = SetupBlobSystem.gmBlobRefs[index].Value.gmArray.Length * SetupBlobSystem.gmBlobRefs[index].Value.deltaTime; });
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

                ShowNotification("Start Simulation");
            });// 关联地震开始按钮

        // Exit Button
        exitBtn.clickEvent.AddListener(System.Diagnostics.Process.GetCurrentProcess().Kill);

        // Analysis Button


        // Export Button
        exportBtn.clickEvent.AddListener(BGExcelImportGo.Instance.Export);
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
                case "ExitBtn":
                    exitBtn = button;
                    break;
                case "AnalysisBtn":
                    analysisBtn = button;
                    break;
                case "ExportBtn":
                    exportBtn = button;
                    break;
                default:
                    break;
            }
        }
        progress = GetComponentInChildren<ProgressBar>();
        progress.currentTime = 0;
        progress.maxTime = 0;
        notification = GetComponentInChildren<NotificationManager>();
    }

    public void ShowNotification(string message)
    {
        notification.title = message;
        notification.UpdateUI();
        notification.OpenNotification();
    }
}