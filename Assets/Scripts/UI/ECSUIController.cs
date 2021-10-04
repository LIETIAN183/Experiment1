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

    public ButtonManager analysisBtn, exportBtn;
    public ProgressBar progress;
    public Text eqName;


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
        // Analysis Button
        analysisBtn.clickEvent.AddListener(World.DefaultGameObjectInjectionWorld.GetExistingSystem<FullAnalysisSystem>().StartFullAnalysis);
        // Export Button
        exportBtn.clickEvent.AddListener(BGExcelImportGo.Instance.Export);
    }

    /// <summary>
    /// Reset is called when the user hits the Reset button in the Inspector's
    /// context menu or when adding the component the first time.
    /// 重置函数
    /// </summary>
    void Reset()
    {
        foreach (var button in GetComponentsInChildren<ButtonManager>())
        {
            switch (button.name)
            {
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
    }

    public void UpdateEqDisplay(int index)
    {
        eqName.text = SetupBlobSystem.gmBlobRefs[index].Value.gmName.ToString();
        progress.maxTime = SetupBlobSystem.gmBlobRefs[index].Value.gmArray.Length * SetupBlobSystem.gmBlobRefs[index].Value.deltaTime;
    }
}