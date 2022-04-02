using Unity.Entities;
// using Unity.Physics;
using Unity.Mathematics;
// using BansheeGz.BGDatabase;
using Unity.Transforms;
// using UnityEngine;
// using Random = Unity.Mathematics.Random;
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

    public int cofficientBackup = 0;

    public int repeatCount = 0;

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
                // if (setting.cofficient >= 29 && repeatCount == 0)
                if (setting.index > 2)
                {
                    // BGExcelImportGo.Instance.Export();
                    this.Enabled = false;
                    ECSUIController.Instance.cofficientDisplay.text = "Simulation End";
                }
                else
                {
                    if (repeatCount > 0)
                    {
                        repeatCount--;
                    }
                    else
                    {
                        // cofficientBackup += 2;
                        // simulation.GetExistingSystem<AnalysisSystem>().GroupID = "400People";
                        if (setting.index == 0)
                        {
                            cofficientBackup = 29;
                            simulation.GetExistingSystem<AnalysisSystem>().GroupID = "North";
                        }
                        else if (setting.index == 1)
                        {
                            cofficientBackup = 79;
                            simulation.GetExistingSystem<AnalysisSystem>().GroupID = "Kobe";
                        }
                        else if (setting.index == 2)
                        {
                            cofficientBackup = 105;
                            simulation.GetExistingSystem<AnalysisSystem>().GroupID = "Imperial";
                        }

                        // if (cofficientBackup == 0)
                        // {
                        //     simulation.GetExistingSystem<AnalysisSystem>().GroupID = "OurModel.07g";
                        //     cofficientBackup = 7;
                        // }
                        // else if (cofficientBackup == 7)
                        // {
                        //     simulation.GetExistingSystem<AnalysisSystem>().GroupID = "OurModel_0.3g";
                        //     cofficientBackup = 29;
                        // }
                        // else if (cofficientBackup == 29)
                        // {
                        //     simulation.GetExistingSystem<AnalysisSystem>().GroupID = "OurModel_0.5g";
                        //     cofficientBackup = 48;
                        // }
                        // else
                        // {
                        //     cofficientBackup = 100;
                        // }
                        repeatCount = 0;
                    }
                    setting.cofficient = cofficientBackup;
                    //激活仿真
                    // setting.cofficient += 30;
                    setting.eqCount++;

                    DB_Eq newData = DB_Eq.NewEntity();
                    newData.F_eqIndex = setting.eqCount;
                    newData.F_eqName = SetupBlobSystem.gmBlobRefs[setting.index].Value.gmName.ToString() + "|" + (setting.cofficient / 100f).ToString();

                    World.DefaultGameObjectInjectionWorld.GetExistingSystem<InitialSystem>().Active(setting.index);
                    setting.index++;

                    // ECSUIController.Instance.cofficientDisplay.text = "Cofficient:" + (setting.cofficient / 100f).ToString();
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
        setting.cofficient = cofficientBackup;
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
        Entities.WithAll<ShakeData>().ForEach((ref ShakeData data, in LocalToWorld ltd) =>
        {
            data.k += x.NextFloat(-5, 5);
            data.c += x.NextFloat(-0.2f, 0.2f);
            data.forward = ltd.Forward;//math.normalize(math.mul(rotation.Value, math.forward()));
        }).ScheduleParallel();

        Entities.WithAll<SubShakeData>().ForEach((ref SubShakeData subShakeData, in Rotation rotation, in Translation translation) =>
         {
             subShakeData.originLocalPosition = translation.Value;
             subShakeData.originalRotation = rotation.Value;
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
            // data.reactionTimeVariable = NormalDistribution.RandomGaussianInRange(0.7f, 1.3f);
            data.reactionTimeVariable = 0;
            data.stepDuration = 0f;
        }).Run();

        bool flag = true;
        simulation.GetExistingSystem<AgentMovementSystem>().Enabled = flag;
        simulation.GetExistingSystem<SFMmovementSystem>().Enabled = !flag;
        simulation.GetExistingSystem<SFMmovementSystem2>().Enabled = !flag;
        simulation.GetExistingSystem<SFMmovementSystem3>().Enabled = !flag;

        simulation.GetExistingSystem<ComsShakeSystem>().Enabled = flag;
        simulation.GetExistingSystem<SubShakeSystem>().Enabled = flag;
        simulation.GetExistingSystem<ComsMotionSystem>().Enabled = flag;
        simulation.GetExistingSystem<ConstraintsSystem>().Enabled = flag;
    }
}
