using Unity.Entities;
// using Unity.Physics;
using Unity.Mathematics;
// using BansheeGz.BGDatabase;
using Unity.Transforms;
// using UnityEngine;
// using System;

// 程序一开始就运行该系统，放在 FixedStepSimulationSystemGroup 可能出现无法读到单例数据的错误
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
// [DisableAutoCreation]
public class FullAnalysisSystem : SystemBase
{
    // 延迟 1 秒
    private float delayTime = 1;
    private float timeCounter;

    World simulation;
    public bool flag = true;

    protected override void OnCreate()
    {
        simulation = World.DefaultGameObjectInjectionWorld;
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
                if (setting.cofficient >= 100)
                {
                    // BGExcelImportGo.Instance.Export();
                    this.Enabled = false;
                }
                else
                {
                    //激活仿真
                    setting.cofficient += 1;
                    setting.eqCount++;

                    DB_Eq newData = DB_Eq.NewEntity();
                    newData.F_eqIndex = setting.eqCount;
                    newData.F_eqName = SetupBlobSystem.gmBlobRefs[setting.index].Value.gmName.ToString() + "|" + (setting.cofficient / 100f).ToString();

                    World.DefaultGameObjectInjectionWorld.GetExistingSystem<InitialSystem>().Active(setting.index);
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
        setting.eqCount = 0;
        setting.index = 0;
        SetSingleton<AnalysisTypeData>(setting);
        this.Enabled = true;
    }

    public void ProjectInit()
    {
        // 保存货架初始数据
        Random x = new Random();
        x.InitState();
        Entities.WithAll<ShakeData>().ForEach((ref ShakeData data) =>
        {
            data.k += x.NextFloat(-5, 5);
            data.c += x.NextFloat(-0.1f, 0.1f);
        }).ScheduleParallel();

        Entities.WithAll<SubShakeData>().ForEach((ref SubShakeData subShakeData, in Rotation rotation, in Translation translation) =>
         {
             subShakeData.originLocalPosition = translation.Value;
         }).ScheduleParallel();

        // 保存商品初始数据
        Entities.WithAll<ComsData>().ForEach((ref ComsData data, in Translation translation, in Rotation rotation) =>
        {
            data.originPosition = translation.Value;
            data.originRotation = rotation.Value;
        }).ScheduleParallel();

        // 保存智能体初始数据
        Entities.WithAll<AgentMovementData>().ForEach((ref AgentMovementData data, in Translation translation) =>
        {
            data.originPosition = translation.Value;
            data.reactionTimeVariable = NormalDistribution.RandomGaussianInRange(0.7f, 1.3f);
        }).Run();

        simulation.GetExistingSystem<AgentMovementSystem>().Enabled = flag;
        simulation.GetExistingSystem<SFMmovementSystem>().Enabled = !flag;

        simulation.GetExistingSystem<ComsShakeSystem>().Enabled = flag;
        simulation.GetExistingSystem<SubShakeSystem>().Enabled = flag;
        simulation.GetExistingSystem<ComsMotionSystem>().Enabled = flag;
    }
}
