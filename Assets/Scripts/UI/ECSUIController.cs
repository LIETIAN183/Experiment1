using UnityEngine;
using Unity.Entities;
using Michsky.UI.ModernUIPack;
using BansheeGz.BGDatabase;
using System;
public class ECSUIController : MonoBehaviour
{
    public static ECSUIController Instance { get; private set; }
    public HorizontalSelector EqSelector;
    // StatusBtn 实现 Pause/Continue 功能
    public ButtonManager startBtn, exitBtn, analysisBtn, exportBtn;
    public ProgressBar progress;

    public CustomDropdown debugDropdown;

    public NotificationManager notification;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        // 单例模式判断
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Setup()
    {
        // 关联 HorizontalSelector 数据
        SetupBlobSystem.gmBlobRefs.ForEach(item => EqSelector.CreateNewItem(item.Value.gmName.ToString()));
        EqSelector.SetupSelector();
        // 同步 Progress 最大值
        EqSelector.selectorEvent.AddListener((int index) => { progress.maxTime = SetupBlobSystem.gmBlobRefs[index].Value.gmArray.Length * SetupBlobSystem.gmBlobRefs[index].Value.deltaTime; });
        // 使 EqSelector 显示第一个元素
        EqSelector.ForwardClick();
        EqSelector.UpdateUI();

        // StartBtn 地震开始按钮
        startBtn.clickEvent.AddListener(() =>
        {
            World.DefaultGameObjectInjectionWorld.GetExistingSystem<FullAnalysisSystem>().ProjectInit();
            // 获得选择的地震 Index. 开始仿真
            World.DefaultGameObjectInjectionWorld.GetExistingSystem<InitialSystem>().Active(EqSelector.index);

            // 更新 Button 状态
            startBtn.GetComponent<CanvasGroup>().interactable = false;
            EqSelector.GetComponent<CanvasGroup>().interactable = false;
        });

        // Analysis Button
        analysisBtn.clickEvent.AddListener(World.DefaultGameObjectInjectionWorld.GetExistingSystem<FullAnalysisSystem>().StartFullAnalysis);

        // Export Button
        exportBtn.clickEvent.AddListener(BGExcelImportGo.Instance.Export);

        // Dropdown
        foreach (var name in Enum.GetValues(typeof(FlowFieldDisplayType)))
        {
            debugDropdown.CreateNewItemFast(name.ToString(), null);
        }
        debugDropdown.SetupDropdown();
        debugDropdown.dropdownEvent.AddListener((int index) => { World.DefaultGameObjectInjectionWorld.GetExistingSystem<FlowFieldDebugSystem>()._curDisplayType = (FlowFieldDisplayType)index; });

        // Exit Button
        exitBtn.clickEvent.AddListener(System.Diagnostics.Process.GetCurrentProcess().Kill);
    }

    public void ShowNotification(string message)
    {
        notification.title = message;
        notification.UpdateUI();
        notification.OpenNotification();
    }
}