using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using BansheeGz.BGDatabase;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class FullAnalysisSystem : SystemBase
{
    private int delayTime;
    private int timeCounter;

    protected override void OnCreate()
    {
        //延迟100帧 = 1s
        delayTime = 100;
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
                    ECSUIController.Instance.eqName.text = "Simulation End";
                }
                else
                {
                    //激活仿真
                    ECSUIController.Instance.UpdateEqDisplay(setting.index);
                    World.DefaultGameObjectInjectionWorld.GetExistingSystem<EnvInitialSystem>().Active(setting.index++);
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
        ProjectInit();
        var setting = GetSingleton<AnalysisTypeData>();
        setting.task = AnalysisTasks.Start;
        SetSingleton<AnalysisTypeData>(setting);
    }

    void ProjectInit()
    {
        Entities
       .WithAll<ComsTag>()
       .ForEach((ref ComsTag data, in LocalToWorld worldPosition) =>
       {
           if (worldPosition.Position.x > 3.5f)
           {
               data.groupID = 3;
           }
           else if (worldPosition.Position.x < 0.5f)
           {
               data.groupID = 1;
           }
           else
           {
               data.groupID = 2;
           }
       }).ScheduleParallel();

        // 直接修改物体质量 inversemass 会出错，不可行
    }
}
