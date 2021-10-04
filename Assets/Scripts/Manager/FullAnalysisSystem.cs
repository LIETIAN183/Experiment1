using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using BansheeGz.BGDatabase;
using Unity.Transforms;
using UnityEngine;
using System;

// 程序一开始就运行该系统，放在 FixedStepSimulationSystemGroup 可能出现无法读到单例数据的错误
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class FullAnalysisSystem : SystemBase
{
    // 延迟 1 秒
    private float delayTime = 1;
    private float timeCounter;

    protected override void OnCreate()
    {
        this.Enabled = false;
    }

    protected override void OnUpdate()
    {
        var setting = GetSingleton<AnalysisTypeData>();
        // 获取地震总数
        // if (setting.eqCount == 0)
        // {
        //     setting.eqCount = SetupBlobSystem.gmBlobRefs.Count;
        // }

        // 开始仿真逻辑
        if (setting.task == AnalysisTasks.Start)
        {
            // 延迟30帧执行代码
            if (timeCounter < delayTime)
            {
                timeCounter += Time.DeltaTime;
            }
            else
            {
                timeCounter = 0;
                setting.task = AnalysisTasks.Idle;
                // if (setting.index == setting.eqCount)
                if (setting.cofficient >= 1)
                {
                    // BGExcelImportGo.Instance.Export();
                    ECSUIController.Instance.eqName.text = "Simulation End";
                }
                else
                {
                    //激活仿真
                    setting.cofficient += 0.01f;

                    DB_Eq newData = DB_Eq.NewEntity();
                    newData.F_eqIndex = Convert.ToInt32(setting.cofficient * 100);
                    newData.F_eqName = SetupBlobSystem.gmBlobRefs[setting.index].Value.gmName.ToString() + "|" + setting.cofficient.ToString();

                    ECSUIController.Instance.UpdateEqDisplay(setting.index);
                    World.DefaultGameObjectInjectionWorld.GetExistingSystem<AccTimerSystem>().Active(setting.index);

                }
            }
        }

        // 重置场景
        if (setting.task == AnalysisTasks.Reload)
        {
            if (timeCounter < delayTime)
            {
                timeCounter += Time.DeltaTime;
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
        setting.cofficient = 0;
        SetSingleton<AnalysisTypeData>(setting);
        this.Enabled = true;
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

        Entities.WithAll<SubShakeData>().ForEach((ref SubShakeData subShakeData, in Rotation rotation, in Translation translation) =>
         {
             subShakeData.originLocalPosition = translation.Value;
         }).ScheduleParallel();

        Entities.WithAll<ComsTag>().ForEach((ref ComsTag data, in Translation translation, in Rotation rotation) =>
       {
           data.originPosition = translation.Value;
           data.originRotation = rotation.Value;
       }).ScheduleParallel();

        // 直接修改物体质量 inversemass 会出错，不可行
    }
}
