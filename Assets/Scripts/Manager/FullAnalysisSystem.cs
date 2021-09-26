using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using BansheeGz.BGDatabase;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public class FullAnalysisSystem : SystemBase
{
    private int delayTime;
    private int timeCounter;

    protected override void OnCreate()
    {
        //延迟30帧
        delayTime = 30;
    }

    protected override void OnUpdate()
    {
        var setting = GetSingleton<AnalysisTypeData>();
        // 获取地震总数
        if (setting.eqCount == 0)
        {
            setting.eqCount = SetupBlobSystem.gmBlobRefs.Count;
        }

        // 开始仿真逻辑
        if (setting.task == AnalysisTasks.Start)
        {
            // 延迟30帧执行代码
            if (timeCounter < delayTime)
            {
                timeCounter++;
            }
            else
            {
                timeCounter = 0;
                setting.task = AnalysisTasks.Idle;
                if (setting.index == setting.eqCount)
                {
                    BGExcelImportGo.Instance.Export();
                }
                else
                {
                    //激活仿真

                    ECSUIController.Instance.EqSelector.index = setting.index;
                    ECSUIController.Instance.EqSelector.selectorEvent.Invoke(setting.index++);
                    ECSUIController.Instance.EqSelector.UpdateUI();
                    ECSUIController.Instance.startBtn.clickEvent.Invoke();
                    // World.DefaultGameObjectInjectionWorld.GetExistingSystem<EnvInitialSystem>().Active(setting.index++);
                }

            }
        }

        // 重置场景
        if (setting.task == AnalysisTasks.Reload)
        {
            if (timeCounter < delayTime)
            {
                timeCounter++;
            }
            else
            {
                timeCounter = 0;
                setting.task = AnalysisTasks.Start;
                World.DefaultGameObjectInjectionWorld.GetExistingSystem<ReloadSystem>().Enabled = true;
            }
        }

        SetSingleton<AnalysisTypeData>(setting);
    }

    public void StartFullAnalysis()
    {
        var setting = GetSingleton<AnalysisTypeData>();
        setting.task = AnalysisTasks.Start;
        SetSingleton<AnalysisTypeData>(setting);
    }
}
